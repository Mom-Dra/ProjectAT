using UnityEngine;
using Interactable;
using PlayerStateCapabilities;


namespace PlayerStateMachine
{
    public class InteractChaseState : PlayerState, ILeftClickHandler, IRightClickHandler, ISkillInputHandler
    {
        private const float StopDistanceThreshold = 0.1f; // 상호작용 위치에 도달했다고 판단하는 거리 임계값.
        private PlayerInteractionModule myInteractionModule;
        private PlayerMovementModule myMovementModule;
        private PlayerSkillModule mySkillModule;

        public InteractChaseState(PlayerController context) : base(context)
        {
            myInteractionModule = context.MyInteractionModule;
            myMovementModule = context.MyMovementModule;
            mySkillModule = context.MySkillModule;
        }

        public override void OnEnter()
        {
            //myInteractionModule.CurrentInteractTarget.OnSelected();
        }

        public override void OnExit()
        {
            // 상태를 빠져나갈 때 뒷정리 (이동 멈춤 명령, 이동 애니메이션 끄기 등)
            // context.StopMove(); 
            // context.MyAnimationModule.SetBool("IsMoving", false);
            
        }

        public override void OnUpdate()
        {
            InteractableObject target = myInteractionModule.CurrentInteractTarget;

            // 안전 장치 1: 추적 중에 대상이 파괴되었거나 null이 된 경우
            if (myInteractionModule.CheckCurrentInteractObjectAvailable())
            {
                context.PlayerMove(context.transform.position, false); // 이동 멈춤
                CancelInteractChasing(PlayerStateType.Normal);
                return;
            }

            if (!myInteractionModule.CheckCurrentInteractTargetReachable())
            {
                context.PlayerMove(context.transform.position, false); // 이동 멈춤
                CancelInteractChasing(PlayerStateType.Normal);
            }

            Vector3 requiredPos = myInteractionModule.CurrentInteractPosition;
            Vector3 currentPosXZ = new Vector3(context.transform.position.x, 0, context.transform.position.z);
            Vector3 requiredPosXZ = new Vector3(requiredPos.x, 0, requiredPos.z);
            
            float sqrtDistance = Vector3.SqrMagnitude(currentPosXZ - requiredPosXZ);

            if (sqrtDistance <= StopDistanceThreshold * StopDistanceThreshold)
            {
                context.transform.position = new Vector3(requiredPos.x, context.transform.position.y, requiredPos.z);

                if (target.TryLock(context))
                {
                    Vector3 requiredLook = myInteractionModule.CurrentInteractLookDir;
                    context.transform.forward = requiredLook == Vector3.zero ? context.transform.forward : requiredLook;
                    context.ChangeState(PlayerStateType.Interacting);
                }
                else
                {
                    context.PlayerMove(context.transform.position, false);
                    CancelInteractChasing(PlayerStateType.Normal);
                }
            }
            else
            {
                context.PlayerMove(requiredPos, false);
            }
        }

        public void OnLeftClick(RaycastHit castedObject)
        {
            if (mySkillModule.IsTargetting && mySkillModule.CanSelectTarget(castedObject, out GameObject target, out Vector3 point))
            {
                mySkillModule.ActivateSelectedSkill();
                mySkillModule.SetUpSkillContext(target, point);
                CancelInteractChasing(PlayerStateType.SkillChase);
            }
        }

        public void OnRightClick(RaycastHit castedObject)
        {        
            if(mySkillModule.IsTargetting) // 스킬 UI 중 우클릭 시 UI 해제. 만약 이 로직이 모든 State들의 RightClick에서 공통적으로 일어나면 아예 PlayerController에서 처리하기.
            {
                mySkillModule.CancelTargettingMode();
                return;
            }
            
            if(myInteractionModule.TrySetInteractTarget(castedObject))
            {
                context.ChangeState(PlayerStateType.InteractChasing);
                return;
            }

            switch (castedObject.collider.gameObject.layer) //검사 후순위
            {
                case 6: //Ground Layer
                    context.PlayerMove(castedObject.point, false);
                    CancelInteractChasing(PlayerStateType.Normal);
                    break;
                case 10: //Indicator Layer
                    context.PlayerMoveWithIndicator(castedObject.point, true);
                    CancelInteractChasing(PlayerStateType.Normal);
                    break;
                case 7: //Enemy Layer
                    context.SetTargetEnemy(castedObject.collider.GetComponent<Enemy>());
                    CancelInteractChasing(PlayerStateType.Normal);
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

        private void CancelInteractChasing(PlayerStateType nextState)
        {
            if(myInteractionModule.CurrentInteractTarget == null) return;
            
            myInteractionModule.ClearInteractTarget();
            context.ChangeState(nextState);
        }
    }
}