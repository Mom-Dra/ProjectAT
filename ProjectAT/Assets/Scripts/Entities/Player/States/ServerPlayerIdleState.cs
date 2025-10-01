using UnityEngine;


public class ServerPlayerIdleState : PlayerIdleStateBase
{
    public ServerPlayerIdleState(PlayerStateMachine context) : base(context)
    {
    }

    public override void OnUpdate()
    {
        var enemy = context.PlayerController.FindNearEnemy();
        if (enemy)
        {
            context.ChangeState(PlayerStateMachine.StateId.Attack);
        }
    }
}