using Interactable;
using UnityEngine;

namespace ProjectAT.Mission
{
    /// <summary>
    /// 상호작용형 인터렉터블 오브젝트에 부착할 미션진행 소스입니다. 상호작용이 완료되면 미션 진행 상황을 InagmeManager에 보고합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InterationMissionProgressSource : MissionProgressSource
    {
        [SerializeField] private InteractableObject sourceObject;
        public override MissionObjectiveType ObjectiveType => MissionObjectiveType.Interact;

        private void Reset()
        {
            sourceObject = GetComponent<InteractableObject>();
        }

        private void Awake()
        {
            if (sourceObject == null)
            {
                sourceObject = GetComponent<InteractableObject>();
            }
        }

        private void OnEnable()
        {
            if(sourceObject == null)
            {
                Debug.Log($"{name} : InteractableObject가 필요합니다.", this);
                return;
            }

            sourceObject.OnInteractCompleted += HandleInteractionCompleted;
        }

        private void OnDisable()
        {
            if(sourceObject != null)
            {
                sourceObject.OnInteractCompleted -= HandleInteractionCompleted;
            }
        }

        private void HandleInteractionCompleted()
        {
            ReportProgress();
        }
    }
}
