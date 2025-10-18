using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

public class SkillCastingStateBase : EntityState
{
    private PlayerSkillStrategyMap skillStrategyMap;

    public SkillCastingStateBase(PlayerStateMachine context, PlayerSkillStrategyMap skillMap) : base(context) 
    {
        skillStrategyMap = skillMap;
    }
    
    public override void Enter()
    {

    }
    public override void Exit()
    {
        skillStrategyMap.NowActiveSkill.OnFinish();
    }
    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.RightClick:
                //context.PlayerController.SetTargetEnemy(null);
                PlayerStateMachine.StateId nextState = context.PlayerController.CalCulateNextStateByMouseRaycast();
                context.ChangeState(nextState);
                break;
        }
    }
    public override void OnUpdate()
    {
        if(context.PlayerController.SelectedEnemy == null)
        {
            context.ChangeState(PlayerStateMachine.StateId.Idle);
            return;
        }
        skillStrategyMap.NowActiveSkill.OnSkillUpdate();
    }
}