using UnityEngine;
using PlayerStatusCapabilities;


namespace PlayerStateMachine
{
    public class InteractingState : PlayerState, IRightClickHandler
    {
        private PlayerInteractionModule myInteractionModule;
        private float currentInteractTime;

        public InteractingState(PlayerController context) : base(context) 
        { 
            myInteractionModule = context.MyInteractionModule;
        }

        public override void OnEnter()
        {
            Debug.Log($"Enter InteractingState. : {myInteractionModule.CurrentInteractTarget}");
            myInteractionModule.CurrentInteractTarget.OnTargetSelected();
            currentInteractTime = 0f;
        }

        public override void OnExit()
        {
            Debug.Log("Exit InteractingState.");
            currentInteractTime = 0f;
            myInteractionModule.CurrentInteractTarget.OnTargetDeselected();

            if(myInteractionModule.CurrentInteractTarget.NextState == PlayerStateType.Normal)
            {
                myInteractionModule.CurrentInteractTarget.UnLock();
                myInteractionModule.CurrentInteractTarget = null; //상호작용이 끝나면 타겟 초기화.
            }
        }

        public override void OnUpdate()
        {
            currentInteractTime += Time.deltaTime;

            if (currentInteractTime >= myInteractionModule.CurrentInteractTarget.InteractDuration)
            {
                myInteractionModule.CurrentInteractTarget.OnExecute(context);
                context.ChangeState(myInteractionModule.CurrentInteractTarget.NextState);
            }
        }

        public void OnRightClick(RaycastHit castedObject)
        {        
            
            if (castedObject.collider.TryGetComponent(out IInteractable interactable) && interactable != myInteractionModule.CurrentInteractTarget) //인터렉터블 오브젝트 처리.
            {
                myInteractionModule.CurrentInteractTarget = interactable;
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
    }
}
