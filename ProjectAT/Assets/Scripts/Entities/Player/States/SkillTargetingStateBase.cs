using UnityEngine;

public class SkillTargetingStateBase : EntityState
{
    public ISkill SkillStrategy;

    public SkillTargetingStateBase(PlayerStateMachine context) : base(context) { }

    public override void Enter()
    {
    }

    public override void Exit()
    {

    }

    public override void HandleInput(PlayerInputType type)
    {

    }

    public override void OnUpdate()
    {

    }

    public void SetSkillStrategy(ISkill strategy)
    {
        SkillStrategy = strategy;
    }
}
