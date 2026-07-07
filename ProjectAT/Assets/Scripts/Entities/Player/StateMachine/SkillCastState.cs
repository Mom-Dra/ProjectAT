using PlayerStateCapabilities;
using UnityEngine;


namespace PlayerStateMachine
{
    public class SkillCastState : PlayerState, IRightClickHandler, IInterruptiblePlayerState
    {

        private SkillContext skillContext;

        private float castTimer = 0.0f;
        private bool isRotationFinished = false;

        public SkillCastState(PlayerController context) : base(context)
        {
        }

        public void SetSkillContext(SkillContext context)
        {
            skillContext = context;
        }

        public override void OnEnter()
        {
            if (skillContext == null || skillContext.SkillToExecute == null)
            {
                context.ChangeState(PlayerStateType.Normal);
                return;
            }

            castTimer = 0.0f;
            isRotationFinished = false;
            context.PlayerMove(context.transform.position, false);
            skillContext.SkillToExecute.OnCastingStart(skillContext);
        }

        public override void OnUpdate()
        {
            if (skillContext.TargetCollider != null && !skillContext.TargetCollider.gameObject.activeInHierarchy)
            {
                CancelCasting();
                return;
            }

            Vector3 lookTarget = skillContext.TargetCollider != null 
                ? skillContext.TargetCollider.transform.position 
                : skillContext.CastedPosition;
            lookTarget.y = context.transform.position.y; 

            if (!isRotationFinished)
            {
                if (context.MyMovementModule.PlayerRotateToward(lookTarget))
                {
                    isRotationFinished = true;
                }
            }
            
            if (!skillContext.SkillToExecute.CanExecute(skillContext))
            {
                ResumeChasing();
                return;
            }

            castTimer += Time.deltaTime;
            if (castTimer >= skillContext.SkillToExecute.CastTime)
            {
                FinishCastingAndExecute();
            }
        }

        public override void OnExit() { }

        private void FinishCastingAndExecute()
        {
            // if (skillContext.SkillToExecute.CanExecute(skillContext))
            // {
            //     skillContext.SkillToExecute.Execute(skillContext);
            //     context.MySkillModule.SetSkillCooldownTimer(skillContext.SkillToExecute);
            //     CancelCasting(); 
            // }
            // else
            // {
            //     ResumeChasing();
            // }
            if (!skillContext.SkillToExecute.CanExecute(skillContext))
            {
                ResumeChasing();
                return;
            }

            skillContext.SkillToExecute.OnCastingEnd(skillContext);
            // SkillExecuteState executeState = context.GetState(PlayerStateType.SkillExecute) as SkillExecuteState;
            // executeState.SetSkillContext(skillContext);
            context.ChangeState(PlayerStateType.SkillExecute);
        }

        // 다시 추적 상태로 돌아가는 로직
        private void ResumeChasing()
        {
            EndCasting();

            // SkillChaseState skillChaseState = context.GetState(PlayerStateType.SkillChase) as SkillChaseState; //굳이 필요한 로직인가?
            // skillChaseState.SetSkillContext(skillContext);

            context.ChangeState(PlayerStateType.SkillChase);
        }

        private void CancelCasting(bool shouldChangeState = true)
        {
            EndCasting();
            context.MySkillModule.CancelCurrentSkill();

            if (shouldChangeState)
            {
                context.ChangeState(PlayerStateType.Normal);
            }
        }

        private void EndCasting()
        {
            if (skillContext == null || skillContext.SkillToExecute == null)
            {
                return;
            }

            skillContext.SkillToExecute.OnCastingEnd(skillContext);
        }

        public void Interrupt()
        {
            CancelCasting(false);
        }

        public void OnRightClick(RaycastHit castedObject)
        {
            switch (castedObject.collider.gameObject.layer)
            {
                case 6: //Ground Layer
                    context.PlayerMoveWithIndicator(castedObject.point, false);
                    break;
                case 7: //Enemy Layer
                    context.SetTargetEnemy(castedObject.collider.GetComponent<Enemy>());
                    break;
                case 10: //Indicator Layer
                    context.PlayerMoveWithIndicator(castedObject.point, true);
                    break;
                default:
                    break;
            }

            CancelCasting();
        }
    }
}
