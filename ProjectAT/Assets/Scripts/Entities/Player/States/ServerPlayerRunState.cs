using UnityEngine;


public class ServerPlayerRunState : PlayerRunStateBase
{
    public ServerPlayerRunState(PlayerStateMachine context) : base(context)
    {
    }

    public override void OnUpdate()
    {
        //if (context.IsArrivedToDest())
            //context.ChangeStateServerRpc(PlayerStateMachine.StateId.Idle);
    }
}