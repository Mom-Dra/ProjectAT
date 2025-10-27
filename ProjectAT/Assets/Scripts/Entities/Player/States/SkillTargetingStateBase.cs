using UnityEngine;

public class SkillTargetingStateBase : EntityState
{
    private PlayerSkillStrategyMap skillStrategyMap;

    public SkillTargetingStateBase(PlayerStateMachine context, PlayerSkillStrategyMap skillMap) : base(context)
    {
        skillStrategyMap = skillMap;
    }

    public override void Enter()
    {
        Debug.Log("Skill Targetting Enter");
        //UI.ON?
    }

    public override void Exit()
    {
        //UI.OFF?
    }

    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.LeftClick:
                Debug.Log("Set Target Start");
                if (skillStrategyMap.NowActiveSkill.SelectTarget())
                {
                    Debug.Log("Set Target Success");
                    context.ChangeState(PlayerStateMachine.StateId.SkillCasting);
                }
                else
                {
                    Debug.Log("Set Target Fail");
                    context.ChangeState(PlayerStateMachine.StateId.Idle);
                }
                break;
            default:
                context.ChangeState(PlayerStateMachine.StateId.Idle);
                break;
        }
    }

    public override void OnUpdate()
    {

    }
}
