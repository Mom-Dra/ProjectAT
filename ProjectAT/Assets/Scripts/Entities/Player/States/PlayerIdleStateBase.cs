using UnityEngine;

public class PlayerIdleStateBase : EntityState
{
    public PlayerIdleStateBase(PlayerStateMachine cxt) : base(cxt)
    {
    }

    public override void Enter()
    {
        context.MyAgent.ResetPath();
    }

    public override void Exit()
    {
    }

    public override void HandleClickInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.LeftClick:
                context.ChangeStateServerRpc(PlayerStateMachine.StateId.Walk);
                break;
            default:
                break;
        }
    }

    public override void OnUpdate()
    {
    }
}
