using PlayerStatusCapabilities;
using UnityEngine;

namespace PlayerStateMachine
{
    public class CarryState : PlayerState, IRightClickHandler, IDropObjectHandler
    {
        private PlayerInteractionModule myInteractionModule;
        private PlayerAnimator myAnimatorModule;

        public CarryState(PlayerController playerController) : base(playerController)
        {
            myInteractionModule = context.MyInteractionModule;
            myAnimatorModule = context.MyAnimModule;

        }

        public override void OnEnter()
        {
            Debug.Log($"Enter CarryingState. 들고 있는 대상: {myInteractionModule.CurrentInteractTarget}");
            
            // TODO: 플레이어의 애니메이터를 '들고 있는 애니메이션(Carrying)'으로 변경
            // context.Animator.SetBool("IsCarrying", true);
        }

        public override void OnExit()
        {
            Debug.Log("Exit CarryingState.");
            
            // TODO: 들고 있는 애니메이션 해제
            // context.Animator.SetBool("IsCarrying", false);

            // 🚨 안전 장치 (예외 처리)
            // 플레이어가 G키를 눌러서 정상적으로 내려놓은 게 아니라, 
            // 적에게 맞아 죽어서 강제로 DeadState로 전이될 때 시체를 떨어뜨리게 합니다.
            // if (myInteractionModule.CurrentInteractTarget != null)
            // {
            //     Debug.LogWarning("CarryingState 강제 종료! 시체를 바닥에 떨어뜨립니다.");
            //     myInteractionModule.CurrentInteractTarget.OnExecute(context); // StopCarrying 호출
            //     myInteractionModule.CurrentInteractTarget = null;
            // }
        }

        public override void OnUpdate()
        {
            // InputManager에서 특정 키 이벤트를 상태 머신으로 넘겨주는 구조라면
            // 이 부분을 별도의 인터페이스(예: IInteractKeyHandler)로 분리하셔도 좋습니다.
            // 여기서는 이해를 돕기 위해 직접 키 입력을 체크하는 형태로 작성했습니다.
            // if (Input.GetKeyDown(KeyCode.G)) // 예: G키를 내려놓기 키로 사용
            // {
            //     ExecuteDrop();
            // }
        }

        public void OnRightClick(RaycastHit castedObject)
        {
            switch (castedObject.collider.gameObject.layer) 
            {
                case 6: // Ground Layer
                    context.PlayerMoveWithIndicator(castedObject.point, false);
                    break;
                case 10: // Indicator Layer
                    context.PlayerMoveWithIndicator(castedObject.point, true);
                    break;
                default:
                    // Enemy Layer나 다른 Interactable 오브젝트는 철저히 무시합니다.
                    Debug.Log("현재 시체를 들고 있어서 다른 행동을 할 수 없습니다.");
                    break;
            }
        }

        public void OnDropObjectInput()
        {
            Debug.Log("Drop Inputed while carrying. Attempting to drop the object...");
            ExecuteDrop();
        }

        private void ExecuteDrop()
        {
            context.ChangeState(PlayerStateType.Interacting);
        }
    }
}