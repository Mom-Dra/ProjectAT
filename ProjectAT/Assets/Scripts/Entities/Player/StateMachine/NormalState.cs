using UnityEngine;
using Interactable;
using PlayerStateCapabilities;

namespace PlayerStateMachine
{
    public class NormalState : PlayerState, ILeftClickHandler, IRightClickHandler, ISkillInputHandler, IReloadInputHandler
    {
        private PlayerSkillModule mySkillModule;
        private PlayerInteractionModule myInteractionModule;
        private PlayerCombatModule myCombatModule;

        public NormalState(PlayerController playerController) : base(playerController)
        {
            mySkillModule = context.MySkillModule;
            myInteractionModule = context.MyInteractionModule;
            myCombatModule = context.MyCombatModule;
        }

        public override void OnEnter()
        {
            context.CancelEnemySelect();
        }
        
        public override void OnUpdate()
        {
            if (context.SelectedEnemy != null)
            {
                if (!context.TryExecuteAttack())
                {
                    context.AimingEnemy(false);
                    context.ChaseEnemy();
                }
            }
            else
            {
                context.AimingEnemy(false);
            }
        }
        
        public override void OnExit()
        {
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
            if(myInteractionModule.TrySetInteractTarget(castedObject))
            {
                context.ChangeState(PlayerStateType.InteractChasing);
                return;
            }

            switch (castedObject.collider.gameObject.layer) //검사 후순위
            {
                case 6: //Ground Layer
                    context.PlayerMoveWithIndicator(castedObject.point, false);
                    break;
                case 10: //Indicator Layer
                    context.PlayerMoveWithIndicator(castedObject.point, true);
                    break;
                case 7: //Enemy Layer
                    context.SetTargetEnemy(castedObject.collider.GetComponent<Enemy>());
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
            myCombatModule.RequestWeaponReload();
        }
    }
}