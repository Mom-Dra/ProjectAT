using UnityEngine;
using Unity.Netcode;

public class PlayerRunStateBase : EntityState
{
    public PlayerRunStateBase(PlayerStateMachine context) : base(context) { }

    public override void Enter()
    {
        context.MyAgent.speed = context.MyStatus.RunSpeed.Value;
        context.MyAnim.SetBool("isWalking", true);
        context.PlayerMove();
    }

    public override void Exit()
    {
        context.MyAnim.SetBool("isWalking", false);
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
