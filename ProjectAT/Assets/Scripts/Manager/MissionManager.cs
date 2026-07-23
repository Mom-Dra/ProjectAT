using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectAT.Mission;
using EPOOutline;

public class MissionManager : MonoBehaviour
{
    [SerializeField] private List<MissionData> activeMissions = new List<MissionData>();
    private readonly Dictionary<MissionData, MissionRuntimeState> missionStates = new Dictionary<MissionData, MissionRuntimeState>();
    private readonly HashSet<MissionData> completedMissions = new HashSet<MissionData>();

    private readonly bool[] isDeadPlayers = new bool[(int)PlayerId.Last];
    private EventManager eventManager;

    public event Action<MissionData, MissionObjectiveDefinition, int> ObjectiveProgressed;
    public event Action<MissionData> MissionCompleted;

    private void Awake()
    {
        BuildRuntimeStates();
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

        foreach(MissionData mission in activeMissions)
        {
            if(mission == null) continue;
            if (missionStates.ContainsKey(mission))
            {
                Debug.LogWarning($"활성 미션 목록에 {mission.name}이 중복 등록되었습니다.", this);
                continue;
            }
            missionStates.Add(mission, new MissionRuntimeState(mission));
        }
    }

    private void HandleMissionProgressed(MissionProgressSignal signal)
    {
        if(signal.ObjectiveKey == null || signal.Amount <= 0) return;

        foreach(MissionRuntimeState state in missionStates.Values)
        {
            MissionData mission = state.Mission;
            if(completedMissions.Contains(mission)) continue;
            if(!state.TryApply(signal, out MissionObjectiveDefinition objective, out int currentProgress) ) continue;
            Debug.Log( $"Mission '{mission.MissionName}' progress: " + $"{objective.ObjectiveKey.DisplayName} " + $"{currentProgress}/{objective.RequiredAmount}");

            ObjectiveProgressed?.Invoke(mission, objective, currentProgress);
            if(state.IsComplete) CompleteMission(mission);
        }
    }

    private void HandlePlayerDied(PlayerId playerId)
    {
        int index = (int)playerId;
        if(index < 0 || index>= isDeadPlayers.Length) return;

        isDeadPlayers[index] = true;

        foreach(bool isDead in isDeadPlayers)
        {
            if(!isDead) return;
        }

        StatgeFail();
    }

    private void CompleteMission(MissionData mission)
    {
        if(!completedMissions.Add(mission)) return;
        Debug.Log($"Mission '{mission.MissionName}' completed!");
        MissionCompleted?.Invoke(mission);

        if(missionStates.Count > 0 && completedMissions.Count >= missionStates.Count)
        {
            StageClear();
        }
    }

    public bool TryGetObjectiveProgress(MissionData mission, MissionObjectiveKey objectiveKey, MissionObjectiveType objectiveType, out int currentProgress, out int requiredProgress)
    {
        currentProgress = 0;
        requiredProgress = 0;
        return mission != null
            && missionStates.TryGetValue(mission, out MissionRuntimeState state)
            && state.TryGetProgress(objectiveKey, objectiveType, out currentProgress, out requiredProgress);
    }

    private void StatgeFail()
    {
        Managers.Instance.SceneManager.LoadSceneAsync(SceneType.End); //실패 컷신같은걸 재생해야..?(26.07.22)
    }

    private void StageClear()
    {
        Managers.Instance.SceneManager.LoadSceneAsync(SceneType.End); //성공 컷신같은걸 재생해야..?(26.07.22)
    }

    #region MissionRuntimeState Class
    /// <summary>
    /// 런타임동안 미션 진행 상황을 저장하고 나타내는 클래스입니다. MissionManager에서 관리합니다.
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