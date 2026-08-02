using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectAT.Mission
{
    /// <summary>
    /// MissionManager의 미션 진행 이벤트를 받아서, 미션별로 등록된 Action을 실행하는 핸들러.
    /// </summary>
    public sealed class MissionSequenceActionHandler : MonoBehaviour
    {
        #region MissionActionEntry Class
        [Serializable]
        private sealed class MissionActionEntry
        {
            [SerializeField] private MissionData mission;

            [Header("Mission Activated")]
            [SerializeField] private UnityEvent onActivated = new UnityEvent();

            [Header("Mission Completed")]
            [SerializeField] private bool waitForCompletionSignal;
            [SerializeField] private UnityEvent onCompleted = new UnityEvent();

            public MissionData Mission => mission;
            public bool WaitForCompletionSignal => waitForCompletionSignal;

            public void InvokeActivated()
            {
                onActivated?.Invoke();
            }

            public void InvokeCompleted()
            {
                onCompleted?.Invoke();
            }
        }
        #endregion =========================
        [Header("Mission Actions")]
        [SerializeField]
        private List<MissionActionEntry> missionActions = new List<MissionActionEntry>();

        [Header("Stage Failure")]
        [SerializeField] private bool waitForStageFailedSignal;
        [SerializeField] private UnityEvent onStageFailed = new UnityEvent();
        
        private readonly Dictionary<MissionData, MissionActionEntry> actionEntryByMission = new Dictionary<MissionData, MissionActionEntry>();
        private Action pendingContinuation; // 현재 연출이 끝난 뒤 MissionManager가 실행할 함수 목록
        public bool HasPendingAction => pendingContinuation != null;


        private void Awake()
        {
            BuildActionLookup();
        }

        private void OnDestroy()
        {
            pendingContinuation = null;
        }


        private void BuildActionLookup()
        {
            actionEntryByMission.Clear();

            if (missionActions == null) return;

            foreach (MissionActionEntry entry in missionActions)
            {
                if (entry == null || entry.Mission == null)
                {
                    Debug.LogWarning($"{name} : MissionData가 없는 Action 항목을 건너뜁니다.", this);
                    continue;
                }

                if (actionEntryByMission.ContainsKey(entry.Mission))
                {
                    Debug.LogWarning($"{name} : '{entry.Mission.MissionName}'의 " + "Action 항목이 중복 등록되어 있습니다.", this);
                    continue;
                }

                actionEntryByMission.Add(entry.Mission, entry);
            }
        }

        /// <summary>
        /// MissionManager가 새로운 미션을 활성화 할 때 호출. 활성화 Action은 흐름을 대기시키지 않음.
        /// </summary>
        /// <param name="mission"></param>
        public void ExecuteMissionActivated(MissionData mission)
        {
            if(mission == null) return;
            if(!actionEntryByMission.TryGetValue(mission, out MissionActionEntry entry))
            {
                return;
            }

            entry.InvokeActivated();
        }


        /// <summary>
        /// MissionManager가 현재 미션을 완료했을때 호출함. waitForCompletionSignal 설정에 따라 즉시 or 완료 신호 후 continution을 실행함.
        /// </summary>
        /// <param name="mission"></param>
        /// <param name="continuation"></param>
        public void ExecuteMissionCompleted(MissionData mission, Action continuation)
        {
            if(mission == null) return;
            if(!actionEntryByMission.TryGetValue(mission, out MissionActionEntry entry))
            {
                continuation?.Invoke();
                return;
            }

            ExecuteAction(entry.WaitForCompletionSignal, entry.InvokeCompleted, continuation);
        }

        /// <summary>
        /// MissionManager가 스테이지 실패를 판정했을때 호출
        /// </summary>
        /// <param name="continuation">후속으로 실행할 함수들</param>
        public void ExecuteStageFailed(Action continuation)
        {
            ExecuteAction(waitForStageFailedSignal, InvokeStageFailed, continuation);
        }

        /// <summary>
        /// 컷신, Timeline, 애니메이션 등의 실제 종료 시점에서 호출함. 대ㅔ기중인 MissionManager의 흐름을 재개함.
        /// </summary>
        public void NotifyPendingActionFinished()
        {
            if(pendingContinuation == null)
            {
                Debug.LogWarning($"{name} : 완료를 기다리는 Mission Action이 없습니다.", this);
                return;
            }

            Action continuation = pendingContinuation;
            pendingContinuation = null;

            continuation.Invoke();
        }

        private void ExecuteAction(bool waitForSignal, Action executeAction, Action continuation)
        {
            if(pendingContinuation != null)
            {
                Debug.LogWarning($"{name} : 다른 Mission Action의 완료를 기다리는 중", this);
                return;
            }

            if (waitForSignal)
            {
                pendingContinuation = continuation;
            }
            
            executeAction?.Invoke();

            if (!waitForSignal)
            {
                continuation?.Invoke();
            }
        }

        private void InvokeStageFailed()
        {
            onStageFailed?.Invoke();
        }
    }
}