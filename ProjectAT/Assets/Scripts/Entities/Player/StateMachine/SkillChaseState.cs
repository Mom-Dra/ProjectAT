using PlayerStateCapabilities;
using Unity.Services.Lobbies.Models;
using UnityEngine;


namespace PlayerStateMachine
{
    public class SkillChaseState : PlayerState, ILeftClickHandler, IRightClickHandler, ISkillInputHandler, IInterruptiblePlayerState
    {
        private PlayerSkillModule mySkillModule;
        private SkillContext nowActivatedSkillContext;
        private float logicCheckingDuration = 0.2f;
        private float currentCheckTime = 0f;


        public SkillChaseState(PlayerController context) : base(context) 
        {
            mySkillModule = context.MySkillModule;
        }

        public void SetSkillContext(SkillContext context)
        {
            nowActivatedSkillContext = context;
        }

        public override void OnEnter()
        {
            currentCheckTime = 0f;
        }

        public override void OnExit()
        {
            
        }

        public override void OnUpdate()
        {
            if(logicCheckingDuration > Time.time - currentCheckTime)
            {
                return;
            } 
            
            currentCheckTime = Time.time;

            if (mySkillModule.CanCastingSkill(nowActivatedSkillContext))
            {
                context.ChangeState(PlayerStateType.SkillCast);
            }
            else
            {
                if (nowActivatedSkillContext.TargetCollider != null)
                {
                    if (!nowActivatedSkillContext.TargetCollider.gameObject.activeInHierarchy)
                    {
                        mySkillModule.CancelCurrentSkill();
                        context.ChangeState(PlayerStateType.Normal);
                        return;
                    }
                    context.PlayerMove(nowActivatedSkillContext.CastedPosition, false);
                }
                else
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
            if (mySkillModule.IsTargetting && mySkillModule.CanSelectTarget(castedObject, out Collider castedCollider, out Vector3 point))
            {
                mySkillModule.ActivateSelectedSkill();
                mySkillModule.SetUpSkillContext(castedCollider, point);
                context.ChangeState(PlayerStateType.SkillChase);
            }
        }
    }
}
