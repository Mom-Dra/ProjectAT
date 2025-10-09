using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [SerializeField]
    private SecurityCameraData securityCameraData;

    [SerializeField]
    private LayerMask targetMask;
    [SerializeField]
    private LayerMask obstacleMask;

    [SerializeField]
    private MeshFilter fixedMeshFilter;
    [SerializeField]
    private MeshFilter viewmeshFilter;

    private Mesh fixedMesh;
    private Mesh viewMesh;

    [SerializeField]
    private float meshReolution;
    [SerializeField]
    private int edgeResolveIteration;
    [SerializeField]
    private float edgeDistanceThreshold;

    private void Awake()
    {
        fixedMesh = new Mesh() { name = "Full Mesh" };
        fixedMeshFilter.mesh = fixedMesh;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 10, Color.red);
        DrawFieldOfView(fixedMesh, securityCameraData.ScanRange);
    }

    private void DrawFieldOfView(Mesh mesh, float radius)
    {
        int stepCount = Mathf.Max(1, Mathf.RoundToInt(securityCameraData.ViewAngle * meshReolution));
        float stepAngleSize = securityCameraData.ViewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>(stepCount);
        ViewCastInfo oldViewCast;

        // i = 0
        float firstAngle = transform.eulerAngles.y - securityCameraData.ViewAngle / 2;
        ViewCastInfo firstViewCast = ViewCast(firstAngle, radius);
        viewPoints.Add(firstViewCast.Point);
        oldViewCast = firstViewCast;

        for (int i = 1; i <= stepCount; ++i)
        {
            float angle = transform.eulerAngles.y - securityCameraData.ViewAngle / 2 + stepAngleSize * i;
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
        return new Vector3(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Tan(transform.eulerAngles.x * Mathf.Deg2Rad), Mathf.Cos(deg * Mathf.Deg2Rad));
    }
}
