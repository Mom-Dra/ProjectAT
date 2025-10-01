using UnityEngine;


public class ServerPlayerWalkState : PlayerWalkStateBase
{
    public ServerPlayerWalkState(PlayerStateMachine context) : base(context)
    {
    }

    public override void OnUpdate()
    {
        /*if (context.IsArrivedToDest())
            context.ChangeStateServerRpc(PlayerStateMachine.StateId.Idle);*/
    }
}