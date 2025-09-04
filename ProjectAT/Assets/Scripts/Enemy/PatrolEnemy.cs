using UnityEngine;

public class PatrolEnemy : Enemy
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
            ChangeState(Enemy_State.Patrol);
    }

    protected override void ScanStarted()
    {
        Debug.Log("ScanStarted");
        ChangeState(Enemy_State.Idle);
    }

    protected override void ScanCanceled()
    {
        Debug.Log("ScanCanceled");
        ChangeState(Enemy_State.Patrol);
    }

    internal override void ChangeDefaultState()
    {
        Debug.Log("ChangeDefaultState");
        ChangeState(Enemy_State.Patrol);
    }
}
