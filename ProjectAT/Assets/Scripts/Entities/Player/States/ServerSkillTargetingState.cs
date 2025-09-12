using System.Net.Mime;
using UnityEngine;

public class ServerSkillTargetingState : EntityState
{
    private readonly ISkill skillStrategy;
    public ServerSkillTargetingState(PlayerStateMachine cxt, ISkill skillStrategy) : base(cxt)
    {
        this.skillStrategy = skillStrategy;
    }

    public override void Enter()
    {
        //skillStrategy.OnAimEnter(context);
    }

    public override void Exit()
    {
        //skillStrategy.OnAimExit(context);
    }

    public override void HandleClickInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.LeftClick:
                if (skillStrategy.TryCommit(context)) //context에서 raycast를 사용하는것이 좋을듯
                {
                    //context.ChangeStateServerRpc(PlayerStateMachine.StateId.SkillCasting);
                }
                break;
        }
    }

    public override void OnUpdate()
    {
        //skillStrategy.OnAimUpdate(context); //마우스 바라보기
    }
}
