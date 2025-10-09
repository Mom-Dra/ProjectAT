using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public struct ViewCastInfo
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
public struct EdgeInfo
{
    public Vector3 PointA;
    public Vector3 PointB;

    public EdgeInfo(Vector3 pointA, Vector3 pointB)
    {
        PointA = pointA;
        PointB = pointB;
    }
}

public class FieldOfView : MonoBehaviour
{
    [SerializeField]
    private float viewRadius;
    public float ViewRadius => viewRadius;

    [SerializeField]
    private LayerMask targetMask;
    [SerializeField]
    private LayerMask obstacleMask;

    [SerializeField]
    private float defaultScanTime = 5f;
    [SerializeField]
    private float detectInterval = 0.2f;
    private WaitForSeconds detectWait;

    private List<(Transform transform, float distance)> visibleTargets = new List<(Transform transform, float distance)>();
    public IReadOnlyList<(Transform transform, float distance)> VisibleTargets => visibleTargets;

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

    private Coroutine growingCoroutine;
    private Coroutine detectLoopCoroutine;

    public event System.Action onScanComplete;
    public event System.Action onScanStart;
    public event System.Action onScanCancel;

    private Collider[] colliders = new Collider[4];

    private Enemy enemy;

    private EnemyData enemyData;
    public float ViewAngle => enemyData.ViewAngle;

    public event System.Action onTargetDetect;
    public event System.Action onTargetLosted;

    private void Awake()
    {
        fixedMesh = new Mesh { name = "Full Mesh" };
        viewMesh = new Mesh { name = "View Mesh" };

        fixedMeshFilter.mesh = fixedMesh;
        viewmeshFilter.mesh = viewMesh;

        detectWait = new WaitForSeconds(detectInterval);
    }

    private void OnEnable()
    {
        StartDetectLoop();
    }

    private void OnDisable()
    {
        StopDetectLoop();
    }

    public void SetEnemyData(Enemy enemy)
    {
        Debug.Log("SetEnemyData");

        this.enemy = enemy;
        enemyData = enemy.EnemyData;
    }

    private void StartDetectLoop()
    {
        if (detectLoopCoroutine == null)
        {
            detectLoopCoroutine = StartCoroutine(ServerDetectLoop());
        }
    }

    private void StopDetectLoop()
    {
        if (detectLoopCoroutine != null)
        {
            StopCoroutine(detectLoopCoroutine);
            detectLoopCoroutine = null;
        }
    }

    private IEnumerator GrowingCoroutine()
    {
        float scanTime = defaultScanTime * (enemy.AlertData.CombatThreshold - enemy.AlertLevel) / enemy.AlertData.CombatThreshold;

        yield return AnimateRadiusCoroutine(enemyData.SecondaryViewRadius, scanTime);

        viewRadius = 0f;

        // 부채꼴 차오르는 로직이 클라에서 작동함!
        // 이 이벤틀를 이용해서 Enemy STate가 Attck으로 바꿔야함!


        // 서버에 있는 Enemy FixedUpdate 같은 곳에서 Player 탐지!
        // 플레이어 있음 -> 자신의 경계 수치 차오름
        // 경계 수치가 NetworkVariable로 하면 항상 동기화 됨

        // 그러면 클라에서 메시 차오르는 건 경계심 수치를 이용해서
        // 여기서 말한 경계심 수치는 부채꼴 차오르는 정도..!

        onScanComplete?.Invoke();

        growingCoroutine = null;
    }

    private IEnumerator ShrinkingCoroutine()
    {
        yield return AnimateRadiusCoroutine(0f, defaultScanTime);

        onScanCancel?.Invoke();
        growingCoroutine = null;
    }

    private IEnumerator AnimateRadiusCoroutine(float targetRadius, float scanTime)
    {
        float time = 0f;
        float startRadius = viewRadius;
        float journey = Mathf.Abs(targetRadius - startRadius);

        float duration = scanTime * (journey / enemyData.SecondaryViewRadius);

        if (duration < 0f) yield break;

        while (time <= duration)
        {
            viewRadius = Mathf.Lerp(startRadius, targetRadius, time / duration);
            DrawFieldOfView(viewMesh, viewRadius);
            DrawFieldOfView(fixedMesh, enemyData.SecondaryViewRadius);

            time += Time.deltaTime;
            yield return null;
        }

        viewRadius = targetRadius;
        DrawFieldOfView(viewMesh, viewRadius);

        ClearMesh();
    }

    private IEnumerator ServerDetectLoop()
    {
        int beforeDetectedCount = 0;
        bool isBeforeDetected = false;

        while (true)
        {
            bool detectedNow = CheckDetectedServer();

            if (visibleTargets.Count > beforeDetectedCount)
                onTargetDetect?.Invoke();
            else if (!detectedNow) onTargetLosted?.Invoke();

            if (detectedNow != isBeforeDetected)
            {
                isBeforeDetected = detectedNow;

                if (detectedNow) StartScan();
                else CancelScan();
            }

            beforeDetectedCount = visibleTargets.Count;

            yield return detectWait;
        }
    }

    private void StartScan()
    {
        if (growingCoroutine != null) StopCoroutine(growingCoroutine);

        onScanStart?.Invoke();

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

        int count = Physics.OverlapSphereNonAlloc(transform.position, enemyData.SecondaryViewRadius, colliders, targetMask);

        for (int i = 0; i < count; ++i)
        {
            Transform target = colliders[i].transform;
            Vector3 dir = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dir) < enemyData.ViewAngle * 0.5f)
            {
                float dst = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dir, dst, obstacleMask))
                    visibleTargets.Add((target, dst));
            }
        }

        return visibleTargets.Count > 0;
    }

    private void DrawFieldOfView(Mesh mesh, float radius)
    {
        int stepCount = Mathf.Max(1, Mathf.RoundToInt(enemyData.ViewAngle * meshReolution));
        float stepAngleSize = enemyData.ViewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>(stepCount);
        ViewCastInfo oldViewCast;

        // i = 0
        float firstAngle = transform.eulerAngles.y - enemyData.ViewAngle / 2;
        ViewCastInfo firstViewCast = ViewCast(firstAngle, radius);
        viewPoints.Add(firstViewCast.Point);
        oldViewCast = firstViewCast;

        for (int i = 1; i <= stepCount; ++i)
        {
            float angle = transform.eulerAngles.y - enemyData.ViewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle, radius);

            bool tooFar = Mathf.Abs(oldViewCast.Distance - newViewCast.Distance) > edgeDistanceThreshold;

            if (oldViewCast.Hit != newViewCast.Hit || (oldViewCast.Hit && newViewCast.Hit && tooFar))
            {
                EdgeInfo edge = FindEdge(oldViewCast, newViewCast, radius);

                if (edge.PointA != Vector3.zero) viewPoints.Add(edge.PointA);
                if (edge.PointB != Vector3.zero) viewPoints.Add(edge.PointB);
            }

            viewPoints.Add(newViewCast.Point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < viewPoints.Count; ++i)
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

        for (int i = 0; i < vertexCount - 2; ++i)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void DrawSimpleWedge()
    {
        int stepCount = Mathf.Max(1, Mathf.RoundToInt(enemyData.ViewAngle * meshReolution));
        float stepAngleSize = enemyData.ViewAngle / stepCount;

        int vertexCount = stepCount + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= stepCount; ++i)
        {
            float localAng = -enemyData.ViewAngle * 0.5f + stepAngleSize * i;
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
