using UnityEngine;
using PlayerStateCapabilities;


namespace PlayerStateMachine
{
    public class SkillExecuteState : PlayerState, IRightClickHandler, IInterruptiblePlayerState
    {
        private SkillContext nowActivatedSkillContext;
        private bool finished;

        
        public SkillExecuteState(PlayerController context) : base(context) 
        {
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

            if (nowActivatedSkillContext.TargetCollider != null && !nowActivatedSkillContext.TargetCollider.gameObject.activeInHierarchy)
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

        private void Finish(bool success, bool shouldChangeState = true)
        {
            if (finished)
                return;

            finished = true;

            SkillContext skillContext = nowActivatedSkillContext;
            Skill skill = skillContext?.SkillToExecute;

            if (skill != null)
            {
                skill.OnExecuteEnd(skillContext);
            }

            if (success && skill != null)
            {
                context.MySkillModule.NotifySkillExecuted(skill, skillContext.SkillNumber);
            }

            context.MySkillModule.CancelCurrentSkill();

            if (shouldChangeState)
            {
                context.ChangeState(PlayerStateType.Normal);
            }
        }

        public void SetSkillContext(SkillContext context)
        {
            nowActivatedSkillContext = context;
        }


        public void OnRightClick(RaycastHit castedObject)
        {
            if (nowActivatedSkillContext == null || nowActivatedSkillContext.SkillToExecute == null)
                return;

            Finish(false);
        }

        public void Interrupt()
        {
            Finish(false, false);
        }
    }
}
