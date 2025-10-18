using UnityEngine;

public class PlayerIdleStateBase : EntityState
{
    public PlayerIdleStateBase(PlayerStateMachine cxt) : base(cxt)
    {
    }

    public override void Enter()
    {
        context.PlayerController.StopMoving();
    }

    public override void Exit()
    {
    }

    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.RightClick:
                PlayerStateMachine.StateId nextState = context.PlayerController.CalCulateNextStateByMouseRaycast();
                if (nextState == PlayerStateMachine.StateId.Chase)
                {
                    context.ChaseState.SetChaseState(context.PlayerController.MyWeapon.Radius, PlayerStateMachine.StateId.Attack);
                    context.ChangeState(PlayerStateMachine.StateId.Chase);
                }
                else
                {
                    context.ChangeState(nextState);
                }
                break;
            case PlayerInputType.DesignatedFireKey:
                context.ChangeState(PlayerStateMachine.StateId.SkillTargeting);
                break;
            default:
                break;
        }
    }

    public override void OnUpdate()
    {
        Enemy enemy = context.PlayerController.FindNearestEnemy();
        if (enemy)
        {
            context.PlayerController.SetTargetEnemy(enemy);
            context.ChangeState(PlayerStateMachine.StateId.Attack);
        }
    }
}
