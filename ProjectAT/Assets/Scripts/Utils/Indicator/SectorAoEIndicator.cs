using System.Collections.Generic;
using UnityEngine;
using Indicators;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SectorAoEIndicator : IndicatorBase, ISectorIndicator
{
    private readonly struct SectorCastInfo
    {
        public readonly bool Hit;
        public readonly Vector3 LocalPoint;
        public readonly float Distance;
        public readonly float Angle;

        public SectorCastInfo(bool hit, Vector3 localPoint, float distance, float angle)
        {
            Hit = hit;
            LocalPoint = localPoint;
            Distance = distance;
            Angle = angle;
        }
    }
    #region SectorAoE Parameters
    private const int MinSegments = 1;
    private const int MaxSegments = 60;
    private const float FullCircleAngle = 359.9f;
    private const float PointMergeSqrDistance = 0.000001f;

    [Header("Shape")]
    [Tooltip("Total sector angle in degrees. The sector is centered on this object's forward direction.")]
    [SerializeField, Range(0f, 360f)] private float angle = 60f;

    [Tooltip("Maximum length/radius of the sector mesh from the origin.")]
    [SerializeField, Min(0f)] private float width = 10f;

    [Tooltip("Number of arc subdivisions. Higher values make the curve smoother but require more raycasts and mesh vertices.")]
    [SerializeField, Range(MinSegments, MaxSegments)] private int segments = 32;

    [Tooltip("Local Y offset applied to the visual mesh to reduce ground z-fighting.")]
    [SerializeField] private float heightOffset = 0.03f;

    [Header("Collider")]
    [Tooltip("When enabled, creates and updates a MeshCollider that matches the rendered sector shape.")]
    [SerializeField] private bool createCollider;

    [Tooltip("Sets the generated MeshCollider as a trigger.")]
    [SerializeField] private bool colliderIsTrigger = true;

    [Tooltip("Thickness of the generated collider prism around the visual mesh plane.")]
    [SerializeField, Min(0.01f)] private float colliderHeight = 0.5f;

    [Header("Obstacle Clipping")]
    [Tooltip("When enabled, raycasts against obstacles and stops the mesh at the first hit point in each segment direction.")]
    [SerializeField] private bool clipByObstacle;

    [Tooltip("Layers that block the sector mesh when obstacle clipping is enabled.")]
    [SerializeField] private LayerMask obstacleLayerMask;

    [Tooltip("World-space height offset used as the raycast origin above this object's position.")]
    [SerializeField, Min(0f)] private float obstacleRayHeight = 0.5f;

    [Tooltip("Extra binary-search steps used to refine rough obstacle edges between adjacent rays.")]
    [SerializeField, Range(0, 8)] private int edgeResolveIterations = 3;

    [Tooltip("Distance difference between adjacent ray hits required before edge refinement is applied.")]
    [SerializeField, Min(0f)] private float edgeDistanceThreshold = 0.5f;

    private MeshFilter meshFilter;
    private Mesh visualMesh;
    private Mesh colliderMesh;
    private MeshCollider meshCollider;

    private readonly List<Vector3> sectorPoints = new List<Vector3>(MaxSegments * 3 + 1);
    private readonly List<Vector3> cachedSectorPoints = new List<Vector3>(MaxSegments * 3 + 1);

    private readonly List<Vector3> visualVertices = new List<Vector3>(MaxSegments * 3 + 2);
    private readonly List<Vector2> visualUvs = new List<Vector2>(MaxSegments * 3 + 2);
    private readonly List<int> visualTriangles = new List<int>(MaxSegments * 9);

    private readonly List<Vector3> colliderVertices = new List<Vector3>((MaxSegments * 3 + 2) * 2);
    private readonly List<int> colliderTriangles = new List<int>((MaxSegments * 3 + 1) * 12);

    private float cachedAngle = -1f;
    private float cachedWidth = -1f;
    private float cachedHeightOffset = -1f;
    private float cachedColliderHeight = -1f;
    private float cachedObstacleRayHeight = -1f;
    private float cachedEdgeDistanceThreshold = -1f;
    private int cachedSegments = -1;
    private int cachedObstacleLayerMask = int.MinValue;
    private int cachedEdgeResolveIterations = -1;
    private bool cachedCreateCollider;
    private bool cachedColliderIsTrigger;
    private bool cachedClipByObstacle;
    private bool visualMeshUploaded;
    private bool colliderMeshUploaded;
    private bool colliderWasEnabled;
    #endregion

    public float Angle => angle;
    public float Width => width;

    [SerializeField]
    private Transform attachedTarget;

    public Transform AttachedTarget => attachedTarget;

    private void Awake()
    {
        EnsureMesh();
        RebuildIfNeeded(true);
    }

    // private void OnValidate()
    // {
    //     SanitizeValues();
    //     EnsureMesh();
    //     RebuildIfNeeded(false);
    // }

    private void LateUpdate()
    {
        if (!clipByObstacle)
        {
            return;
        }

        RebuildIfNeeded(true);
    }

    private void OnDestroy()
    {
        DestroyGeneratedMesh(visualMesh);
        DestroyGeneratedMesh(colliderMesh);
    }

    private void SanitizeValues()
    {
        angle = Mathf.Clamp(angle, 0f, 360f);
        width = Mathf.Max(0f, width);
        segments = Mathf.Clamp(segments, MinSegments, MaxSegments);
        colliderHeight = Mathf.Max(0.01f, colliderHeight);
        obstacleRayHeight = Mathf.Max(0f, obstacleRayHeight);
        edgeResolveIterations = Mathf.Clamp(edgeResolveIterations, 0, 8);
        edgeDistanceThreshold = Mathf.Max(0f, edgeDistanceThreshold);
    }

    private void EnsureMesh()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (visualMesh == null)
        {
            visualMesh = new Mesh { name = "Sector AoE Visual Mesh" };
            visualMesh.MarkDynamic();
            visualMesh.hideFlags = HideFlags.DontSave;
        }

        if (meshFilter.sharedMesh != visualMesh)
        {
            meshFilter.sharedMesh = visualMesh;
        }

        EnsureBufferCapacity(GetMaxSectorPointCount());

        if (createCollider && colliderMesh == null)
        {
            colliderMesh = new Mesh { name = "Sector AoE Collider Mesh" };
            colliderMesh.MarkDynamic();
            colliderMesh.hideFlags = HideFlags.DontSave;
        }
    }

    private void RebuildIfNeeded(bool force)
    {
        bool shapeChanged =
            !Mathf.Approximately(cachedAngle, angle) ||
            !Mathf.Approximately(cachedWidth, width) ||
            !Mathf.Approximately(cachedHeightOffset, heightOffset) ||
            cachedSegments != segments;

        bool colliderChanged =
            cachedCreateCollider != createCollider ||
            cachedColliderIsTrigger != colliderIsTrigger ||
            !Mathf.Approximately(cachedColliderHeight, colliderHeight);

        bool obstacleChanged =
            cachedClipByObstacle != clipByObstacle ||
            cachedObstacleLayerMask != obstacleLayerMask.value ||
            !Mathf.Approximately(cachedObstacleRayHeight, obstacleRayHeight) ||
            cachedEdgeResolveIterations != edgeResolveIterations ||
            !Mathf.Approximately(cachedEdgeDistanceThreshold, edgeDistanceThreshold);

        if (!force && !shapeChanged && !colliderChanged && !obstacleChanged)
        {
            return;
        }

        bool needsPointRefresh = force || shapeChanged || obstacleChanged || sectorPoints.Count == 0;
        bool pointsChanged = false;

        if (needsPointRefresh)
        {
            CollectSectorPoints();
            EnsureBufferCapacity(Mathf.Max(GetMaxSectorPointCount(), sectorPoints.Count));
            pointsChanged = !AreSamePoints(sectorPoints, cachedSectorPoints);

            if (pointsChanged || !visualMeshUploaded)
            {
                UploadVisualMesh();
                CopyPoints(sectorPoints, cachedSectorPoints);
                visualMeshUploaded = true;
            }
        }

        bool colliderGeometryChanged =
            createCollider &&
            HasColliderArea() &&
            sectorPoints.Count >= 2 &&
            (pointsChanged || !colliderMeshUploaded || !Mathf.Approximately(cachedColliderHeight, colliderHeight));

        if (pointsChanged || colliderChanged || colliderGeometryChanged || shapeChanged || obstacleChanged)
        {
            ConfigureCollider(colliderGeometryChanged);
        }

        cachedAngle = angle;
        cachedWidth = width;
        cachedHeightOffset = heightOffset;
        cachedSegments = segments;
        cachedCreateCollider = createCollider;
        cachedColliderIsTrigger = colliderIsTrigger;
        cachedColliderHeight = colliderHeight;
        cachedClipByObstacle = clipByObstacle;
        cachedObstacleLayerMask = obstacleLayerMask.value;
        cachedObstacleRayHeight = obstacleRayHeight;
        cachedEdgeResolveIterations = edgeResolveIterations;
        cachedEdgeDistanceThreshold = edgeDistanceThreshold;
    }

    private void CollectSectorPoints()
    {
        sectorPoints.Clear();

        if (clipByObstacle)
        {
            CollectClippedSectorPoints();
        }
        else
        {
            CollectSimpleSectorPoints();
        }
    }

    private void CollectSimpleSectorPoints()
    {
        float startAngle = -angle * 0.5f;
        float stepAngle = angle / segments;

        for (int i = 0; i <= segments; i++)
        {
            Vector3 point = DirectionFromAngle(startAngle + stepAngle * i) * width;
            point.y = heightOffset;
            sectorPoints.Add(point);
        }
    }

    private void CollectClippedSectorPoints()
    {
        float startAngle = -angle * 0.5f;
        float stepAngle = angle / segments;
        SectorCastInfo oldCast = default;

        for (int i = 0; i <= segments; i++)
        {
            SectorCastInfo newCast = CastSectorPoint(startAngle + stepAngle * i);

            if (i > 0)
            {
                bool tooFar = Mathf.Abs(oldCast.Distance - newCast.Distance) > edgeDistanceThreshold;
                if (oldCast.Hit != newCast.Hit || (oldCast.Hit && newCast.Hit && tooFar))
                {
                    AddEdgePoints(oldCast, newCast);
                }
            }

            AddSectorPoint(newCast.LocalPoint);
            oldCast = newCast;
        }
    }

    private SectorCastInfo CastSectorPoint(float localAngle)
    {
        Vector3 localDirection = DirectionFromAngle(localAngle);
        Vector3 worldDirection = transform.TransformDirection(localDirection).normalized;
        Vector3 rayOrigin = transform.position + Vector3.up * obstacleRayHeight;

        if (obstacleLayerMask.value != 0 &&
            Physics.Raycast(rayOrigin, worldDirection, out RaycastHit hit, width, obstacleLayerMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 localHit = transform.InverseTransformPoint(hit.point);
            localHit.y = heightOffset;

            return new SectorCastInfo(true, localHit, hit.distance, localAngle);
        }

        Vector3 localPoint = localDirection * width;
        localPoint.y = heightOffset;

        return new SectorCastInfo(false, localPoint, width, localAngle);
    }

    private void AddEdgePoints(SectorCastInfo oldCast, SectorCastInfo newCast)
    {
        if (edgeResolveIterations <= 0)
        {
            return;
        }

        SectorCastInfo minCast = oldCast;
        SectorCastInfo maxCast = newCast;
        float minAngle = oldCast.Angle;
        float maxAngle = newCast.Angle;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) * 0.5f;
            SectorCastInfo cast = CastSectorPoint(angle);
            bool tooFar = Mathf.Abs(oldCast.Distance - cast.Distance) > edgeDistanceThreshold;

            if (cast.Hit == oldCast.Hit && !tooFar)
            {
                minAngle = angle;
                minCast = cast;
            }
            else
            {
                maxAngle = angle;
                maxCast = cast;
            }
        }

        AddSectorPoint(minCast.LocalPoint);
        AddSectorPoint(maxCast.LocalPoint);
    }

    private void AddSectorPoint(Vector3 point)
    {
        if (sectorPoints.Count > 0 &&
            (sectorPoints[sectorPoints.Count - 1] - point).sqrMagnitude <= PointMergeSqrDistance)
        {
            return;
        }

        sectorPoints.Add(point);
    }

    private void UploadVisualMesh()
    {
        int pointCount = sectorPoints.Count;

        visualVertices.Clear();
        visualUvs.Clear();
        visualTriangles.Clear();

        visualVertices.Add(Vector3.up * heightOffset);
        visualUvs.Add(new Vector2(0.5f, 0f));

        for (int i = 0; i < pointCount; i++)
        {
            int vertexIndex = i + 1;
            Vector3 point = sectorPoints[i];

            visualVertices.Add(point);
            visualUvs.Add(new Vector2(
                pointCount > 1 ? (float)i / (pointCount - 1) : 0.5f,
                width > 0.001f ? Mathf.Clamp01(new Vector2(point.x, point.z).magnitude / width) : 0f
            ));

            if (i < pointCount - 1)
            {
                visualTriangles.Add(0);
                visualTriangles.Add(vertexIndex);
                visualTriangles.Add(vertexIndex + 1);
            }
        }

        visualMesh.Clear(false);
        visualMesh.SetVertices(visualVertices);
        visualMesh.SetUVs(0, visualUvs);
        visualMesh.SetTriangles(visualTriangles, 0, true);
        visualMesh.RecalculateNormals();
        visualMesh.RecalculateBounds();
    }

    private void ConfigureCollider(bool geometryChanged)
    {
        if (!createCollider || !HasColliderArea() || sectorPoints.Count < 2)
        {
            DisableCollider();
            return;
        }

        EnsureCollider();

        if (geometryChanged)
        {
            UploadColliderMesh();
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = colliderMesh;
            colliderMeshUploaded = true;
        }
        else if (meshCollider.sharedMesh != colliderMesh)
        {
            meshCollider.sharedMesh = colliderMesh;
        }

        meshCollider.enabled = true;
        meshCollider.convex = true;
        meshCollider.isTrigger = colliderIsTrigger;
        colliderWasEnabled = true;
    }

    private void EnsureCollider()
    {
        if (meshCollider == null)
        {
            meshCollider = GetComponent<MeshCollider>();
        }

        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        if (colliderMesh == null)
        {
            colliderMesh = new Mesh { name = "Sector AoE Collider Mesh" };
            colliderMesh.MarkDynamic();
            colliderMesh.hideFlags = HideFlags.DontSave;
        }
    }

    private void DisableCollider()
    {
        if (meshCollider == null)
        {
            meshCollider = GetComponent<MeshCollider>();
        }

        if (meshCollider == null)
        {
            return;
        }

        if (colliderWasEnabled || meshCollider.sharedMesh != null || meshCollider.enabled)
        {
            meshCollider.sharedMesh = null;
            meshCollider.enabled = false;
        }

        colliderMeshUploaded = false;
        colliderWasEnabled = false;
    }

    private bool HasColliderArea()
    {
        return angle > 0.01f && width > 0.001f;
    }

    private void UploadColliderMesh()
    {
        if (colliderMesh == null)
        {
            return;
        }

        bool isFullCircle = angle >= FullCircleAngle;
        int pointCount = sectorPoints.Count;
        int fanTriangleCount = pointCount - 1;
        int bottomOffset = pointCount + 1;
        float halfHeight = colliderHeight * 0.5f;
        float topY = heightOffset + halfHeight;
        float bottomY = heightOffset - halfHeight;

        colliderVertices.Clear();
        colliderTriangles.Clear();

        colliderVertices.Add(Vector3.up * topY);
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 point = sectorPoints[i];
            colliderVertices.Add(new Vector3(point.x, topY, point.z));
        }

        colliderVertices.Add(Vector3.up * bottomY);
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 point = sectorPoints[i];
            colliderVertices.Add(new Vector3(point.x, bottomY, point.z));
        }

        for (int i = 0; i < fanTriangleCount; i++)
        {
            int topA = i + 1;
            int topB = i + 2;
            int bottomA = bottomOffset + topA;
            int bottomB = bottomOffset + topB;

            AddTriangle(colliderTriangles, 0, topA, topB);
            AddTriangle(colliderTriangles, bottomOffset, bottomB, bottomA);
            AddTriangle(colliderTriangles, topA, bottomA, bottomB);
            AddTriangle(colliderTriangles, topA, bottomB, topB);
        }

        if (!isFullCircle)
        {
            int firstTop = 1;
            int firstBottom = bottomOffset + firstTop;
            int lastTop = pointCount;
            int lastBottom = bottomOffset + lastTop;

            AddTriangle(colliderTriangles, 0, bottomOffset, firstBottom);
            AddTriangle(colliderTriangles, 0, firstBottom, firstTop);
            AddTriangle(colliderTriangles, 0, lastTop, lastBottom);
            AddTriangle(colliderTriangles, 0, lastBottom, bottomOffset);
        }

        colliderMesh.Clear(false);
        colliderMesh.SetVertices(colliderVertices);
        colliderMesh.SetTriangles(colliderTriangles, 0, true);
        colliderMesh.RecalculateNormals();
        colliderMesh.RecalculateBounds();
    }

    private void EnsureBufferCapacity(int maxPointCount)
    {
        int visualVertexCapacity = maxPointCount + 1;
        int visualTriangleCapacity = Mathf.Max(0, maxPointCount - 1) * 3;
        int colliderVertexCapacity = visualVertexCapacity * 2;
        int colliderTriangleCapacity = (Mathf.Max(0, maxPointCount - 1) * 4 + 4) * 3;

        EnsureCapacity(sectorPoints, maxPointCount);
        EnsureCapacity(cachedSectorPoints, maxPointCount);
        EnsureCapacity(visualVertices, visualVertexCapacity);
        EnsureCapacity(visualUvs, visualVertexCapacity);
        EnsureCapacity(visualTriangles, visualTriangleCapacity);
        EnsureCapacity(colliderVertices, colliderVertexCapacity);
        EnsureCapacity(colliderTriangles, colliderTriangleCapacity);
    }

    private int GetMaxSectorPointCount()
    {
        return segments + 1 + segments * 2;
    }

    private static bool AreSamePoints(List<Vector3> a, List<Vector3> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        for (int i = 0; i < a.Count; i++)
        {
            if ((a[i] - b[i]).sqrMagnitude > PointMergeSqrDistance)
            {
                return false;
            }
        }

        return true;
    }

    private static void CopyPoints(List<Vector3> source, List<Vector3> destination)
    {
        destination.Clear();
        EnsureCapacity(destination, source.Count);

        for (int i = 0; i < source.Count; i++)
        {
            destination.Add(source[i]);
        }
    }

    private static void EnsureCapacity<T>(List<T> list, int capacity)
    {
        if (list.Capacity < capacity)
        {
            list.Capacity = capacity;
        }
    }

    private static void AddTriangle(List<int> triangles, int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }

    private static Vector3 DirectionFromAngle(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
    }

    private static void DestroyGeneratedMesh(Mesh mesh)
    {
        if (mesh == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(mesh);
        }
        else
        {
            DestroyImmediate(mesh);
        }
    }

    #region Public API
    public void SetSize(float newAngle, float newWidth)
    {
        angle = 2f * Mathf.Atan2(newAngle, newWidth) * Mathf.Rad2Deg;
        width = newWidth;
        SanitizeValues();
        EnsureMesh();
        RebuildIfNeeded(true);
    }

    public void SetTarget(Transform target)
    {
        attachedTarget = target;
    }

    public override void Show()
    {
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
        attachedTarget = null;
        
    }

    public override void UpdateIndicator(Vector3 toDest)
    {   
        transform.position = attachedTarget.position + heightOffset * Vector3.up;
        transform.rotation = Quaternion.LookRotation(toDest - attachedTarget.position, Vector3.up);
        RebuildIfNeeded(true);
    }
    #endregion
}