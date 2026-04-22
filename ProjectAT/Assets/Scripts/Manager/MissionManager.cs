using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField]
    private List<MissionData> activeMissions = new List<MissionData>();
    private HashSet<MissionData> completedMissions = new HashSet<MissionData>();

    private bool[] isDeadPlayers = new bool[(int)PlayerNumber.Last];

    private void Awake()
    {

    }

    private void OnEnable()
    {
        Managers.Instance.EventManager.Subscribe<EnemyIdentity>(EventType.TargetDied, TargetDied);
        Managers.Instance.EventManager.Subscribe<PlayerNumber>(EventType.PlayerDied, PlayerDied);
    }

    private void OnDisable()
    {
        Managers.Instance.EventManager.UnSubscribe<EnemyIdentity>(EventType.TargetDied, TargetDied);
        Managers.Instance.EventManager.UnSubscribe<PlayerNumber>(EventType.PlayerDied, PlayerDied);
    }

    private void TargetDied(EnemyIdentity enemyIdentity)
    {
        // UpdateMission(MissionType.KillTarget, id);
        Debug.Log($"Handle {enemyIdentity.EnemyName}");

        foreach (MissionData missionData in activeMissions)
        {
            if (completedMissions.Contains(missionData)) continue;

            if (missionData is KillTargetData killTargetData && killTargetData.EnemyIdentity == enemyIdentity)
            {
                CompleteMission(missionData);
                break;
            }
        }
    }

    private void PlayerDied(PlayerNumber playerType)
    {
        isDeadPlayers[(int)playerType] = true;

        foreach (bool isDead in isDeadPlayers)
        {
            if (!isDead) return;
        }

        FailMission();
    }

    private void PlayerRevived(PlayerNumber playerType)
    {
        isDeadPlayers[(int)playerType] = false;
    }

    private void CompleteMission(MissionData mission)
    {
        completedMissions.Add(mission);

        if (completedMissions.Count >= activeMissions.Count)
        {
            ClearMission();
        }
    }

    private void FailMission()
    {
        Managers.Instance.SceneManager.LoadSceneAsync(SceneType.End);
    }

    private void ClearMission()
    {
        // OnMissionCleared?.Invoke();
        // 파르티잔은 탈추구역이 오픈되는 방식이에요!!
        // 탈출구역Object.SetActive(true);
        Managers.Instance.SceneManager.LoadSceneAsync(SceneType.End);
    }
}
