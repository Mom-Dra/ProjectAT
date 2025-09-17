using System.Net.Mime;
using UnityEngine;

public class ServerSkillTargetingState : SkillTargetingStateBase
{
    public ServerSkillTargetingState(PlayerStateMachine cxt) : base(cxt) { }

    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.LeftClick:
                if (SkillStrategy.TryCommit(context)) //context에서 raycast를 사용하는것이 좋을듯
                {
                    Debug.Log("ServerSKillTargetng: Success");
                    context.ChangeStateServerRpc(PlayerStateMachine.StateId.SkillCasting, 0);
                }
                else
                {
                    context.ChangeStateServerRpc(PlayerStateMachine.StateId.Idle);
                }
                    break;
        }
    }

    public override void OnUpdate()
    {
        SkillStrategy.OnTargetingUpdate(context); //마우스 바라보기
    }
}
