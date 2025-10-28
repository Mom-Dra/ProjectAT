using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Camera: 회전하다가 멈춘다..!

public class SecurityCamera : MonoBehaviour
{
    [SerializeField]
    private SecurityCameraData securityCameraData;

    private Coroutine rotateCoroutine;

    private Collider[] targetsInViewRadius = new Collider[4];

    private bool isPlayerDetected = false;

    private void Awake()
    { 

    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(securityCameraData.ViewAngle, 0f, 0f);
        StartRotate();
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 10, Color.red);
        DetectPlayers();
    }

    private void StartRotate()
    {
        if(rotateCoroutine == null)
        {
            rotateCoroutine = StartCoroutine(RotateCoroutine());
        }
    }

    private void StopRotate()
    {
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }
    }

    private void DetectPlayers()
    {
        // 1단계: 탐지 범위 내 모든 플레이어 후보 찾기
        int count = Physics.OverlapSphereNonAlloc(transform.position, securityCameraData.DetectionRange, targetsInViewRadius, securityCameraData.PlayerLayer);

        // 탐지된 플레이어가 있는지 확인하는 플래그
        bool isplayerFoundThisFrame = false;
        Transform target = null;

        for (int i = 0; i < count; ++i)
        {
            target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // 2단계: 시야각 확인
            if (Vector3.Angle(transform.forward, directionToTarget) < securityCameraData.DetectionAngle / 2)
            {
                // 3단계: 장애물 확인 (레이캐스트)
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                // target(플레이어)에게 레이를 쐈을 때, 장애물에 먼저 부딪히지 않았다면!
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, securityCameraData.ObstacleLayer))
                {
                    // 모든 검사 통과: 최종 탐지 성공!
                    isplayerFoundThisFrame = true;
                    break; // 한 명이라도 찾으면 더 검사할 필요 없이 루프 종료
                }
            }
        }

        // 발견했어: 회전을 멈춘다!
        // 점점 게이지가 차오르는 느낌스!

        // 사라졌어: 다시 회전한다!
        // 점점 게이지가 낮아지는 느낌스!

        // 이번 프레임의 탐지 결과에 따라 상태 업데이트
        if (isplayerFoundThisFrame)
        {
            Debug.DrawRay(transform.position, target.position - transform.position, Color.red);

            if (!isPlayerDetected) // 이전 프레임에서는 탐지 못했다가 이번에 처음 탐지했다면
            {
                OnPlayerDetected();
            }
        }
        else
        {
            if (isPlayerDetected) // 이전 프레임에서는 탐지했으나 지금은 보이지 않는다면
            {
                OnPlayerNotDetected();
            }
        }
    }

    private void OnPlayerDetected()
    {
        Debug.Log("OnPlayerDetected");
        isPlayerDetected = true;
    }

    private void OnPlayerNotDetected()
    {
        Debug.Log("OnPlayerNotDetected");
        isPlayerDetected = false;
    }

    private void CheckDetection()
    {
        //Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

        //// 2단계: 시야각 확인
        //// 카메라의 정면 방향과 플레이어 방향 사이의 각도를 계산
        //if (Vector3.Angle(transform.forward, directionToPlayer) < fieldOfViewAngle / 2)
        //{
        //    // 3단계: 장애물 확인 (레이캐스트)
        //    // 카메라와 플레이어 사이의 거리를 계산
        //    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        //    // 시야 거리를 초과하면 탐지하지 않음
        //    if (distanceToPlayer > viewDistance) return;

        //    // 카메라에서 플레이어 방향으로 레이캐스트를 발사해서 장애물이 있는지 확인
        //    if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
        //    {
        //        // 장애물이 없으면 최종적으로 탐지 성공!
        //        Debug.Log("플레이어 발견!");
        //        // 여기에 플레이어 발견 시 실행할 코드 작성 (예: 경보 울리기)
        //    }
        //}
    }

    private IEnumerator RotateCoroutine()
    {
        while (true)
        {
            float pingPong = Mathf.PingPong(Time.time * securityCameraData.RotateSpeed, securityCameraData.RotateAngle * 2);
            float targetAngle = pingPong - securityCameraData.ViewAngle;

            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z);

            yield return null;
        }
    }

    //[SerializeField]
    //private LayerMask targetMask;
    //[SerializeField]
    //private LayerMask obstacleMask;

    //[SerializeField]
    //private MeshFilter fixedMeshFilter;
    //[SerializeField]
    //private MeshFilter viewmeshFilter;

    //private Mesh fixedMesh;
    //private Mesh viewMesh;

    //[SerializeField]
    //private float meshReolution;
    //[SerializeField]
    //private int edgeResolveIteration;
    //[SerializeField]
    //private float edgeDistanceThreshold;

    //private void DrawFieldOfView(Mesh mesh, float radius)
    //{
    //    int stepCount = Mathf.Max(1, Mathf.RoundToInt(securityCameraData.ViewAngle * meshReolution));
    //    float stepAngleSize = securityCameraData.ViewAngle / stepCount;
    //    List<Vector3> viewPoints = new List<Vector3>(stepCount);
    //    ViewCastInfo oldViewCast;

    //    // i = 0
    //    float firstAngle = transform.eulerAngles.y - securityCameraData.ViewAngle / 2;
    //    ViewCastInfo firstViewCast = ViewCast(firstAngle, radius);
    //    viewPoints.Add(firstViewCast.Point);
    //    oldViewCast = firstViewCast;

    //    for (int i = 1; i <= stepCount; ++i)
    //    {
    //        float angle = transform.eulerAngles.y - securityCameraData.ViewAngle / 2 + stepAngleSize * i;
    //        ViewCastInfo newViewCast = ViewCast(angle, radius);

    //        bool tooFar = Mathf.Abs(oldViewCast.Distance - newViewCast.Distance) > edgeDistanceThreshold;

    //        if (oldViewCast.Hit != newViewCast.Hit || (oldViewCast.Hit && newViewCast.Hit && tooFar))
    //        {
    //            EdgeInfo edge = FindEdge(oldViewCast, newViewCast, radius);

    //            if (edge.PointA != Vector3.zero) viewPoints.Add(edge.PointA);
    //            if (edge.PointB != Vector3.zero) viewPoints.Add(edge.PointB);
    //        }

    //        viewPoints.Add(newViewCast.Point);
    //        oldViewCast = newViewCast;
    //    }

    //    int vertexCount = viewPoints.Count + 1;
    //    Vector3[] vertices = new Vector3[vertexCount];
    //    int[] triangles = new int[(vertexCount - 2) * 3];

    //    vertices[0] = Vector3.zero;
    //    for (int i = 0; i < viewPoints.Count; ++i)
    //        vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

    //    for (int i = 0; i < vertexCount - 2; ++i)
    //    {
    //        triangles[i * 3] = 0;
    //        triangles[i * 3 + 1] = i + 1;
    //        triangles[i * 3 + 2] = i + 2;
    //    }

    //    mesh.Clear();
    //    mesh.vertices = vertices;
    //    mesh.triangles = triangles;
    //    mesh.RecalculateNormals();
    //}

    //private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast, float radius)
    //{
    //    float minAngle = minViewCast.Angle;
    //    float maxAngle = maxViewCast.Angle;
    //    Vector3 minPt = default;
    //    Vector3 maxPt = default;

    //    for (int i = 0; i < edgeResolveIteration; ++i)
    //    {
    //        float angle = (minAngle + maxAngle) / 2f;
    //        ViewCastInfo newViewCast = ViewCast(angle, radius);
    //        bool tooFar = Mathf.Abs(minViewCast.Distance - newViewCast.Distance) > edgeDistanceThreshold;

    //        if (newViewCast.Hit == minViewCast.Hit && !tooFar)
    //        {
    //            minAngle = angle;
    //            minPt = newViewCast.Point;
    //        }
    //        else
    //        {
    //            maxAngle = angle;
    //            maxPt = newViewCast.Point;
    //        }
    //    }

    //    return new EdgeInfo(minPt, maxPt);
    //}

    //private ViewCastInfo ViewCast(float globalAngle, float radius)
    //{
    //    Vector3 dir = DirFromGlobalAngle(globalAngle);
    //    if (Physics.Raycast(transform.position, dir, out RaycastHit hit, radius, obstacleMask))
    //        return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);

    //    return new ViewCastInfo(false, transform.position + dir * radius, radius, globalAngle);
    //}

    //private Vector3 DirFromGlobalAngle(float deg)
    //{
    //    return new Vector3(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Tan(transform.eulerAngles.x * Mathf.Deg2Rad), Mathf.Cos(deg * Mathf.Deg2Rad));
    //}
}
