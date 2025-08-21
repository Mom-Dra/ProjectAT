using UnityEngine;
using Unity.Netcode;

public class PlayerRunStateBase : EntityState
{
    public PlayerRunStateBase(PlayerStateMachine context) : base(context) { }

    public override void Enter()
    {
        context.MyAgent.speed = context.MyStatus.RunSpeed.Value;
        context.PlayerMove();
    }

    public override void Exit()
    {
    }

    public override void OnUpdate()
    {
    }

    public override void HandleClickInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.LeftClick:
                if (context.IsClickSamePosition())
                {
                    context.ChangeStateServerRpc(PlayerStateMachine.StateId.Run);
                }
                else
                {
                    context.ChangeStateServerRpc(PlayerStateMachine.StateId.Walk);
                }
                break;
            default:
                break;
        }
    }
}
