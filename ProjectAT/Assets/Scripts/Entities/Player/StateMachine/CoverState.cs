using UnityEngine;
using Interactable;
using PlayerStateCapabilities;

namespace PlayerStateMachine
{
    public class CoverState : PlayerState, ILeftClickHandler, IRightClickHandler, ISkillInputHandler, IReloadInputHandler
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
            myAnimationModule.WeaponMeshVisible(true);
        }

        public override void OnExit()
        {
            Debug.Log("CoverState 종료: 엄폐를 해제하고 버프를 원상복구합니다.");

            // 1. 애니메이션 전환 해제
            // myAnimationModule.SetBool("IsCovering", false);

            // 2. 스탯 보너스 회수 (어떤 이유로 상태를 나가든 100% 보장됨!)
            // context.MyStatModule.RemoveDefense(CoverDefenseBonus);
            // context.MyStatModule.RemoveAccuracy(CoverAccuracyBonus);

            InteractableObject occupiedCoverPoint = myInteractionModule.CurrentInteractTarget;
            if(occupiedCoverPoint != null && occupiedCoverPoint.CurrentInteractor == context.gameObject)
            {
                myInteractionModule.ClearInteractTarget(true);
            }
            myAnimationModule.SetCrouch(false);
        }

        public override void OnUpdate()
        {
            if (context.SelectedEnemy != null)
            {
                if (!context.TryExecuteAttack())
                {
                    context.AimingEnemy(false); 
                    //context.ChangeState(PlayerStateType.Normal); // 상태를 Normal로 바꾸면, 다음 프레임부터 NormalState가 알아서 ChaseEnemy()를 실행함
                }
                else
                {
                    context.AimingEnemy(true, context.SelectedEnemy.transform); // 3. 공격이 성공적으로 수행됐다면? -> 엄폐 유지한 채로 에임만 적으로 고정!
                }
            }
            else
            {
                context.AimingEnemy(false);
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

            InteractableObject interactable = castedObject.collider.GetComponentInParent<InteractableObject>();            
            if (interactable != null && !interactable.IsInUse) 
            {
                myInteractionModule.SetInteractTarget(interactable);
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

        public void OnReloadInput()
        {
            context.MyCombatModule.RequestWeaponReload();
        }
    }
}
