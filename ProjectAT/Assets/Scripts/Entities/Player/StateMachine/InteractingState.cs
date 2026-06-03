using UnityEngine;
using PlayerStateCapabilities;
using Interactable;

namespace PlayerStateMachine
{
    public class InteractingState : PlayerState, IRightClickHandler
    {
        private PlayerInteractionModule myInteractionModule;
        private PlayerAnimator myAnimationModule;
        private PlayerStateType nextStateCash = PlayerStateType.Normal;
        private float currentInteractTime = 0.1f;
        private bool isInteractingComplete = false;

        public InteractingState(PlayerController context) : base(context) 
        { 
            myInteractionModule = context.MyInteractionModule;
            myAnimationModule = context.MyAnimModule;
        }

        public override void OnEnter()
        {
            Debug.Log($"Enter InteractingState. : {myInteractionModule.CurrentInteractTarget}");
            myInteractionModule.CurrentInteractTarget.OnSelected();
            currentInteractTime = 0f;
            isInteractingComplete = false;

            myAnimationModule.WeaponMeshVisible(false);
            myInteractionModule.CurrentInteractTarget.OnInteractStart(context);
        }

        public override void OnExit()
        {
            Debug.Log("Exit InteractingState.");

            InteractableObject target = myInteractionModule.CurrentInteractTarget;

            //본인 후처리
            if(!isInteractingComplete || nextStateCash == PlayerStateType.Normal)
            {
                target.UnLock();
                myInteractionModule.CurrentInteractTarget = null;
                myAnimationModule.WeaponMeshVisible(true);
            }
            
            //타겟 후처리
            target.OnDeselected();
            target.OnInteractEnd(context);
        }

        public override void OnUpdate()
        {
            currentInteractTime += Time.deltaTime;

            if (currentInteractTime >= myInteractionModule.CurrentInteractTarget.InteractDuration)
            {
                isInteractingComplete = true;
                
                nextStateCash = myInteractionModule.CurrentInteractTarget.NextState;
                myInteractionModule.CurrentInteractTarget.OnExecute(context);

                context.ChangeState(nextStateCash);
            }
        }

        public void OnRightClick(RaycastHit castedObject)
        {        
            if(!myInteractionModule.CurrentInteractTarget.CanStopInteract) return;
            
            if (castedObject.collider.TryGetComponent(out InteractableObject interactable) && interactable != myInteractionModule.CurrentInteractTarget) //인터렉터블 오브젝트 처리.
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
                case 7: //Enemy Layer
                    context.SetTargetEnemy(castedObject.collider.GetComponent<Enemy>());
                    context.ChangeState(PlayerStateType.Normal);
                    break;
                default:
                    break;
            }
        }
    }
}
