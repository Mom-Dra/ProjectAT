using PlayerStatusCapabilities;
using UnityEngine;


namespace PlayerStateMachine
{
    public class SkillCastState : PlayerState, IRightClickHandler
    {
        private PlayerSkillModule mySkillModule;
        private PlayerMovementModule myMovementModule;
        private SkillContext skillContext;

        private float castTimer = 0.0f;
        private bool isRotationFinished = false;

        public SkillCastState(PlayerController context) : base(context)
        {
            mySkillModule = context.MySkillModule;
            myMovementModule = context.MyMovementModule;
        }

        public void SetSkillContext(SkillContext context)
        {
            skillContext = context;
        }

        public override void OnEnter()
        {
            mySkillModule.CancelTargettingMode();
            
            castTimer = 0.0f;
            isRotationFinished = false;
            context.PlayerMove(context.transform.position, false);
        }

        public override void OnUpdate()
        {
            // 1. 타겟 파괴/비활성화 체크 (이건 재추적이 불가능하므로 취소)
            if (skillContext.TargetObject != null && !skillContext.TargetObject.activeInHierarchy)
            {
                CancelCasting();
                return;
            }

            // 2. 목적지 및 회전 로직
            Vector3 lookTarget = skillContext.TargetObject != null 
                ? skillContext.TargetObject.transform.position 
                : skillContext.CastedPosition;
            lookTarget.y = context.transform.position.y; 

            if (!isRotationFinished)
            {
                if (context.MyMovementModule.PlayerRotateToward(lookTarget))
                {
                    isRotationFinished = true;
                    //context.MyAnimModule.PlaySkillAnimation(skillContext.SkillToExecute.skillData.AnimTriggerName);
                }
                else return; 
            }

            // 3. 실시간 유효성 체크 (캐스팅 도중 적이 도망갔는지 확인)
            // 캐스팅 중에도 매 프레임 CanExecute를 체크하여, 조건에 부합하지 않으면 즉시 다시 추적합니다.
            if (!skillContext.SkillToExecute.CanExecute(skillContext))
            {
                ResumeChasing();
                return;
            }

            // 4. 캐스팅 타이머
            castTimer += Time.deltaTime;
            if (castTimer >= skillContext.SkillToExecute.CastTime)
            {
                FinishCastingAndExecute();
            }
        }

        public override void OnExit()
        {
        }

         // 캐스팅 시간이 성공적으로 모두 끝났을 때 호출되는 함수
        private void FinishCastingAndExecute()
        {
            // 마지막 순간에도 한 번 더 체크
            if (skillContext.SkillToExecute.CanExecute(skillContext))
            {
                skillContext.SkillToExecute.Execute(skillContext);
                context.MySkillModule.SetSkillCooldownTimer(skillContext.SkillToExecute);
                
                context.MySkillModule.CancelCurrentSkill(); 
                context.ChangeState(PlayerStateType.Normal);
            }
            else
            {
                // 실행 직전에 조건이 깨졌다면 다시 추적 상태로 보냄
                ResumeChasing();
            }
        }

        // ★ 핵심: 다시 추적 상태로 돌아가는 로직
        private void ResumeChasing()
        {
            SkillChaseState skillChaseState = context.GetState(PlayerStateType.SkillChase) as SkillChaseState; //굳이 필요한 로직인가?
            skillChaseState.SetSkillContext(skillContext);

            context.ChangeState(PlayerStateType.SkillChase);
        }

        private void CancelCasting()
        {
            context.MySkillModule.CancelCurrentSkill();
            context.ChangeState(PlayerStateType.Normal);
        }

        public void OnRightClick(RaycastHit hit)
        {
            CancelCasting();
        }
    }
}
