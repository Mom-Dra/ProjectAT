using PlayerStateCapabilities;
using Unity.Services.Lobbies.Models;
using UnityEngine;


namespace PlayerStateMachine
{
    public class SkillChaseState : PlayerState, ILeftClickHandler, IRightClickHandler, ISkillInputHandler, IInterruptiblePlayerState
    {
    #region  Needed Modules
        private PlayerSkillModule mySkillModule;
        private PlayerCoverModule myCoverModule;

    #endregion


        private SkillContext nowActivatedSkillContext;

        public SkillChaseState(PlayerController context) : base(context) 
        {
            mySkillModule = context.MySkillModule;
            myCoverModule = context.MyCoverModule;
        }

        public void SetSkillContext(SkillContext context)
        {
            nowActivatedSkillContext = context;
        }

        public override void OnEnter()
        {
        }

        public override void OnExit()
        {
            
        }

        public override void OnUpdate()
        {
            if (mySkillModule.CanCastingSkill(nowActivatedSkillContext))
            {
                context.ChangeState(PlayerStateType.SkillCast);
            }
            else
            {
                if (nowActivatedSkillContext.TargetObject != null)
                {
                    if (!nowActivatedSkillContext.TargetObject.activeInHierarchy)
                    {
                        mySkillModule.CancelCurrentSkill();
                        context.ChangeState(PlayerStateType.Normal);
                        return;
                    }
                    context.PlayerMove(nowActivatedSkillContext.TargetObject.transform.position, false);
                }
                else  // 타겟 오브젝트가 없는 스킬인 경우 (지점 지정형 스킬 등)에는 캐릭터가 지정된 지점으로 이동하도록
                {   
                    Debug.Log("ChaseState : 스킬 추적 중. 타겟 위치로 이동합니다.");
                    context.PlayerMove(nowActivatedSkillContext.CastedPosition, false);
                }  
            }
        }

        public void Interrupt()
        {
            if (mySkillModule.IsTargetting)
            {
                mySkillModule.CancelTargettingMode();
            }

            mySkillModule.CancelCurrentSkill();
            nowActivatedSkillContext = null;
        }

        public void OnRightClick(RaycastHit castedObject)
        {        
            if(mySkillModule.IsTargetting) // 스킬 UI 중 우클릭 시 UI 해제. 만약 이 로직이 모든 State들의 RightClick에서 공통적으로 일어나면 아예 PlayerController에서 처리하기.
            {
                mySkillModule.CancelTargettingMode();
                return;
            }

            mySkillModule.CancelCurrentSkill();
            
            switch (castedObject.collider.gameObject.layer)
            {
                case 6: //Ground Layer
                    context.PlayerMoveWithIndicator(castedObject.point, false);
                    context.ChangeState(PlayerStateType.Normal);
                    break;
                case 7: //Enemy Layer
                    context.SetTargetEnemy(castedObject.collider.GetComponent<Enemy>());
                    context.ChangeState(PlayerStateType.Normal);
                    break;
                case 10: //Indicator Layer
                    context.PlayerMoveWithIndicator(castedObject.point, true);
                    context.ChangeState(PlayerStateType.Normal);
                    break;
                // case 11: // CoverPoint Layer
                //     if (castedObject.transform.TryGetComponent(out CoverPoint coverPoint))
                //         myCoverModule.StartMoveToCover(coverPoint);
                //         context.ChangeState(PlayerStateType.Normal);
                //     break;
                // case 13: // Interactable Layer
                //     context.MyInteractionModule.HandleRightClick();
                //     //context.ChangeState(PlayerStateInteractable);
                //     break;
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

        public void OnLeftClick(RaycastHit castedObject)
        {
            if (mySkillModule.IsTargetting && mySkillModule.CanSelectTarget(castedObject, out GameObject target, out Vector3 point))
            {
                mySkillModule.ActivateSelectedSkill();
                mySkillModule.SetUpSkillContext(target, point);
                context.ChangeState(PlayerStateType.SkillChase);
            }
        }
    }
}
