using UnityEngine;

public class ServerPlayerAttackState : PlayerAttackStateBase
{
    public ServerPlayerAttackState(PlayerStateMachine cxt) : base(cxt)
    {
    }

    public override void OnUpdate()
    {
        var target = context.FindNearEnemy();
        if (target)
        {
            context.AttackEnemy(target);
        }
    }
}
