using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : MonoBehaviour
{
    [SerializeField]
    private PlayerId playerId;

    private EntityStatus entityStatus;

    private void Awake()
    {
        entityStatus = GetComponent<EntityStatus>();
    }

    private void OnEnable()
    {
        entityStatus.onDeath += PlayerDied;
    }

    private void OnDisable()
    {
        entityStatus.onDeath -= PlayerDied;
    }

    private void PlayerDied()
    {
        Managers.Instance.EventManager.Publish(EventType.PlayerDied, playerId);
    }
}
