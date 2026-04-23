using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TargetDetector : MonoBehaviour
{
    public event Action<IPerceivable> onTargetDetect;
    public event Action<IPerceivable> onTargetLosted;

    [SerializeField] private float detectInterval = 0.2f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    private readonly List<(IPerceivable perceivable, float distance)> visibleTargets = new List<(IPerceivable perceivable, float distance)>();
    private readonly List<IPerceivable> previousTargets = new List<IPerceivable>();
    private readonly Collider[] colliders = new Collider[16];

    private bool isAttackMode;
    private Enemy enemy;
    private EnemyData enemyData;
    private EntityStatus entityStatus;

    private float scanTimer;

    public LayerMask ObstacleMask => obstacleMask;
    public float ViewAngle => enemyData?.ViewAngle ?? 0f;
    public bool IsAttackMode => isAttackMode;

    public IReadOnlyList<(IPerceivable perceivable, float distance)> VisibleTargets => visibleTargets;
    public (IPerceivable perceivable, float distance)? GetFirstTargetInfo => visibleTargets.Count == 0 ? null : visibleTargets[0];

    private void Awake()
    {
        entityStatus = GetComponent<EntityStatus>();
        scanTimer = UnityEngine.Random.Range(0f, detectInterval);
    }

    private void Update()
    {
        scanTimer += Time.deltaTime;

        if (scanTimer < detectInterval) return;
        scanTimer = 0f;

        Scan();
    }

    private void OnEnable()
    {
        entityStatus.onDeath += EnemyDied;
    }

    private void OnDisable()
    {
        entityStatus.onDeath -= EnemyDied;

        Clear();
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

    private void Scan()
    {
        previousTargets.Clear();

        foreach (var target in visibleTargets)
            previousTargets.Add(target.perceivable);

        visibleTargets.Clear();

        float radius = isAttackMode ? enemyData.SearchRadius : enemyData.SecondaryViewRadius;
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, colliders, targetMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; ++i)
        {
            if (!colliders[i].TryGetComponent(out IPerceivable perceivable) || !perceivable.IsValidTarget) continue;
            if (!IsInFieldOfView(perceivable.Transform)) continue;
            if (!perceivable.IsValidTarget) continue;
            if (!IsInFieldOfView(perceivable.Transform)) continue;
            if (!HasLineOfSight(perceivable.Transform, out float distance)) continue;

            visibleTargets.Add((perceivable, distance));
        }

        visibleTargets.Sort(CompareByDistance);

        EmitDiffEvents();
    }

    private void EmitDiffEvents()
    {
        foreach (var target in visibleTargets)
        {
            if (!ContainsReference(previousTargets, target.perceivable))
            {
                onTargetDetect?.Invoke(target.perceivable);
            }
        }

        foreach (var target in previousTargets)
        {
            if (!ContainsPerceivable(visibleTargets, target))
                onTargetLosted?.Invoke(target);
        }
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

        var first = GetFirstTargetInfo;

        Vector3 targetPos = first.Value.perceivable.Transform.position;
        float attackRangeSquare = Mathf.Pow(enemyData.AttackRange, 2f);

        foreach (Collider collider in hits)
        {
            if (collider.TryGetComponent(out CoverPoint point) && !point.IsOccupied)
            {
                if ((collider.transform.position - targetPos).sqrMagnitude < attackRangeSquare)
                {
                    Vector3 rayStart = targetPos;
                    Vector3 rayEnd = collider.transform.position;
                    Vector3 dir = rayEnd - rayStart;

                    if (Physics.Raycast(rayStart, dir, Vector3.Distance(rayStart, rayEnd), LayerMask.GetMask("CoverPoint"), QueryTriggerInteraction.Ignore))
                        return point;
                }
            }
        }

        return null;
    }


    private bool IsInFieldOfView(Transform target)
    {
        if (isAttackMode) return true;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < float.Epsilon) return true;

        return Vector3.Angle(transform.forward, dir.normalized) < enemyData.ViewAngle * 0.5f;
    }

    private bool HasLineOfSight(Transform target, out float distance)
    {
        Vector3 from = transform.position;
        Vector3 to = target.position;
        Vector3 delta = to - from;

        distance = delta.magnitude;

        if (distance < float.Epsilon) return true;

        Vector3 dir = delta / distance;

        return !Physics.Raycast(from, dir, distance, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private static int CompareByDistance((IPerceivable perceivable, float distance) a, (IPerceivable perceivable, float distance) b) => a.distance.CompareTo(b.distance);

    private static bool ContainsReference(List<IPerceivable> list, IPerceivable target)
    {
        foreach (IPerceivable currTarget in list)
            if (ReferenceEquals(currTarget, target)) return true;

        return false;
    }

    private static bool ContainsPerceivable(List<(IPerceivable perceivable, float distance)> list, IPerceivable target)
    {
        foreach (var currTarget in list)
            if (ReferenceEquals(currTarget.perceivable, target)) return true;

        return false;
    }

    private void EnemyDied()
    {
        enabled = false;
    }

    private void Clear()
    {
        for (int i = 0; i < visibleTargets.Count; ++i)
            onTargetLosted?.Invoke(visibleTargets[i].perceivable);

        visibleTargets.Clear();
        previousTargets.Clear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (enemyData is null) return;
        float r = isAttackMode ? enemyData.SearchRadius : enemyData.SecondaryViewRadius;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, r);

        if (!isAttackMode && enemyData.ViewAngle > 0f)
        {
            Vector3 fwd = transform.forward;
            Quaternion lq = Quaternion.AngleAxis(-enemyData.ViewAngle * 0.5f, Vector3.up);
            Quaternion rq = Quaternion.AngleAxis(+enemyData.ViewAngle * 0.5f, Vector3.up);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, lq * fwd * r);
            Gizmos.DrawRay(transform.position, rq * fwd * r);
        }
    }
#endif
}
