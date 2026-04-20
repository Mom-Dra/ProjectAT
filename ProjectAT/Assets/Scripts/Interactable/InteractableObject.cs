using System;
using UnityEngine;

namespace Interactable
{
    public abstract class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private float interactDuration = 1.0f;
        [SerializeField] private string playerAnimationTrigger = "Interact";
        [SerializeField] private bool isCarryable = false;

        public float InteractDuration => interactDuration;

        public string PlayerAnimationTrigger => playerAnimationTrigger;

        public bool IsCarryable => isCarryable;

        public abstract Vector3 GetInteractLookDir(Transform playerTransform);

        public abstract Vector3 GetInteractPosition(Transform playerTransform);
        
        public abstract void OnExecute(PlayerController player);
    }
}
