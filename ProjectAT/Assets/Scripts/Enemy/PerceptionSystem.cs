using System;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionSystem : MonoBehaviour
{
    public event Action<IPerceivable> onTargetDetected;
    public event Action<IPerceivable> onTargetLost;

    [SerializeField] private float detectInterval = 0.2f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    private readonly List<DetectedTarget> visibleTargets = new List<DetectedTarget>();
    private readonly List<IPerceivable> previousTargets = new List<IPerceivable>();
    private readonly Collider[] colliders = new Collider[16];

    private float viewAngle;
    private float searchRadius;
    private float secondaryViewRadius;

    private bool isAttackMode;
    private float scanTimer;

    public IReadOnlyList<DetectedTarget> VisibleTargets => visibleTargets;
    public DetectedTarget? NearestTarget => visibleTargets.Count == 0 ? null : visibleTargets[0];
    public bool HasAnyTarget => visibleTargets.Count > 0;
    public LayerMask ObstacleMask => obstacleMask;

    public float ViewAngle => viewAngle;
    public float SecondaryViewRadius => secondaryViewRadius;

    private void Awake()
    {
        scanTimer = UnityEngine.Random.Range(0f, detectInterval);
    }

    public void Initialize(float viewAngle, float searchRadius, float secondaryViewRadius)
    {
        this.viewAngle = viewAngle;
        this.searchRadius = searchRadius;
        this.secondaryViewRadius = secondaryViewRadius;
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Update()
    {
        Scan();
        DebugRay();
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
        scanTimer += Time.deltaTime;

        if (scanTimer < detectInterval) return;
        scanTimer = 0f;

        previousTargets.Clear();

        foreach (DetectedTarget target in visibleTargets)
            previousTargets.Add(target.Perceivable);

        visibleTargets.Clear();

        float radius = isAttackMode ? searchRadius : secondaryViewRadius;
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, colliders, targetMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; ++i)
        {
            if (!colliders[i].TryGetComponent(out IPerceivable perceivable) || !perceivable.IsValidTarget) continue;
            if (!IsInFieldOfView(perceivable.Transform)) continue;
            if (!perceivable.IsValidTarget) continue;
            if (!HasLineOfSight(perceivable.Transform, out float distance)) continue;

            visibleTargets.Add(new DetectedTarget(perceivable, distance));
        }

        visibleTargets.Sort(CompareByDistance);

        EmitDiffEvents();
    }

    private void EmitDiffEvents()
    {
        // 이전 X, 이번 O
        foreach (var target in visibleTargets)
        {
            if (!ContainsReference(previousTargets, target.Perceivable))
            {
                Debug.Log("onTargetDetected");
                onTargetDetected?.Invoke(target.Perceivable);
            }
        }

        // 이전 O, 이번 X
        foreach (var target in previousTargets)
        {
            if (!ContainsPerceivable(visibleTargets, target))
            {
                Debug.Log("onTargetLost");
                onTargetLost?.Invoke(target);
            }
        }
    }

    // public CoverPoint FindBestCover()
    // {
    //     Collider[] hits = Physics.OverlapSphere(transform.position, enemyData.SearchRadius, LayerMask.GetMask("CoverPoint"));
    //     Array.Sort(hits, (x, y) =>
    //     {
    //         float dis1 = (transform.position - x.transform.position).sqrMagnitude;
    //         float dis2 = (transform.position - y.transform.position).sqrMagnitude;

    //         return dis1.CompareTo(dis2);
    //     });

    //     var first = GetFirstTargetInfo;

    //     Vector3 targetPos = first.Value.perceivable.Transform.position;
    //     float attackRangeSquare = Mathf.Pow(enemyData.AttackRange, 2f);

    //     foreach (Collider collider in hits)
    //     {
    //         if (collider.TryGetComponent(out CoverPoint point) && !point.IsOccupied)
    //         {
    //             if ((collider.transform.position - targetPos).sqrMagnitude < attackRangeSquare)
    //             {
    //                 Vector3 rayStart = targetPos;
    //                 Vector3 rayEnd = collider.transform.position;
    //                 Vector3 dir = rayEnd - rayStart;

    //                 if (Physics.Raycast(rayStart, dir, Vector3.Distance(rayStart, rayEnd), LayerMask.GetMask("CoverPoint"), QueryTriggerInteraction.Ignore))
    //                     return point;
    //             }
    //         }
    //     }

    //     return null;
    // }


    private bool IsInFieldOfView(Transform target)
    {
        if (isAttackMode) return true;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < float.Epsilon) return true;

        return Vector3.Angle(transform.forward, dir.normalized) < viewAngle * 0.5f;
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

    private void DebugRay()
    {
        foreach (DetectedTarget detectedTarget in visibleTargets)
        {
            // Vector3 targetPosition = (detectedTarget.Perceivable.Transform.position - transform.position)
            Debug.DrawLine(transform.position, detectedTarget.Perceivable.Transform.position, Color.red);
        }
    }

    private static int CompareByDistance(DetectedTarget a, DetectedTarget b) => a.Distance.CompareTo(b.Distance);

    private static bool ContainsReference(List<IPerceivable> list, IPerceivable target)
    {
        foreach (IPerceivable currTarget in list)
            if (ReferenceEquals(currTarget, target)) return true;

        return false;
    }

    private static bool ContainsPerceivable(List<DetectedTarget> list, IPerceivable target)
    {
        foreach (var currTarget in list)
            if (ReferenceEquals(currTarget.Perceivable, target)) return true;

        return false;
    }

    private void Clear()
    {
        for (int i = 0; i < visibleTargets.Count; ++i)
            onTargetLost?.Invoke(visibleTargets[i].Perceivable);

        visibleTargets.Clear();
        previousTargets.Clear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float r = secondaryViewRadius;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, r);

        if (!isAttackMode && viewAngle > 0f)
        {
            Vector3 fwd = transform.forward;
            Quaternion lq = Quaternion.AngleAxis(-viewAngle * 0.5f, Vector3.up);
            Quaternion rq = Quaternion.AngleAxis(+viewAngle * 0.5f, Vector3.up);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, lq * fwd * r);
            Gizmos.DrawRay(transform.position, rq * fwd * r);
        }
    }
#endif
}
