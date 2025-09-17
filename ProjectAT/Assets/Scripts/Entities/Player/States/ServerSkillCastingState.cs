using System.Diagnostics;

public class ServerSkillCastingState : SkillCastingStateBase
{

    public ServerSkillCastingState(PlayerStateMachine cxt) : base(cxt)
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
        if(context)
        {
            SkillStrategy.OnCastingUpdate(context);
            //context.ChangeStateServerRpc(PlayerStateMachine.StateId.Idle); // 스킬 시전 애니메이션 나오는게 필요함. 어떻게? exacute에서 전환?
        }
    }
}
