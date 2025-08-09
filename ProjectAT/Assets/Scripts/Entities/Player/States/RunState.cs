using UnityEngine;

public class RunState : EntityState
{
    public RunState(PlayerStateMachine context)
    {
        this.context = context;

    }

    public override void Enter()
    {
        context.Agent.speed = context.Status.RunSpeed;
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
        if (!context.IsClickSamePosition())
        {
            context.ChangeState(context.WalkState);
        }
        else
        {
            context.PlayerMove();
        }
    }

}
