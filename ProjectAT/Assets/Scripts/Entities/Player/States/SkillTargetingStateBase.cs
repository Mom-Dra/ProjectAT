using UnityEngine;

public class SkillTargetingStateBase : EntityState
{
    public ISkill SkillStrategy;

    public SkillTargetingStateBase(PlayerStateMachine context) : base(context) { }

    public override void Enter()
    {
        SkillStrategy.OnTargetingEnter(context);
    }

    public override void Exit()
    {
        SkillStrategy.OnTargetingExit(context);
    }
    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.LeftClick:
                if (SkillStrategy.TryCommit(context)) //context에서 raycast를 사용하는것이 좋을듯
                {
                    Debug.Log("SKillTargetng: Success");
                    //context.ChangeState(PlayerStateMachine.StateId.SkillCasting, 0);
                }
                else
                {
                    context.ChangeState(PlayerStateMachine.StateId.Idle);
                }
                break;
        }
    }

    public override void OnUpdate()
    {
        SkillStrategy.OnTargetingUpdate(context); //마우스 바라보기
    }

    public void SetSkillStrategy(ISkill strategy)
    {
        SkillStrategy = strategy;
    }
}
