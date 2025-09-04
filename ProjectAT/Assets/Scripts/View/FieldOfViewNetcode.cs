using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class FieldOfViewNetcode : NetworkBehaviour
{
    private struct ViewCastInfo
    {
        public bool Hit;
        public Vector3 Point;
        public float Distance;
        public float Angle;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            Hit = hit;
            Point = point;
            Distance = distance;
            Angle = angle;
        }
    }

    private struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }

    [SerializeField]
    private float fixedRadius;
    public float FixedRadius => fixedRadius;

    [SerializeField]
    private float viewRadius;
    public float ViewRadius => viewRadius;

    [SerializeField, Range(0f, 360f)]
    private float viewAngle = 90f;
    public float ViewAngle => viewAngle;

    [SerializeField]
    private LayerMask targetMask;
    [SerializeField]
    private LayerMask obstacleMask;

    [SerializeField]
    private float scanTime = 5f;

    private List<Transform> visibleTargets = new List<Transform>();
    public IReadOnlyList<Transform> VisibleTargets => visibleTargets;

    [SerializeField]
    private float meshReolution;
    [SerializeField]
    private int edgeResolveIteration;
    [SerializeField]
    private float edgeDistanceThreshold;

    [SerializeField]
    private MeshFilter fixedMeshFilter;
    [SerializeField]
    private MeshFilter viewmeshFilter;

    private Mesh fixedMesh;
    private Mesh viewMesh;

    private NetworkVariable<bool> isDetected = new NetworkVariable<bool>(false);

    private Coroutine growingCoroutine;

    public event Action onScanCompleted;
    public event Action onScanStarted;
    public event Action onScanCanceled;

    private void Awake()
    {
        fixedMesh = new Mesh { name = "Full Mesh" };
        viewMesh = new Mesh { name = "View Mesh" };

        fixedMeshFilter.mesh = fixedMesh;
        viewmeshFilter.mesh = viewMesh;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(ServerDetectLoop());

        isDetected.OnValueChanged += OnIsDetected;
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
            isDetected.OnValueChanged -= OnIsDetected;
    }

    private void OnIsDetected(bool previous, bool current)
    {
        if (current) StartScan();
        else CancelScan();
    }

    private IEnumerator GrowingCoroutine()
    {
        yield return AnimateRadiusCoroutine(fixedRadius);

        viewRadius = 0f;

        // 부채꼴 차오르는 로직이 클라에서 작동함!
        // 이 이벤틀를 이용해서 Enemy STate가 Attck으로 바꿔야함!


        // 서버에 있는 Enemy FixedUpdate 같은 곳에서 Player 탐지!
        // 플레이어 있음 -> 자신의 경계 수치 차오름
        // 경계 수치가 NetworkVariable로 하면 항상 동기화 됨

        // 그러면 클라에서 메시 차오르는 건 경계심 수치를 이용해서
        // 여기서 말한 경계심 수치는 부채꼴 차오르는 정도..!

        if (IsServer)
            onScanCompleted?.Invoke();

        growingCoroutine = null;
    }

    private IEnumerator ShrinkingCoroutine()
    {
        yield return AnimateRadiusCoroutine(0f);

        if (IsServer)
            onScanCanceled?.Invoke();

        growingCoroutine = null;
    }

    private IEnumerator AnimateRadiusCoroutine(float targetRadius)
    {
        float time = 0f;
        float startRadius = viewRadius;
        float journey = Mathf.Abs(targetRadius - startRadius);

        float duration = scanTime * (journey / fixedRadius);

        if (duration <= 0f) yield break;

        while (time <= duration)
        {
            viewRadius = Mathf.Lerp(startRadius, targetRadius, time / duration);
            DrawFieldOfView(viewMesh, viewRadius);
            DrawFieldOfView(fixedMesh, fixedRadius);

            time += Time.deltaTime;
            yield return null;
        }

        viewRadius = targetRadius;
        DrawFieldOfView(viewMesh, viewRadius);

        ClearMesh();
    }

    private IEnumerator ServerDetectLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            bool detectedNow = CheckDetectedServer();

            if (isDetected.Value != detectedNow)
                isDetected.Value = detectedNow;

            yield return wait;
        }
    }

    private void StartScan()
    {
        if (growingCoroutine != null) StopCoroutine(growingCoroutine);

        if (IsServer)
            onScanStarted?.Invoke();

        growingCoroutine = StartCoroutine(GrowingCoroutine());
    }

    private void CancelScan()
    {
        if (growingCoroutine != null)
        {
            StopCoroutine(growingCoroutine);

            growingCoroutine = StartCoroutine(ShrinkingCoroutine());
        }
    }

    private void ClearMesh()
    {
        if (viewMesh.vertexCount > 0) viewMesh.Clear();
        if (fixedMesh.vertexCount > 0) fixedMesh.Clear();
    }

    private bool CheckDetectedServer()
    {
        visibleTargets.Clear();

        Collider[] cols = Physics.OverlapSphere(transform.position, fixedRadius, targetMask);

        for (int i = 0; i < cols.Length; ++i)
        {
            Transform t = cols[i].transform;
            Vector3 dir = (t.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dir) < viewAngle * 0.5f)
            {
                float dst = Vector3.Distance(transform.position, t.position);

                if (!Physics.Raycast(transform.position, dir, dst, obstacleMask))
                    visibleTargets.Add(t);
            }
        }

        return visibleTargets.Count > 0;
    }

    private void DrawFieldOfView(Mesh mesh, float radius)
    {
        int stepCount = Mathf.Max(1, Mathf.RoundToInt(viewAngle * meshReolution));
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>(stepCount);
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; ++i)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle, radius);

            if (i > 0)
            {
                bool tooFar = Mathf.Abs(oldViewCast.Distance - newViewCast.Distance) > edgeDistanceThreshold;

                if (oldViewCast.Hit != newViewCast.Hit || (oldViewCast.Hit && newViewCast.Hit && tooFar))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast, radius);

                    if (edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                }
            }

            viewPoints.Add(newViewCast.Point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; ++i)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void DrawSimpleWedge()
    {
        int stepCount = Mathf.Max(1, Mathf.RoundToInt(viewAngle * meshReolution));
        float stepAngleSize = viewAngle / stepCount;

        int vertexCount = stepCount + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= stepCount; ++i)
        {
            float localAng = -viewAngle * 0.5f + stepAngleSize * i;
            float rad = localAng * Mathf.Deg2Rad;

            Vector3 localDir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            vertices[i + 1] = localDir * viewRadius;

            if (i < stepCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast, float radius)
    {
        float minAngle = minViewCast.Angle;
        float maxAngle = maxViewCast.Angle;
        Vector3 minPt = default;
        Vector3 maxPt = default;

        for (int i = 0; i < edgeResolveIteration; ++i)
        {
            float angle = (minAngle + maxAngle) / 2f;
            ViewCastInfo newViewCast = ViewCast(angle, radius);
            bool tooFar = Mathf.Abs(minViewCast.Distance - newViewCast.Distance) > edgeDistanceThreshold;

            if (newViewCast.Hit == minViewCast.Hit && !tooFar)
            {
                minAngle = angle;
                minPt = newViewCast.Point;
            }
            else
            {
                maxAngle = angle;
                maxPt = newViewCast.Point;
            }
        }

        return new EdgeInfo(minPt, maxPt);
    }

    private ViewCastInfo ViewCast(float globalAngle, float radius)
    {
        Vector3 dir = DirFromGlobalAngle(globalAngle);
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, radius, obstacleMask))
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);

        return new ViewCastInfo(false, transform.position + dir * radius, radius, globalAngle);
    }

    private Vector3 DirFromGlobalAngle(float deg)
    {
        return new Vector3(Mathf.Sin(deg * Mathf.Deg2Rad), 0f, Mathf.Cos(deg * Mathf.Deg2Rad));
    }

    public Vector3 DirFromLocalAngle(float angleInDegrees)
    {
        angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0f, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
