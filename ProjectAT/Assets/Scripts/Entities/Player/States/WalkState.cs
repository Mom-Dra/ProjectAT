using UnityEngine;
using UnityEngine.XR;

public class WalkState : EntityState
{
    public WalkState(PlayerStateMachine context)
    {
        this.context = context;
    }

    public override void Enter()
    {
        context.PlayerMove();
    }

    public override void Exit()
    {
    }

    public override void OnUpdate()
    {
        if (context.IsArrivedToDest())
            context.ChangeState(context.IdleState);
    }

    public override void HandleClickInput()
    {
        Enter();
    }
}
