using PlayerStateCapabilities;
using UnityEngine;

namespace PlayerStateMachine
{
    public class CarryState : PlayerState, IRightClickHandler, IDropObjectHandler, IInterruptiblePlayerState
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
        }

        public override void OnUpdate()
        {
        }

        public void Interrupt()
        {
            myInteractionModule.DropHoldedObject();
            myAnimatorModule.WeaponMeshVisible(true);
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
