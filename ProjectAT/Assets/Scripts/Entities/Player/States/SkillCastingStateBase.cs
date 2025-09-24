using Unity.VisualScripting;
using UnityEngine;

public class SkillCastingStateBase : EntityState
{
    public ISkill SkillStrategy;

    public SkillCastingStateBase(PlayerStateMachine context) : base(context) { }
    
    public override void Enter()
    {
        Debug.Log("SkillCastingBase");
    }
    public override void Exit()
    {

    }
    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.LeftClick:
                context.ChangeStateServerRpc(PlayerStateMachine.StateId.Walk);
                break;
        }
    }
    public override void OnUpdate()
    {

    }

    public void SetSkillStrategy(ISkill nextStrategy)
    {
        SkillStrategy = nextStrategy;
    }
}