public class ServerSkillCastingState : EntityState
{
    private readonly ISkill skillStrategy;

    public ServerSkillCastingState(PlayerStateMachine cxt, ISkill skillStrategy) : base(cxt)
    {
        this.skillStrategy = skillStrategy;
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {

    }

    public override void HandleClickInput(PlayerInputType type)
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
        if(context)
        {
            skillStrategy.OnUpdate(context);
            //context.ChangeStateServerRpc(PlayerStateMachine.StateId.Idle); // 스킬 시전 애니메이션 나오는게 필요함. 어떻게? exacute에서 전환?
        }
    }
}
