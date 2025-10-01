using UnityEngine;

public class PlayerWalkStateBase : EntityState
{
    public PlayerWalkStateBase(PlayerStateMachine context) : base(context) { }
    public override void Enter()
    {
        context.PlayerController.PlayerWalk();
    }

    public override void Exit()
    { }

    public override void OnUpdate()
    {
        if (context.PlayerController.IsArrivedDestination())
        {
            context.ChangeState(PlayerStateMachine.StateId.Idle);
        }
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
        }
    }
}
