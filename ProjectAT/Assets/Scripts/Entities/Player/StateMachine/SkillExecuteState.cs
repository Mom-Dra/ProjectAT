using UnityEngine;
using PlayerStateCapabilities;


namespace PlayerStateMachine
{
    public class SkillExecuteState : PlayerState, IRightClickHandler
    {
        private PlayerSkillModule mySkillModule;

        private SkillContext nowActivatedSkillContext;
        private bool finished;

        
        public SkillExecuteState(PlayerController context) : base(context) 
        {
            mySkillModule = context.MySkillModule;
        }

        public override void OnEnter()
        {
            finished = false;

            if (nowActivatedSkillContext == null || nowActivatedSkillContext.SkillToExecute == null)
            {
                context.ChangeState(PlayerStateType.Normal);
                return;
            }

            context.PlayerMove(context.transform.position, false);
            nowActivatedSkillContext.SkillToExecute.OnExecuteStart(nowActivatedSkillContext);
        }

        public override void OnExit()
        {
            
        }

        public override void OnUpdate()
        {
            if (finished || nowActivatedSkillContext == null || nowActivatedSkillContext.SkillToExecute == null)
                return;

            if (nowActivatedSkillContext.TargetObject != null && !nowActivatedSkillContext.TargetObject.activeInHierarchy)
            {
                Finish(false);
                return;
            }

            Skill skill = nowActivatedSkillContext.SkillToExecute;

            skill.OnExecuteUpdate(nowActivatedSkillContext, Time.deltaTime);

            if (skill.IsExecutionFinished(nowActivatedSkillContext))
            {
                Finish(true);
            }
        }

        private void Finish(bool success)
        {
            if (finished)
                return;

            finished = true;

            Skill skill = nowActivatedSkillContext.SkillToExecute;
            skill.OnExecuteEnd(nowActivatedSkillContext);

            if (success)
            {
                context.MySkillModule.SetSkillCooldownTimer(skill);
            }

            context.MySkillModule.CancelCurrentSkill();
            context.ChangeState(PlayerStateType.Normal);
        }

        public void SetSkillContext(SkillContext context)
        {
            nowActivatedSkillContext = context;
        }


        public void OnRightClick(RaycastHit castedObject)
        {
            if (nowActivatedSkillContext == null || nowActivatedSkillContext.SkillToExecute == null)
                return;

            nowActivatedSkillContext.SkillToExecute.OnExecuteEnd(nowActivatedSkillContext);
            mySkillModule.CancelCurrentSkill();
            context.ChangeState(PlayerStateType.Normal);
        }
    }
}