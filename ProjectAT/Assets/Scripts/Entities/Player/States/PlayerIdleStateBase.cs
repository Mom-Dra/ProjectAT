using UnityEngine;

public class PlayerIdleStateBase : EntityState
{
    public PlayerIdleStateBase(PlayerStateMachine cxt) : base(cxt)
    {
    }

    public override void Enter()
    {
        context.PlayerController.PlayerIdle();
    }

    public override void Exit()
    {
    }

    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.RightClick:
                context.ChangeState(PlayerStateMachine.StateId.Walk);
                break;
            case PlayerInputType.DesignatedFireKey:
                Debug.Log("idle -> Desginate");
                //context.ChangeState(PlayerStateMachine.StateId.SkillTargeting, 0);
                break;
            default:
                break;
        }
    }

    public override void OnUpdate()
    {
    }
}
