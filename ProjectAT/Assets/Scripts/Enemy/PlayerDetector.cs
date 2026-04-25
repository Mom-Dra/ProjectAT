using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TargetDetector : MonoBehaviour
{
    // ó���� ���� �þ߿� ��� �Ե� ��
    // �߰��� ���� �þ߿� ��� ���� �� 
    public event System.Action onTargetDetect;

    // ��� ���� �þ߿� ���� ��
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
            if(collider.TryGetComponent(out CoverPoint point) && !point.IsInUse)
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
            CheckDetectedServer(); // visibleTargets ����Ʈ�� ������Ʈ
            int currentDetectedCount = visibleTargets.Count;

            if (currentDetectedCount > beforeDetectedCount)
            {
                // ���ο� Ÿ���� �����Ǿ��� �� (0->1 ����, 1->2 ��)
                onTargetDetect?.Invoke();
            }
            else if (currentDetectedCount == 0 && beforeDetectedCount > 0)
            {
                // Ÿ���� ��� �Ҿ��� �� ( >0 -> 0 )
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

            if (IsTargetStealthed(target)) continue;

            if (!IsInFieldOfView(target)) continue;

            Vector3 dir = (target.position - transform.position).normalized;
            float dst = Vector3.Distance(transform.position, target.position);

            if (!Physics.Raycast(transform.position, dir, dst, obstacleMask))
                visibleTargets.Add((target, dst));
        }

        return visibleTargets.Count > 0;
    }

    private bool IsTargetStealthed(Transform target)
    {
        return target.TryGetComponent(out IStealthable stealthable) && stealthable.IsHidden;
    }

    private bool IsInFieldOfView(Transform target)
    {
        if (isAttackMode) return true; // ���� ��� �� 360�� �ν�

        Vector3 dir = (target.position - transform.position).normalized;
        return Vector3.Angle(transform.forward, dir) < enemyData.ViewAngle * 0.5f;
    }

    // ���̳��̰� �ۼ��� �ڵ�
    private void OnDrawGizmosSelected()
    {
        // enemyData�� �Ҵ���� �ʾ����� �׸��� �ʽ��ϴ�.
        if (enemyData is null) return;

        // 1. ���� Ȱ��ȭ�� Ž�� �ݰ� �׸���
        // Application.isPlaying�� ������ ���� ���� ���� true�Դϴ�.
        float currentRadius = Application.isPlaying ?
                                (isAttackMode ? enemyData.SearchRadius : enemyData.SecondaryViewRadius) :
                                enemyData.SecondaryViewRadius; // ���� ���� �ƴ� �� �⺻��(Secondary) ǥ��

        Color currentColor = Application.isPlaying && isAttackMode ? Color.red : Color.green;

        Gizmos.color = currentColor;
        Gizmos.DrawWireSphere(transform.position, currentRadius);

        // 2. (���� ����) �� ���� �ݰ��� �׻� ��� ǥ���ϱ�
        // Gizmos.color = new Color(1, 0, 0, 0.3f); // ������ (Attack)
        // Gizmos.DrawWireSphere(transform.position, enemyData.SearchRadius);
        // Gizmos.color = new Color(1, 1, 0, 0.3f); // ����� (Secondary)
        // Gizmos.DrawWireSphere(transform.position, enemyData.SecondaryViewRadius);

        // 3. �þ߰�(View Angle) �׸��� (Attack ��尡 �ƴ� ��)
        if (!isAttackMode || !Application.isPlaying) // ���� ���� �ƴ� ���� ǥ��
        {
            Gizmos.color = Color.cyan;
            Vector3 forward = transform.forward;
            float viewAngle = enemyData.ViewAngle;
            float viewRadius = enemyData.SecondaryViewRadius; // �þ߰��� Secondary �ݰ�� ����

            // �þ߰��� ���� �� ���� ���
            Vector3 viewAngleA = DirFromAngle(-viewAngle * 0.5f, false);
            Vector3 viewAngleB = DirFromAngle(viewAngle * 0.5f, false);

            // �þ߰� ���� �׸���
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

    // �þ߰� ����� ���� ����(Helper) �Լ�
    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0f, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
