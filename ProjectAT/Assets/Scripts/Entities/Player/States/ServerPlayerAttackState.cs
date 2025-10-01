using UnityEngine;

public class ServerPlayerAttackState : PlayerAttackStateBase
{
    public ServerPlayerAttackState(PlayerStateMachine cxt) : base(cxt)
    {
    }

    public override void OnUpdate()
    {
        var target = context.PlayerController.FindNearEnemy();
        if (target)
        {
            context.PlayerController.AttackEnemy(target, 10);
        }
        else
        //context.ChangeStateServerRpc(PlayerStateMachine.StateId.Idle);
        { }
    }
}
