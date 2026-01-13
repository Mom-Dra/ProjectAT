using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TargetDetector : MonoBehaviour
{
    // 처음에 적이 시야에 들어 왔들 때
    // 추가로 적이 시야에 들어 왔을 때 
    public event System.Action onTargetDetect;

    // 모든 적이 시야에 없을 때
    public event System.Action onTargetLosted;

    [SerializeField]
    private float detectInterval = 0.2f;
    [SerializeField]
    private LayerMask targetMask;
    [SerializeField]
    private LayerMask obstacleMask;
    public LayerMask ObstacleMask => obstacleMask;

    private bool isAttackMode;
    private Enemy enemy;
    private EnemyData enemyData;
    private List<(Transform transform, float distance)> visibleTargets = new List<(Transform transform, float distance)>();
    private Collider[] colliders = new Collider[4];
    private WaitForSeconds detectWait;
    private Coroutine detectLoopCoroutine;
    private EntityStatus entityStatus;

    public float ViewAngle => enemyData?.ViewAngle ?? 0f;
    public IReadOnlyList<(Transform transform, float distance)> VisibleTargets => visibleTargets;
    public (Transform transform, float distance)? GetFirstTargetInfo => visibleTargets.Count == 0 ? null : visibleTargets[0];

    private void Awake()
    {
        entityStatus = GetComponent<EntityStatus>();
        detectWait = new WaitForSeconds(detectInterval);
    }

    private void OnEnable()
    {
        entityStatus.onDeath += EnemyDied;

        StartDetectLoop();
    }

    private void OnDisable()
    {
        entityStatus.onDeath -= EnemyDied;

        StopDetectLoop();
    }

    public void SetEnemyData(Enemy enemy)
    {
        this.enemy = enemy;
        enemyData = enemy.EnemyData;
    }

    public void SetAttackMode(bool isAttackMode)
    {
        this.isAttackMode = isAttackMode;
    }

    public bool IsTargetDetected()
    {
        return visibleTargets.Count > 0;
    }

    public CoverPoint FindBestCover()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, enemyData.SearchRadius, LayerMask.GetMask("CoverPoint"));
        Array.Sort(hits, (x, y) =>
        {
            float dis1 = (transform.position - x.transform.position).sqrMagnitude;
            float dis2 = (transform.position - y.transform.position).sqrMagnitude;

            return dis1.CompareTo(dis2);
        });

        foreach (Collider collider in hits)
        {
            if(collider.TryGetComponent(out CoverPoint point) && !point.IsOccupied)
            {
                if ((collider.transform.position - GetFirstTargetInfo.Value.transform.position).sqrMagnitude < enemyData.AttackRange * enemyData.AttackRange)
                {
                    Vector3 rayStart = GetFirstTargetInfo.Value.transform.position;
                    Vector3 rayEnd = collider.transform.position;
                    Vector3 dir = rayEnd - rayStart;

                    if (Physics.Raycast(rayStart, dir, Vector3.Distance(rayStart, rayEnd), LayerMask.GetMask("CoverPoint"), QueryTriggerInteraction.Ignore))
                    {
                        return point;
                    }
                }
            }
        }

        return null;
    }

    private void EnemyDied()
    {
        enabled = false;
    }

    private bool IsSafeFromPlayer(Vector3 targetPos)
    {
        //Vector3 direction = targetPos - transform.position;
        //float distance = direction.magnitude;

        //if (Physics.Raycast())
        //{

        //}

        return true;

        return false;
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

    private IEnumerator ServerDetectLoop()
    {
        int beforeDetectedCount = 0;

        while (true)
        {
            CheckDetectedServer(); // visibleTargets 리스트를 업데이트
            int currentDetectedCount = visibleTargets.Count;

            if (currentDetectedCount > beforeDetectedCount)
            {
                // 새로운 타겟이 감지되었을 때 (0->1 포함, 1->2 등)
                onTargetDetect?.Invoke();
            }
            else if (currentDetectedCount == 0 && beforeDetectedCount > 0)
            {
                // 타겟을 모두 잃었을 때 ( >0 -> 0 )
                onTargetLosted?.Invoke();
            }

            beforeDetectedCount = currentDetectedCount;

            yield return detectWait;
        }
    }

    private bool CheckDetectedServer()
    {
        visibleTargets.Clear();

        int count = Physics.OverlapSphereNonAlloc(transform.position, isAttackMode ? enemyData.SearchRadius : enemyData.SecondaryViewRadius, colliders, targetMask);

        for (int i = 0; i < count; ++i)
        {
            Transform target = colliders[i].transform;
            Vector3 dir = (target.position - transform.position).normalized;

            bool isInView = false;

            if (isAttackMode) isInView = true;
            else isInView = Vector3.Angle(transform.forward, dir) < enemyData.ViewAngle * 0.5f;

            if (isInView)
            {
                float dst = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dir, dst, obstacleMask))
                    visibleTargets.Add((target, dst));
            }
        }

        return visibleTargets.Count > 0;
    }


    // 제미나이가 작성한 코드
    private void OnDrawGizmosSelected()
    {
        // enemyData가 할당되지 않았으면 그리지 않습니다.
        if (enemyData is null) return;

        // 1. 현재 활성화된 탐지 반경 그리기
        // Application.isPlaying은 게임이 실행 중일 때만 true입니다.
        float currentRadius = Application.isPlaying ?
                                (isAttackMode ? enemyData.SearchRadius : enemyData.SecondaryViewRadius) :
                                enemyData.SecondaryViewRadius; // 실행 중이 아닐 땐 기본값(Secondary) 표시

        Color currentColor = Application.isPlaying && isAttackMode ? Color.red : Color.green;

        Gizmos.color = currentColor;
        Gizmos.DrawWireSphere(transform.position, currentRadius);

        // 2. (선택 사항) 두 개의 반경을 항상 모두 표시하기
        // Gizmos.color = new Color(1, 0, 0, 0.3f); // 빨간색 (Attack)
        // Gizmos.DrawWireSphere(transform.position, enemyData.SearchRadius);
        // Gizmos.color = new Color(1, 1, 0, 0.3f); // 노란색 (Secondary)
        // Gizmos.DrawWireSphere(transform.position, enemyData.SecondaryViewRadius);

        // 3. 시야각(View Angle) 그리기 (Attack 모드가 아닐 때)
        if (!isAttackMode || !Application.isPlaying) // 실행 중이 아닐 때도 표시
        {
            Gizmos.color = Color.cyan;
            Vector3 forward = transform.forward;
            float viewAngle = enemyData.ViewAngle;
            float viewRadius = enemyData.SecondaryViewRadius; // 시야각은 Secondary 반경과 연동

            // 시야각의 양쪽 끝 방향 계산
            Vector3 viewAngleA = DirFromAngle(-viewAngle * 0.5f, false);
            Vector3 viewAngleB = DirFromAngle(viewAngle * 0.5f, false);

            // 시야각 라인 그리기
            Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
            Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
        }

        Gizmos.color = Color.darkKhaki;
        if (visibleTargets is not null)
        {
            foreach (var (target, dst) in visibleTargets)
            {
                if (target is not null)
                {
                    Gizmos.DrawLine(transform.position, target.position);
                }
            }
        }
    }

    // 시야각 계산을 위한 헬퍼(Helper) 함수
    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0f, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
