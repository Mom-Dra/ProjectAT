using UnityEngine;
using PlayerStatusCapabilities;
using Interactable;
using UnityEngine.AI;
using Unity.Services.Lobbies.Models;


namespace PlayerStateMachine
{
    public class InteractingState : PlayerState, IRightClickHandler
    {
        private PlayerInteractionModule myInteractionModule;
        private float currentInteractTime;
        private bool isInteractingComplete;

        public InteractingState(PlayerController context) : base(context) 
        { 
            myInteractionModule = context.MyInteractionModule;
        }

        public override void OnEnter()
        {
            Debug.Log($"Enter InteractingState. : {myInteractionModule.CurrentInteractTarget}");
            myInteractionModule.CurrentInteractTarget.OnTargetSelected();
            currentInteractTime = 0f;
            isInteractingComplete = false;

            myInteractionModule.CurrentInteractTarget.OnInteractStart(context);
        }

        public override void OnExit()
        {
            Debug.Log("Exit InteractingState.");
            currentInteractTime = 0f;
            myInteractionModule.CurrentInteractTarget.OnTargetDeselected();

            //본인 후처리
            if(!isInteractingComplete || myInteractionModule.CurrentInteractTarget.NextState == PlayerStateType.Normal)
            {
                Debug.Log("상호작용이 완료되지 않았거나, 다음 상태가 Normal입니다. 타겟과의 락을 해제합니다.");
                myInteractionModule.CurrentInteractTarget.UnLock();
                myInteractionModule.CurrentInteractTarget = null; //상호작용이 끝나면 타겟 초기화.
            }
            
            myInteractionModule.CurrentInteractTarget?.OnInteractEnd(context);
        }

        public override void OnUpdate()
        {
            currentInteractTime += Time.deltaTime;

            if (currentInteractTime >= myInteractionModule.CurrentInteractTarget.InteractDuration)
            {
                isInteractingComplete = true;
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
