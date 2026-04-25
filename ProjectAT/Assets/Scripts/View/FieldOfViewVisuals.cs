using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AwarenessModule))]
[RequireComponent(typeof(PerceptionSystem))]
public class FieldOfViewVisuals : MonoBehaviour
{
    private readonly struct ViewCastInfo
    {
        public readonly bool Hit;
        public readonly Vector3 Point;
        public readonly float Distance;
        public readonly float Angle;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            Hit = hit;
            Point = point;
            Distance = distance;
            Angle = angle;
        }
    }

    private readonly struct EdgeInfo
    {
        public readonly Vector3 PointA;
        public readonly Vector3 PointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }
    }

    [Header("Mesh Renderers")]
    [Tooltip("AlertLevel에 따라 반경이 변하는 메쉬")]
    [SerializeField] private MeshFilter viewMeshFilter;

    [SerializeField] private MeshFilter fixedMeshFilter;

    [Header("Mesh Quality")]
    [Tooltip("각도 1도당 광선 수. 높을수록 부드러움.")]
    [SerializeField, Range(0.1f, 2f)] private float meshResolution = 0.5f;

    [Tooltip("장애물 가장자리 정밀화 반복 횟수. 0이면 비활성.")]
    [SerializeField, Range(0, 8)] private int edgeResolveIterations = 4;

    [Tooltip("두 광선의 거리 차이가 이 값 이상이면 가장자리로 인식.")]
    [SerializeField] private float edgeDistanceThreshold = 0.5f;

    [Header("Mask Offset")]
    [Tooltip("메쉬가 지면에 묻히지 않도록 살짝 띄움")]
    [SerializeField] private float maskHeightOffset = 0.05f;

    private AwarenessModule awarenessModule;
    private PerceptionSystem perceptionSystem;
    private Mesh viewMesh;
    private Mesh fixedMesh;
    private bool fixedMeshDrawn;

    private readonly List<Vector3> viewPoints = new List<Vector3>(64);

    private void Awake()
    {
        awarenessModule = GetComponent<AwarenessModule>();
        perceptionSystem = GetComponent<PerceptionSystem>();

        viewMesh = new Mesh { name = "Fov View Mesh" };
        viewMeshFilter.mesh = viewMesh;
        viewMesh.MarkDynamic();

        fixedMesh = new Mesh { name = "FOV Fixed Mesh" };
        fixedMeshFilter.mesh = fixedMesh;
    }

    private void OnDisable()
    {
        ClearMesh(viewMesh);
        ClearMesh(fixedMesh);
        fixedMeshDrawn = false;
    }

    private void OnDestroy()
    {
        Destroy(viewMesh);
        Destroy(fixedMesh);
    }

    private void LateUpdate()
    {
        DrawMesh();
    }

    private void DrawMesh()
    {
        float alert = awarenessModule.NormalizedAlert;
        float maxRadius = perceptionSystem.SecondaryViewRadius;
        float fov = perceptionSystem.ViewAngle;

        if (alert > 0.001f)
        {
            float currentRadius = maxRadius * alert;
            DrawFieldOfView(viewMesh, currentRadius, fov);
        }
        else if (viewMesh.vertexCount > 0)
        {
            ClearMesh(viewMesh);
        }

        if (fixedMesh is not null && !fixedMeshDrawn)
        {
            DrawFieldOfView(fixedMesh, maxRadius, fov);
            fixedMeshDrawn = true;
        }
    }

    private void DrawFieldOfView(Mesh mesh, float radius, float fovAngle)
    {
        int stepCount = Mathf.Max(1, Mathf.RoundToInt(fovAngle * meshResolution));
        float stepAngle = fovAngle / stepCount;

        viewPoints.Clear();
        ViewCastInfo oldCast = default;

        for (int i = 0; i <= stepCount; ++i)
        {
            float angle = transform.eulerAngles.y - fovAngle * 0.5f + stepAngle * i;
            ViewCastInfo newCast = ViewCast(angle, radius);

            if (i > 0)
            {
                bool tooFar = Mathf.Abs(oldCast.Distance - newCast.Distance) > edgeDistanceThreshold;
                if (oldCast.Hit != newCast.Hit || (oldCast.Hit && newCast.Hit && tooFar))
                {
                    EdgeInfo edge = FindEdge(oldCast, newCast, radius);
                    if (edge.PointA != Vector3.zero) viewPoints.Add(edge.PointA);
                    if (edge.PointB != Vector3.zero) viewPoints.Add(edge.PointB);
                }
            }

            viewPoints.Add(newCast.Point);
            oldCast = newCast;
        }

        BuildMesh(mesh, viewPoints);
    }

    private void BuildMesh(Mesh mesh, List<Vector3> points)
    {
        int vertexCount = points.Count + 1;

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        // 0번은 원점 (부채꼴의 꼭짓점)
        vertices[0] = Vector3.up * maskHeightOffset;

        for (int i = 0; i < points.Count; ++i)
        {
            Vector3 local = transform.InverseTransformPoint(points[i]);
            local.y = maskHeightOffset;
            vertices[i + 1] = local;
        }

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

    private EdgeInfo FindEdge(ViewCastInfo a, ViewCastInfo b, float radius)
    {
        float minAngle = a.Angle;
        float maxAngle = b.Angle;
        Vector3 minPt = Vector3.zero;
        Vector3 maxPt = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) * 0.5f;
            ViewCastInfo cast = ViewCast(angle, radius);
            bool tooFar = Mathf.Abs(a.Distance - cast.Distance) > edgeDistanceThreshold;

            if (cast.Hit == a.Hit && !tooFar)
            {
                minAngle = angle;
                minPt = cast.Point;
            }
            else
            {
                maxAngle = angle;
                maxPt = cast.Point;
            }
        }

        return new EdgeInfo(minPt, maxPt);
    }

    private ViewCastInfo ViewCast(float globalAngleDeg, float radius)
    {
        Vector3 dir = DirFromAngle(globalAngleDeg);

        if (Physics.Raycast(transform.position, dir, out var hit, radius, perceptionSystem.ObstacleMask, QueryTriggerInteraction.Ignore))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngleDeg);
        }

        return new ViewCastInfo(false, transform.position + dir * radius, radius, globalAngleDeg);
    }

    private static Vector3 DirFromAngle(float deg) => new Vector3(Mathf.Sin(deg * Mathf.Deg2Rad), 0f, Mathf.Cos(deg * Mathf.Deg2Rad));

    private static void ClearMesh(Mesh mesh)
    {
        if (mesh.vertexCount == 0) return;

        mesh.Clear();
    }
}