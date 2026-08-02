using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectAT.Mission;
using UnityEngine.Serialization;

namespace ProjectAT.Mission
{
    public enum MissionFlowState
    {
        NotStarted,
        Active,
        Transitioning,
        StageFailing,
        Finished
    }
    
    public class MissionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MissionSequenceActionHandler actionHandler;
        private EventManager eventManager;


        [FormerlySerializedAs("activeMissions")]
        [SerializeField] private List<MissionData> missionSequence = new List<MissionData>();

        private readonly List<MissionRuntimeState> missionStates = new List<MissionRuntimeState>();
        private readonly HashSet<MissionData> completedMissions = new HashSet<MissionData>();

        private int currentMissionIndex = 0;
        private MissionFlowState flowState = MissionFlowState.NotStarted;
        private readonly bool[] isDeadPlayers = new bool[(int)PlayerId.Last];

        #region Events
        public event Action<MissionData, MissionObjectiveDefinition, int> ObjectiveProgressed;
        public event Action<MissionData> MissionCompleted;
        public event Action<MissionData> MissionActivated;
        public event Action AllMissionsCompleted;
        #endregion =========================
        #region  properties
        public MissionData CurrentMission
        {
            get
            {
                MissionRuntimeState state = GetCurrentMissionState();
                return state != null ? state.Mission : null;
            }
        }
        public int CurrentMissionIndex => currentMissionIndex;
        public bool IsMissionActiave => flowState == MissionFlowState.Active;
        public bool IsTransitioning => flowState == MissionFlowState.Transitioning;
        public bool AreAllMissionsCompleted => (flowState == MissionFlowState.Finished) && missionStates.Count > 0 && (completedMissions.Count == missionStates.Count);
        #endregion ================

        private void Awake()
        {
            actionHandler = GetComponentInChildren<MissionSequenceActionHandler>();
            BuildRuntimeStates();
        }

        private void Start()
        {
            if(missionStates.Count == 0)
            {
                Debug.LogWarning("등록된 유효 미션 없음.", this);
                return;
            }
            ActivateMission(0);
        }

        private void OnEnable()
        {
            InGameManager ingameManger = InGameManager.Instance;
            if(ingameManger == null)
            {
                Debug.LogError($"{name} : InGameManager를 찾을 수 없습니다.", this);
                return;
            }
            eventManager = ingameManger.EventManager;
            if(eventManager == null)
            {
                Debug.LogError($"{name} : EventManager를 찾을 수 없습니다.", this);
                return;
            }

            eventManager.Subscribe<MissionProgressSignal>(EventType.MissionProgressed, HandleMissionProgressed);
            eventManager.Subscribe<PlayerId>(EventType.PlayerDied, HandlePlayerDied);
        }

        private void OnDisable()
        {
            if(eventManager == null) return;

            eventManager.UnSubscribe<MissionProgressSignal>(EventType.MissionProgressed, HandleMissionProgressed);
            eventManager.UnSubscribe<PlayerId>(EventType.PlayerDied, HandlePlayerDied);
        }

        private void BuildRuntimeStates()
        {
            missionStates.Clear();
            completedMissions.Clear();
            Array.Clear(isDeadPlayers, 0, isDeadPlayers.Length);
            currentMissionIndex = -1;
            flowState = MissionFlowState.NotStarted;

            if(missionSequence == null) return;

            HashSet<MissionData> registedMissions = new HashSet<MissionData>();

            foreach(MissionData mission in missionSequence)
            {
                if(mission == null) { Debug.LogWarning($"{name} : null MissionData는 건너뜁니다.", this); continue;}
                if(!registedMissions.Add(mission)) { Debug.LogWarning($"{name} : '{mission.name}'이(가) 중복등록되어 건너뜁니다.", this); continue;}
                if(mission.Objectives == null || mission.Objectives.Count == 0) { Debug.LogWarning($"{name} : '{mission.name}'미션에 목표가 없어 건너뜁니다.", this); continue;}

                missionStates.Add(new MissionRuntimeState(mission));
            }
        }

        private void ActivateMission(int index)
        {
            if(index < 0 || index >= missionStates.Count) return;

            currentMissionIndex = index;
            flowState = MissionFlowState.Active;

            MissionData mission = CurrentMission;

            Debug.Log($"Mission '{mission.MissionName}' activated!");
            MissionActivated?.Invoke(mission);  //선택적으로 구독한 UI 같은 시스템 등등에 미션 현재 미션을 알림
            actionHandler?.ExecuteMissionActivated(mission); //미션 활성화에 필요한 필수적인 씬 action을 실행
        }

        private void HandleMissionProgressed(MissionProgressSignal signal)
        {
            if((flowState != MissionFlowState.Active)
                ||(signal.ObjectiveKey == null || signal.Amount <= 0)
                ) return;

            MissionRuntimeState state = GetCurrentMissionState();
            if(state == null) return;

            MissionData mission = state.Mission;
            if(completedMissions.Contains(mission)) return;
            if(!state.TryApply(signal, out MissionObjectiveDefinition objective, out int currentProgress)) return;

            Debug.Log($"Mission '{mission.MissionName}' progress: {objective.ObjectiveKey.DisplayName} {currentProgress}/{objective.RequiredAmount}");
            
            ObjectiveProgressed?.Invoke(mission, objective, currentProgress);
            if(state.IsComplete) CompleteMission(mission);
        }

        private void CompleteMission(MissionData mission)
        {
            if((flowState != MissionFlowState.Active) ||(mission != CurrentMission)) return; 
            
            if(!completedMissions.Add(mission)) return;
            flowState = MissionFlowState.Transitioning;
            Debug.Log($"Mission '{mission.MissionName}' completed!");
            
            MissionCompleted?.Invoke(mission);
            if(actionHandler == null)
            {
                ContinueAfterMissionCompleted();
                return;
            }

            actionHandler.ExecuteMissionCompleted(mission, ContinueAfterMissionCompleted);
        }

        private void ContinueAfterMissionCompleted()
        {
            if(flowState != MissionFlowState.Transitioning) return;
            int nextMissionIndex = currentMissionIndex + 1;

            if(nextMissionIndex < missionStates.Count)
            {
                ActivateMission(nextMissionIndex);
                return;
            }

            FinishStageClear();  // 마지막 미션의 완료 Action까지 끝난 다음 스테이지 클리어
        }

        private MissionRuntimeState GetCurrentMissionState()
        {
            if(currentMissionIndex < 0 || currentMissionIndex >= missionStates.Count) return null;
            return missionStates[currentMissionIndex];
        }

        private void HandlePlayerDied(PlayerId playerId)
        {
            // 미션 완료 연출 중이거나 이미 실패한 경우에는 무시
            if (flowState != MissionFlowState.Active) return;

            int index = (int)playerId;

            if (index < 0 || index >= isDeadPlayers.Length) return;

            isDeadPlayers[index] = true;

            foreach (bool isDead in isDeadPlayers)
            {
                if (!isDead)
                    return;
            }

            BeginStageFail();
        }

        private void BeginStageFail()
        {
            if(flowState != MissionFlowState.Active) return;
            flowState = MissionFlowState.StageFailing;
            Debug.Log("stage failed!");

            if(actionHandler == null)
            {
                FinishStageFail();
                return;
            }

            actionHandler.ExecuteStageFailed(FinishStageFail);
        }

        private void FinishStageFail()
        {
            if(flowState != MissionFlowState.StageFailing) return;
            flowState = MissionFlowState.Finished;
            currentMissionIndex = -1;

            Managers.Instance.SceneManager.LoadSceneAsync(SceneType.End); // TODO : MISSION FAIL UI 활성화
        }

        private void FinishStageClear()
        {
            if (flowState != MissionFlowState.Transitioning)
                return;

            flowState = MissionFlowState.Finished;
            currentMissionIndex = -1;

            Debug.Log("Stage cleared!");

            AllMissionsCompleted?.Invoke(); // 모든 미션 완료 이벤트 발생
        }

        public bool TryGetObjectiveProgress(MissionData mission, MissionObjectiveKey objectiveKey, MissionObjectiveType objectiveType, out int currentProgress, out int requiredProgress)
        {
            currentProgress = 0;
            requiredProgress = 0;

            if(mission == null) return false;

            foreach(MissionRuntimeState state in missionStates)
            {
                if(state.Mission != mission) continue;
                return state.TryGetProgress(objectiveKey, objectiveType, out currentProgress, out requiredProgress);
            }

            return false;
        }
        #region MissionRuntimeState Class
        /// <summary>
        /// 런타임동안 특정 미션의 진행 상황을 저장하고 나타내는 클래스입니다. MissionManager에서 관리합니다.
        /// </summary>
        private sealed class MissionRuntimeState
        {
            private readonly int[] progress;
            public MissionData Mission {get;}
            public bool IsComplete
            {
                get
                {
                    IReadOnlyList<MissionObjectiveDefinition> objectives = Mission.Objectives;
                    if(objectives == null || objectives.Count == 0) return false;

                    for(int i = 0 ; i < objectives.Count ; i++)
                    {
                        MissionObjectiveDefinition objective = objectives[i];
                        if(objective == null || progress[i] < objective.RequiredAmount) return false;
                    }

                    return true;
                }
            }

            public MissionRuntimeState(MissionData mission)
            {
                Mission = mission;
                progress = new int[mission.Objectives.Count];
            }

            public bool TryApply(MissionProgressSignal signal, out MissionObjectiveDefinition progressedObjective, out int currentProgress)
            {
                progressedObjective = null;
                currentProgress = 0;

                IReadOnlyList<MissionObjectiveDefinition> objectives = Mission.Objectives;

                for(int i = 0 ; i < objectives.Count ; i++)
                {
                    MissionObjectiveDefinition objective = objectives[i];
                    if(objective == null || !objective.Matches(signal)) continue;

                    int prevProgress = progress[i];

                    progress[i] = Mathf.Min(objective.RequiredAmount, prevProgress + signal.Amount);
                    progressedObjective = objective;
                    currentProgress = progress[i];

                    return progress[i] != prevProgress;
                }

                return false;
            }

            public bool TryGetProgress(MissionObjectiveKey objectiveKey, MissionObjectiveType objectiveType, out int currentProgress, out int requiredProgress)
            {
                currentProgress = 0;
                requiredProgress = 0;

                IReadOnlyList<MissionObjectiveDefinition> objectives = Mission.Objectives;

                for(int i = 0 ; i < objectives.Count ; i++)
                {
                    MissionObjectiveDefinition objective = objectives[i];
                    if(objective == null || objective.ObjectiveKey != objectiveKey || objective.ObjectiveType != objectiveType) continue;

                    currentProgress = progress[i];
                    requiredProgress = objective.RequiredAmount;
                    return true;
                }

                return false;
            }
        }
        #endregion
    }
}