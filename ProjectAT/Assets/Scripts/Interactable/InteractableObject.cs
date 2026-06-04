using PlayerStateMachine;
using UnityEngine;

namespace Interactable
{
    public abstract class InteractableObject : MonoBehaviour, IInteractable, IHoverableFeedback, ITargetableFeedback
    {
        [Header("Interactable Object Settings")]
        [SerializeField] private float interactDuration = 1.0f;
        [SerializeField] private string playerAnimationTrigger = "Interact";
        [SerializeField] protected PlayerStateType nextState = PlayerStateType.Normal;
        [SerializeField] private bool canStopInteract = true;
        
        #region Properties
        public GameObject CurrentInteractor {get; protected set;}
        public float InteractDuration => interactDuration;
        public string PlayerAnimationTrigger => playerAnimationTrigger;
        public bool IsInUse => CurrentInteractor != null;
        public PlayerStateType NextState => nextState;
        public bool CanStopInteract => canStopInteract;
        #endregion
        
        #region Interaction Functions
        public abstract Vector3 GetInteractLookDir(Transform playerTransform);
        public abstract Vector3 GetInteractPosition(Transform playerTransform);
        
        public abstract void OnInteractStart(PlayerController player);
        public abstract void OnExecute(PlayerController player);
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
