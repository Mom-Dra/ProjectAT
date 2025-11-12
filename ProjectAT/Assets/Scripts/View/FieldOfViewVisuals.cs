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

[RequireComponent(typeof(TargetDetector))]
public class FieldOfViewVisuals : MonoBehaviour
{
    private TargetDetector targetDetector;

    [Header("Scan Animation")]
    [SerializeField]
    private float defaultScanTime = 5f;

    [Header("Mesh Settings")]
    [SerializeField]
    private float meshReolution;
    [SerializeField]
    private int edgeResolveIteration;
    [SerializeField]
    private float edgeDistanceThreshold;

    [Header("Mesh Filters")]
    [SerializeField]
    private MeshFilter fixedMeshFilter; // 고정된 최대 시야각 (배경)
    [SerializeField]
    private MeshFilter viewmeshFilter;  // 차오르는 시야각

    private Mesh fixedMesh;
    private Mesh viewMesh;
    private float viewRadius; // 현재 차오르는 애니메이션의 반지름

    private Coroutine growingCoroutine;

    // 스캔 애니메이션 관련 이벤트
    public event System.Action onScanComplete;
    public event System.Action onScanStart;
    public event System.Action onScanCancel;

    private Enemy enemy;
    private EnemyData enemyData;
    public float ViewAngle => enemyData?.ViewAngle ?? 0f;

    private void Awake()
    {
        targetDetector = GetComponent<TargetDetector>();

        fixedMesh = new Mesh { name = "Full Mesh" };
        viewMesh = new Mesh { name = "View Mesh" };

        fixedMeshFilter.mesh = fixedMesh;
        viewmeshFilter.mesh = viewMesh;
    }

    private void OnEnable()
    {
        // TargetDetector의 이벤트에 구독
        // onTargetDetect는 0->1, 1->2 등 모든 "새 탐지"시 호출됨
        // 스캔 시작(StartScan)은 0->1 상황에서만 시작되어야 하므로
        // StartScan 내부에서 growingCoroutine이 null일 때만 실행하도록 방어
        targetDetector.onTargetDetect += StartScan;
        targetDetector.onTargetLosted += CancelScan;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        targetDetector.onTargetDetect -= StartScan;
        targetDetector.onTargetLosted -= CancelScan;

        // 비활성화 시 코루틴 정지 및 메시 클리어
        if (growingCoroutine != null)
        {
            StopCoroutine(growingCoroutine);
            growingCoroutine = null;
        }

        ClearMesh();
    }

    public void SetEnemyData(Enemy enemy)
    {
        this.enemy = enemy;
        enemyData = enemy.EnemyData;
    }

    private void StartScan()
    {
        // 이미 스캔(차오르는) 중이 아닐 때만 시작
        if (growingCoroutine == null)
        {
            onScanStart?.Invoke();
            growingCoroutine = StartCoroutine(GrowingCoroutine());
        }
    }

    private void CancelScan()
    {
        // 스캔(차오르는) 중일 때만 취소(줄어드는) 로직 실행
        if (growingCoroutine != null)
        {
            StopCoroutine(growingCoroutine);
            growingCoroutine = StartCoroutine(ShrinkingCoroutine());
        }
    }

    private IEnumerator GrowingCoroutine()
    {
        float scanTime = defaultScanTime * (enemy.AlertData.CombatThreshold - enemy.AlertLevel) / enemy.AlertData.CombatThreshold;

        yield return AnimateRadiusCoroutine(enemyData.SecondaryViewRadius, scanTime);

        viewRadius = enemyData.SecondaryViewRadius;
        onScanComplete?.Invoke();
        growingCoroutine = null;
    }

    private IEnumerator ShrinkingCoroutine()
    {
        yield return AnimateRadiusCoroutine(0f, defaultScanTime);

        viewRadius = 0f;
        onScanCancel?.Invoke();
        growingCoroutine = null;
    }

    private IEnumerator AnimateRadiusCoroutine(float targetRadius, float scanTime)
    {
        float time = 0f;
        float startRadius = viewRadius;
        float journey = Mathf.Abs(targetRadius - startRadius);

        float duration = scanTime * (journey / enemyData.SecondaryViewRadius);

        if (duration <= 0f) yield break;

        while (time <= duration)
        {
            viewRadius = Mathf.Lerp(startRadius, targetRadius, time / duration);

            // viewMesh는 현재 radius로, fixedMesh는 최대 radius로 그림
            DrawFieldOfView(viewMesh, viewRadius);
            DrawFieldOfView(fixedMesh, enemyData.SecondaryViewRadius);

            time += Time.deltaTime;
            yield return null;
        }

        viewRadius = targetRadius;
        DrawFieldOfView(viewMesh, viewRadius);

        ClearMesh();
    }

    private void ClearMesh()
    {
        if (viewMesh is not null && viewMesh.vertexCount > 0) viewMesh.Clear();
        if (fixedMesh is not null && fixedMesh.vertexCount > 0) fixedMesh.Clear();
    }

    private void DrawFieldOfView(Mesh mesh, float radius)
    {
        int stepCount = Mathf.Max(1, Mathf.RoundToInt(enemyData.ViewAngle * meshReolution));
        float stepAngleSize = enemyData.ViewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>(stepCount + 2); // 크기 넉넉하게
        ViewCastInfo oldViewCast;

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

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast, float radius)
    {
        float minAngle = minViewCast.Angle;
        float maxAngle = maxViewCast.Angle;
        Vector3 minPt = default; // 기본값 0
        Vector3 maxPt = default; // 기본값 0

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

        // TargetDetector로부터 ObstacleMask를 가져와 사용
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, radius, targetDetector.ObstacleMask))
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