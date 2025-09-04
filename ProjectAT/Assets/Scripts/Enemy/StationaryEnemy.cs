using UnityEngine;

public class StationaryEnemy : Enemy
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
            ChangeState(Enemy_State.Idle);
    }

    protected override void ScanCanceled()
    {

    }

    protected override void ScanStarted()
    {

    }

    internal override void ChangeDefaultState()
    {
        ChangeState(Enemy_State.Idle);
    }
}
