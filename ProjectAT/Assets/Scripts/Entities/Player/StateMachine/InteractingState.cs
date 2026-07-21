using UnityEngine;
using PlayerStateCapabilities;
using Interactable;

namespace PlayerStateMachine
{
    public class InteractingState : PlayerState, IRightClickHandler, IInterruptiblePlayerState
    {
        private PlayerInteractionModule myInteractionModule;
        private PlayerAnimator myAnimationModule;
        private PlayerStateType nextStateCash = PlayerStateType.Normal;
        private float currentInteractTime = 0.1f;

        public InteractingState(PlayerController context) : base(context) 
        { 
            myInteractionModule = context.MyInteractionModule;
            myAnimationModule = context.MyAnimModule;
        }

        public override void OnEnter()
        {
            Debug.Log($"Enter InteractingState. : {myInteractionModule.CurrentInteractTarget}");
            nextStateCash = PlayerStateType.Normal;
            currentInteractTime = 0f;

            context.PlayerMove(context.transform.position, false);
            myAnimationModule.WeaponMeshVisible(false);
            myInteractionModule.CurrentInteractTarget.OnInteractStart(context);
        }

        public override void OnExit()
        {
            Debug.Log("Exit InteractingState.");

            if (myInteractionModule.CurrentInteractTarget == null)
            {
                myAnimationModule.WeaponMeshVisible(true);
                return;
            }

            myInteractionModule.CurrentInteractTarget.OnInteractEnd(context);
            if(nextStateCash == PlayerStateType.Carry)
            {
                myInteractionModule.UnSelectInteractTarget();
            }
            else
            {
                myInteractionModule.ClearInteractTarget(true);
                myAnimationModule.WeaponMeshVisible(true);
            }
        }

        public override void OnUpdate()
        {
            if(!myInteractionModule.CheckCurrentInteractObjectAvailable())
            {
                context.ChangeState(PlayerStateType.Normal);
                return;
            }

            currentInteractTime += Time.deltaTime;

            if (currentInteractTime >= myInteractionModule.CurrentInteractTarget.InteractDuration)
            {
                nextStateCash = myInteractionModule.CurrentInteractTarget.NextState;
                myInteractionModule.CurrentInteractTarget.OnExecute(context);

                context.ChangeState(nextStateCash);
            }
        }

        public void Interrupt()
        {
            nextStateCash = PlayerStateType.Normal;
        }

        public void OnRightClick(RaycastHit castedObject)
        {        
            if(!myInteractionModule.CurrentInteractTarget.CanStopInteract) return;
            
            if(myInteractionModule.TrySetInteractTarget(castedObject))
            {
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
