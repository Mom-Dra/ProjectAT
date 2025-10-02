using UnityEngine;
using Unity.Netcode;

public class PlayerRunStateBase : EntityState
{
    public PlayerRunStateBase(PlayerStateMachine context) : base(context) { }

    public override void Enter()
    {
        Debug.Log("run");
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
                PlayerStateMachine.StateId nextState = context.PlayerController.CalCulateNextStateByMouseRaycast();
                if (nextState == PlayerStateMachine.StateId.Chase)
                {
                    context.ChaseState.SetChaseState(context.PlayerController.MyWeapon.Radius, PlayerStateMachine.StateId.Attack);
                    context.ChangeState(PlayerStateMachine.StateId.Chase);
                }
                else
                    context.ChangeState(nextState);
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

        if (context.PlayerController.IsInAttackRange(context.PlayerController.SelectedEnemy))
        {
            context.ChangeState(PlayerStateMachine.StateId.Attack);
        }
    }
}
