using UnityEditor;
using UnityEngine;

public class PlayerAttackStateBase : EntityState
{
    public PlayerAttackStateBase(PlayerStateMachine cxt) : base(cxt){ }

    public override void Enter()
    {
        context.PlayerController.MyAnim.SetBool("isFiring", true);
    }

    public override void Exit()
    {
        context.PlayerController.MyAnim.SetBool("isFiring", false);
    }

    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.LeftClick:
                //context.ChangeStateServerRpc(PlayerStateMachine.StateId.Walk);
                break;
        }
    }

    public override void OnUpdate()
    {
    }
}
