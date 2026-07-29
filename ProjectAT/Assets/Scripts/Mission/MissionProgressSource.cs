using UnityEngine;

namespace ProjectAT.Mission
{
    /// <summary>
    /// 미션의 진행상황을 신호로 보낼 수 있는 오브젝트에게 부여되는 컴포넌트. 
    /// </summary>
    public abstract class MissionProgressSource : MonoBehaviour, IMissionProgressSource
    {
        [Header("Mission Progress")]
        [SerializeField] private MissionObjectiveKey objectiveKey;
        [SerializeField, Min(1)] private int progressAmount = 1;
        [SerializeField] private bool reportOnce = true; //여러번 보고할 수 있는지 여부. true면 한 번만 보고함.

        private bool hasReported = false;

        public MissionObjectiveKey ObjectiveKey => objectiveKey;
        public abstract MissionObjectiveType ObjectiveType { get;}

        /// <summary>
        /// 특정 조건에 의해 미션 진행이 될 경우, InGameManager의 EventManager를 경유해 MissionManager에 진행현황을 보고함.
        /// </summary>
        protected bool ReportProgress()
        {
            if(reportOnce && hasReported) { return false; }
            if(objectiveKey == null)
            {
                Debug.LogError($"{name} : MissionObjectiveKey가 지정되지 않음!", this);
                return false;
            }

            InGameManager inGameManager = InGameManager.Instance;
            
            if(inGameManager == null) {
                Debug.LogError($"{name} : InGameManager를 찾을 수 없습니다.", this);
                return false;
            }

            MissionProgressSignal signal = new MissionProgressSignal(objectiveKey, ObjectiveType, Mathf.Max(1, progressAmount), gameObject);
            if (reportOnce)
            {
                hasReported = true;
            }

            inGameManager.EventManager.Publish(EventType.MissionProgressed, signal);
            return true;
        }

        public void ResetReportState()
        {
            hasReported = false;
        }

        // protected virtual void Onvalidate()
        // {
        //     progressAmount = Mathf.Max(1, progressAmount);
        // }
    }
}