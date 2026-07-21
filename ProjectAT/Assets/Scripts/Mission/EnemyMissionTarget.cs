using UnityEngine;

[RequireComponent(typeof(EntityStatus))]
public class EnemyMissionTarget : MonoBehaviour
{
    [Header("Mission Settings")]
    [SerializeField]
    private EnemyIdentity enemyIdentity;

    private EntityStatus entityStatus;

    private void Awake()
    {
        entityStatus = GetComponent<EntityStatus>();
    }

    private void OnEnable()
    {
        entityStatus.onDeath += TargetDied;
    }

    private void OnDisable()
    {
        entityStatus.onDeath -= TargetDied;
    }

    private void TargetDied()
    {
        // InGameManager.Instance.EventManager.TriggerTarget(targetID);
        InGameManager.Instance.EventManager.Publish(EventType.TargetDied, enemyIdentity);
    }
}
