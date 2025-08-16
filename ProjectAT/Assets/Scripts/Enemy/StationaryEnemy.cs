using UnityEngine;

public class StationaryEnemy : Enemy
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            ChangeState(Enemy_State.Idle);
        }
    }
}
