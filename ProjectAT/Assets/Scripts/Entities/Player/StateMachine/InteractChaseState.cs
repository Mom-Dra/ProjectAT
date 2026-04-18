using UnityEngine;
using PlayerStatusCapabilities;



namespace PlayerStateMachine
{
    public class InteractChaseState : PlayerState, ILeftClickHandler, IRightClickHandler, ISkillInputHandler
    {
        private const float StopDistanceThreshold = 1.0f; // 상호작용 위치에 도달했다고 판단하는 거리 임계값.
        private PlayerInteractionModule myInteractionModule;

        public InteractChaseState(PlayerController context) : base(context)
        {
            myInteractionModule = context.MyInteractionModule;
        }

        public override void OnEnter()
        {
            
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

            // // 안전 장치 1: 추적 중에 대상이 파괴되었거나 null이 된 경우
            // if (target == null)
            // {
            //     Debug.LogWarning("InteractChasingState : 타겟이 사라져 Normal 상태로 복귀합니다.");
            //     context.ChangeState(PlayerStateType.Normal);
            //     return;
            // }

            // // 1. 타겟에게 "내가 어디로 가야 하고, 어디를 봐야 해?" 라고 묻습니다.
            // //Vector3 requiredPos = target.GetInteractPosition(context.transform);
            // //Vector3 requiredLook = target.GetInteractLookDir(context.transform);

            // // 안전 장치 2: 높이(Y축) 차이 때문에 도착 판정이 안 나는 것을 방지하기 위해 XZ 평면 거리만 잽니다.
            // Vector3 currentPosXZ = new Vector3(context.transform.position.x, 0, context.transform.position.z);
            // Vector3 requiredPosXZ = new Vector3(requiredPos.x, 0, requiredPos.z);
            
            // float distance = Vector3.Distance(currentPosXZ, requiredPosXZ);

            // // 2. 요구 위치에 도달했는지 확인
            // if (distance <= StopDistanceThreshold)
            // {
            //     // 3. 도착! 애니메이션이 틀어지지 않도록 위치와 회전을 완벽하게 강제 보정(Snapping)합니다.
            //     // (Y축은 플레이어의 현재 바닥 높이를 유지하여 땅에 파묻히는 것을 방지)
            //     context.transform.position = new Vector3(requiredPos.x, context.transform.position.y, requiredPos.z);
                
            //     if (requiredLook != Vector3.zero)
            //     {
            //         context.transform.forward = requiredLook;
            //     }

            //     // 4. 추적을 끝내고 본격적인 상호작용 상태로 넘어갑니다!
            //     context.ChangeState(PlayerStateType.Interacting);
            // }
            // else
            // {
            //     // 아직 멀었다면 요구 위치로 계속 이동 명령을 내립니다.
            //     context.PlayerMove(requiredPos);
            // }
        }

        public void OnLeftClick(RaycastHit castedObject){}

        public void OnRightClick(RaycastHit castedObject){}

        public void OnSkillInput(SkillNumber skillNumber){}
    }
}