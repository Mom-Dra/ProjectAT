using UnityEngine;


public class ServerPlayerIdleState : PlayerIdleStateBase
{
    public ServerPlayerIdleState(PlayerStateMachine context) : base(context)
    {
    }

    public override void OnUpdate()
    {
       /* var enemy = context.PlayerController.DetectNearEnemy();
        if (enemy)
        {
            context.ChangeState(PlayerStateMachine.StateId.Attack);
        }*/
    }
}