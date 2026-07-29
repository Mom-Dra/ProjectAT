
using UnityEngine;
using UnityEngine.Events;

namespace Interactable
{
    /// <summary>
    /// 소스 오브젝트의 상호작용이 완료됐을 경우 외부의 다른 오브젝트에 신호를 보내는 클래스.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InteractableObject))]
    public sealed class InteractSignalSender : MonoBehaviour
    {
        [Header("Source Object")]
        [SerializeField] private InteractableObject interactableObject;

        [Header("Interact Actions")]
        [SerializeField] private UnityEvent onInteractCompleted;
        //[SerializeField] private UnityEvent onInteractStarted;
        
        private void Reset()
        {
            interactableObject = GetComponent<InteractableObject>();
        }

        private void Awake()
        {
            if(interactableObject == null) interactableObject = GetComponent<InteractableObject>();
            if(interactableObject == null)
            {
                Debug.LogError($"{name} : InteractableObject가 필요합니다.", this);
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if(interactableObject != null)
            {
                interactableObject.OnInteractCompleted += HandleInteractionCompleted;
            }
        }

        private void OnDisable()
        {
            if(interactableObject != null)
            {
                interactableObject.OnInteractCompleted -= HandleInteractionCompleted;
            }
        }

        private void HandleInteractionCompleted()
        {
            onInteractCompleted?.Invoke();
        }
    }
}