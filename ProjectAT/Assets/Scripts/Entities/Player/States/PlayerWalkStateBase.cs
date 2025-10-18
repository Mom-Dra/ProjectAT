using UnityEngine;

public class PlayerWalkStateBase : EntityState
{
    public PlayerWalkStateBase(PlayerStateMachine context) : base(context) { }
    public override void Enter()
    {
        Debug.Log("Walk");
        context.PlayerController.PlayerWalk();
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
                    context.ChangeState(nextState);
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
        if (context.PlayerController.IsArrivedDestination())
        {
            context.ChangeState(PlayerStateMachine.StateId.Idle);
        }
    }
}
