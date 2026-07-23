using System;
using PlayerStateMachine;
using EPOOutline;
using UnityEngine;
using UnityEngine.AI;

namespace Interactable
{
    public abstract class InteractableObject : MonoBehaviour, IInteractable, IHoverableFeedback, ITargetableFeedback
    {
        [Header("[Interactable Object]")]
        [Header("References")]
        [SerializeField] protected Outlinable outlinable;

        [Header("Interactable Object Settings")]
        [SerializeField] private float interactDuration = 1.0f;
        [SerializeField] private string playerAnimationTrigger = "Interact";
        [SerializeField] protected PlayerStateType nextState = PlayerStateType.Normal;
        [SerializeField] private bool canStopInteract = true;

        public event Action<PlayerController> InteractionCompleted;
        
        #region Properties
        public GameObject CurrentInteractor {get; protected set;}
        public float InteractDuration => interactDuration;
        public string PlayerAnimationTrigger => playerAnimationTrigger;
        public bool IsInUse => CurrentInteractor != null;
        public PlayerStateType NextState => nextState;
        public bool CanStopInteract => canStopInteract;
        #endregion

        protected virtual void Awake()
        {
            CurrentInteractor = null;
            outlinable = GetComponent<Outlinable>();
        }
        
        #region Interaction Functions
        public virtual bool TryGetInteractLocation(Transform playerTransform, out Vector3 sampledPosition, out Vector3 sampledLookDir, NavMeshAgent agent)
        {
            sampledPosition = Vector3.zero;
            sampledLookDir = Vector3.zero;

            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                return false;
            }

            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, agent.areaMask))
            {
                return false;
            }

            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            sampledPosition = hit.position;
            sampledLookDir = GetInteractLookDir(sampledPosition);
            return true;
        }

        protected virtual Vector3 GetInteractLookDir(Vector3 SampledPosition)
        {
            Vector3 dir = transform.position - SampledPosition; //플레이어가 오브젝트를 바라보는 방향
            dir.y = 0; 
            return dir.normalized;
        }

        public abstract void OnInteractStart(PlayerController player);
        public void OnExecute(PlayerController player) // 상호작용 동작을 실행시키는 래퍼 함수. TryExecuteInteraction을 호출하고 성공하면 InteractionCompleted 이벤트를 발생시킨다.
        {
            if (TryExecuteInteraction(player))
            {
                InteractionCompleted?.Invoke(player);
            }
        }

        /// <summary>
        /// 실제 상호작용 동작을 실행하는 클래스. 성공한 경우 true를 반환한다.
        /// </summary>
        protected abstract bool TryExecuteInteraction(PlayerController player);
        
        public virtual void OnInteractEnd(PlayerController player) {}
        
        public virtual bool TryLock(PlayerController interactor)
        {
            if(IsInUse && CurrentInteractor != interactor) return false;
            
            CurrentInteractor = interactor.gameObject;
            return true;
        }

        public virtual void UnLock()
        {
            CurrentInteractor = null;
        }

        #endregion

        #region InteractionFeedback Functions
        protected bool isSelected = false;
        public abstract void OnHoverEnter();
        public abstract void OnHoverExit();
        public abstract void OnTargeted();
        public abstract void OnUntargeted();
        #endregion
    }
}
