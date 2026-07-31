using UnityEngine;

namespace ProjectAT.Mission
{
    public sealed class MissionActionCompletionAsyncReporter : MonoBehaviour
    {
        [SerializeField]
        private MissionSequenceActionHandler actionHandler;

        private void Awake()
        {
            if (actionHandler == null)
            {
                actionHandler = FindFirstObjectByType<MissionSequenceActionHandler>();
            }
        }

        public void ReportActionFinished()
        {
            if (actionHandler == null)
            {
                Debug.LogError($"{name} : MissionSequenceActionHandler를 찾을 수 없습니다.", this);

                return;
            }

            actionHandler.NotifyPendingActionFinished();
        }
    }
}