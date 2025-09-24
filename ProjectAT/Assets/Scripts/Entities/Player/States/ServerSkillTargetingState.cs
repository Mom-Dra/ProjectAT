using System.Net.Mime;
using UnityEngine;

public class ServerSkillTargetingState : SkillTargetingStateBase
{
    public ServerSkillTargetingState(PlayerStateMachine cxt) : base(cxt) { }

    public override void OnUpdate()
    {
        base.OnUpdate();
        //SkillStrategy.OnTargetingUpdate(context); //마우스 바라보기
    }
}
