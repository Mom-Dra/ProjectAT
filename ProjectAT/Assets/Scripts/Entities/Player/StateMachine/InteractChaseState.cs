using UnityEngine;
using PlayerStatusCapabilities;
using Unity.Services.Lobbies.Models;
using UnityEngine.AI;
using Interactable;



namespace PlayerStateMachine
{
    public class InteractChaseState : PlayerState, ILeftClickHandler, IRightClickHandler, ISkillInputHandler
    {
        private const float StopDistanceThreshold = 0.1f; // 상호작용 위치에 도달했다고 판단하는 거리 임계값.
        private PlayerInteractionModule myInteractionModule;
        private PlayerSkillModule mySkillModule;

        public InteractChaseState(PlayerController context) : base(context)
        {
            myInteractionModule = context.MyInteractionModule;
            mySkillModule = context.MySkillModule;
        }

        public override void OnEnter()
        {
            myInteractionModule.CurrentInteractTarget.OnTargetSelected();
        }

        public override void OnExit()
        {
            // 상태를 빠져나갈 때 뒷정리 (이동 멈춤 명령, 이동 애니메이션 끄기 등)
            // context.StopMove(); 
            // context.MyAnimationModule.SetBool("IsMoving", false);
            
        }

        public override void OnUpdate()
        {
            IInteractable target = myInteractionModule.CurrentInteractTarget;

            // 안전 장치 1: 추적 중에 대상이 파괴되었거나 null이 된 경우
            if (target == null || (target.IsInUse && target.CurrentInteractor != context.gameObject))
            {
                Debug.Log("다른 플레이어가 먼저 상호작용을 시작했습니다. 추적을 취소합니다.");
                context.PlayerMove(context.transform.position, false); // 이동 멈춤
                CancelInteractChasing(PlayerStateType.Normal);
                return;
            }

            // 1. 타겟에게 "내가 어디로 가야 하고, 어디를 봐야 해?" 라고 묻습니다.
            Vector3 requiredPos = target.GetInteractPosition(context.transform);

            // 안전 장치 2: 높이(Y축) 차이 때문에 도착 판정이 안 나는 것을 방지하기 위해 XZ 평면 거리만 잽니다.
            Vector3 currentPosXZ = new Vector3(context.transform.position.x, 0, context.transform.position.z);
            Vector3 requiredPosXZ = new Vector3(requiredPos.x, 0, requiredPos.z);
            
            float sqrtDistance = Vector3.SqrMagnitude(currentPosXZ - requiredPosXZ);

            // 2. 요구 위치에 도달했는지 확인
            if (sqrtDistance <= StopDistanceThreshold * StopDistanceThreshold)
            {
                // 3. 도착! 애니메이션이 틀어지지 않도록 위치와 회전을 완벽하게 강제 보정(Snapping)합니다.(Y축은 플레이어의 현재 바닥 높이를 유지하여 땅에 파묻히는 것을 방지)
                context.transform.position = new Vector3(requiredPos.x, context.transform.position.y, requiredPos.z);

                if (target.TryLock(context))
                {
                    Vector3 requiredLook = target.GetInteractLookDir(context.transform);
                    context.transform.forward = requiredLook == Vector3.zero ? context.transform.forward : requiredLook; // 요구하는 시선이 없으면 현재 방향 유지
                    context.ChangeState(PlayerStateType.Interacting);
                }
                else
                {
                    Debug.Log("도착했지만 다른 플레이어가 먼저 상호작용을 시작했습니다. 추적을 취소합니다.");
                    context.PlayerMove(context.transform.position, false); // 이동 멈춤
                    CancelInteractChasing(PlayerStateType.Normal);
                }
                
            }
            else
            {
                // 아직 멀었다면 요구 위치로 계속 이동 명령을 내립니다.
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
            
            if (castedObject.collider.TryGetComponent(out IInteractable interactable)) //인터렉터블 오브젝트 처리.
            {
                myInteractionModule.CurrentInteractTarget = interactable;
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
            //context.PlayerMove(context.transform.position, false); // 이동 멈춤
            myInteractionModule.CurrentInteractTarget.OnTargetDeselected();
            myInteractionModule.CurrentInteractTarget = null;
            context.ChangeState(nextState);
        }
    }
}