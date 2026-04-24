using System;
using PlayerStateMachine;
using Unity.Networking.Transport;
using UnityEngine;

namespace Interactable
{
    public abstract class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private float interactDuration = 1.0f;
        [SerializeField] private string playerAnimationTrigger = "Interact";
        [SerializeField] private PlayerStateType nextState = PlayerStateType.Normal;

        public GameObject CurrentInteractor {get; private set;}
        public float InteractDuration => interactDuration;
        public string PlayerAnimationTrigger => playerAnimationTrigger;
        public bool IsInUse => CurrentInteractor != null;
        public PlayerStateType NextState => nextState;

        public abstract Vector3 GetInteractLookDir(Transform playerTransform);

        public abstract Vector3 GetInteractPosition(Transform playerTransform);
        
        public abstract void OnExecute(PlayerController player);

        public bool TryLock(PlayerController interactor)
        {
            if(IsInUse && CurrentInteractor != interactor) return false;
            
            CurrentInteractor = interactor.gameObject;
            return true;
        }

        public void UnLock()
        {
            CurrentInteractor = null;
        }
    }
}
