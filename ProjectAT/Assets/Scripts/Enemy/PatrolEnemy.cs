using UnityEngine;

public class PatrolEnemy : Enemy
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            ChangeState(Enemy_State.Patrol);
        }
    }
}
