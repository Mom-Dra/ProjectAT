using UnityEngine;


public class ServerPlayerIdleState : PlayerIdleStateBase
{
    public ServerPlayerIdleState(PlayerStateMachine context) : base(context)
    {
    }

    public override void OnUpdate()
    {
        var enemy = context.FindNearEnemy();
        if (enemy)
        {
            context.ChangeStateServerRpc(PlayerStateMachine.StateId.Attack);
        }
    }
}