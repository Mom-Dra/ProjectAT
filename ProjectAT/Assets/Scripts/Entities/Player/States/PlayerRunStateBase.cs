using UnityEngine;
using Unity.Netcode;

public class PlayerRunStateBase : EntityState
{
    public PlayerRunStateBase(PlayerStateMachine context) : base(context) { }

    public override void Enter()
    {
        context.PlayerController.PlayerRun();
    }

    public override void Exit()
    {
    }

    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.RightClick:
                if (context.PlayerController.IsClickSameDestination())
                {
                    context.ChangeState(PlayerStateMachine.StateId.Run);
                }
                else
                {
                    context.ChangeState(PlayerStateMachine.StateId.Walk);
                }
                break;
            default:
                break;
        }
    }

    public override void OnUpdate()
    {
        if (context.PlayerController.IsArrivedDestination())
        {
            context.ChangeState(PlayerStateMachine.StateId.Idle);
        }
    }
}
