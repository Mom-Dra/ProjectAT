using UnityEngine;
using UnityEngine.InputSystem;

public class IdleState : EntityState
{
    public IdleState(PlayerStateMachine context)
    {
        this.context = context;
    }


    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void OnUpdate()
    {

    }

    public override void HandleClickInput()
    {
        context.ChangeState(context.WalkState);
    }
}
