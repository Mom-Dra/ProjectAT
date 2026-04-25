using UnityEngine;
using PlayerStatusCapabilities;

namespace PlayerStateMachine
{
    public class CoverState : PlayerState, ILeftClickHandler, IRightClickHandler, ISkillInputHandler
    {

        private PlayerSkillModule mySkillModule;
        private PlayerInteractionModule myInteractionModule;
        private PlayerAnimator myAnimationModule;
        private BuffModule myBuffModule;     
        
        public CoverState(PlayerController playerController) : base(playerController)
        {
            mySkillModule = context.MySkillModule;
            myInteractionModule = context.MyInteractionModule;
            myAnimationModule = context.MyAnimModule;
            myBuffModule = context.GetComponent<BuffModule>();
        }

        public override void OnEnter()
        {
            Debug.Log("CoverState 진입: 엄폐 자세를 취하고 방어력/명중률 버프를 얻습니다.");

            // 1. 애니메이션 전환 (엄폐 자세)
            // myAnimationModule.SetBool("IsCovering", true);

            // 2. 스탯 보너스 부여
            // context.MyStatModule.AddDefense(CoverDefenseBonus);
            // context.MyStatModule.AddAccuracy(CoverAccuracyBonus);
            
            myAnimationModule.SetCrouch(true);
        }

        public override void OnExit()
        {
            Debug.Log("CoverState 종료: 엄폐를 해제하고 버프를 원상복구합니다.");

            // 1. 애니메이션 전환 해제
            // myAnimationModule.SetBool("IsCovering", false);

            // 2. 스탯 보너스 회수 (어떤 이유로 상태를 나가든 100% 보장됨!)
            // context.MyStatModule.RemoveDefense(CoverDefenseBonus);
            // context.MyStatModule.RemoveAccuracy(CoverAccuracyBonus);

            IInteractable occupiedCoverPoint = myInteractionModule.CurrentInteractTarget;
            if(occupiedCoverPoint != null && occupiedCoverPoint.CurrentInteractor == context.gameObject)
            {
                occupiedCoverPoint.UnLock();
            }
            myInteractionModule.CurrentInteractTarget = null; // 엄폐 상태에서 나갈 때는 현재 상호작용 타겟 초기화. (엄폐 상태에서만 상호작용 타겟이 엄폐 지점이므로)
            myAnimationModule.SetCrouch(false);
        }

        public override void OnUpdate()
        {
            if (context.SelectedEnemy != null)
            {
                if (!context.TryExecuteAttack()) // 1. 엄폐 중에도 적이 사거리에 있으면 공격 수행
                {
                    context.AimingEnemy(false); // 2. 적이 멀어졌다면? 엄폐 풀기 로직 실행!
                    //context.ChangeState(PlayerStateType.Normal); // 상태를 Normal로 바꾸면, 다음 프레임부터 NormalState가 알아서 ChaseEnemy()를 실행함
                }
            }
        }

        public void OnLeftClick(RaycastHit castedObject)
        {
            if (mySkillModule.IsTargetting && mySkillModule.CanSelectTarget(castedObject, out GameObject target, out Vector3 point))
            {
                mySkillModule.ActivateSelectedSkill();
                mySkillModule.SetUpSkillContext(target, point);
                context.ChangeState(PlayerStateType.SkillChase);
            }
        }

        public void OnRightClick(RaycastHit castedObject)
        {        
            if(mySkillModule.IsTargetting) // 스킬 UI 중 우클릭 시 UI 해제. 만약 이 로직이 모든 State들의 RightClick에서 공통적으로 일어나면 아예 PlayerController에서 처리하기.
            {
                mySkillModule.CancelTargettingMode();
                return;
            }

            context.CancelEnemySelect();

            //인터렉터블 오브젝트 처리부분. 해당 로직들이 자주 쓰이면 PlayerController로 빼는거 고려.
            IInteractable interactable = castedObject.collider.GetComponentInParent<IInteractable>();            
            if (interactable != null && !interactable.IsInUse) 
            {
                myInteractionModule.CurrentInteractTarget = interactable;
                context.ChangeState(PlayerStateType.InteractChasing);
                return;
            }

            switch (castedObject.collider.gameObject.layer) //검사 후순위
            {
                case 6: //Ground Layer
                    context.PlayerMoveWithIndicator(castedObject.point, false);
                    context.ChangeState(PlayerStateType.Normal);
                    break;
                case 10: //Indicator Layer
                    context.PlayerMoveWithIndicator(castedObject.point, true);
                    context.ChangeState(PlayerStateType.Normal);
                    break;
                case 7: //Enemy Layer
                    context.SetTargetEnemy(castedObject.collider.GetComponent<Enemy>());
                    if (!context.TryExecuteAttack())
                    {
                        context.ChangeState(PlayerStateType.Normal);
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnSkillInput(SkillNumber skillNumber)
        {
            if(!mySkillModule.IsTargetting)
            {
                context.MySkillModule.ActivateTargettingMode(skillNumber);
            }
            else
            {
                mySkillModule.CancelTargettingMode();
            }
        }
    }
}
