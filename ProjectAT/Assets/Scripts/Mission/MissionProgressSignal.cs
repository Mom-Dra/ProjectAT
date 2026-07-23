using UnityEngine;

namespace ProjectAT.Mission
{
    /// <summary>
    /// 미션이 진행됨을 알리는 신호 구조체입니다. 이 구조체는 미션 목표 키, 목표 유형, 진행된 양, 소스 게임 오브젝트, 그리고 액터 플레이어 컨트롤러를 포함합니다.
    /// </summary>
    public readonly struct MissionProgressSignal
    {
        public MissionObjectiveKey ObjectiveKey { get; }
        public MissionObjectiveType ObjectiveType { get; }
        public int Amount { get; }
        public GameObject Source { get; }
        public PlayerController Actor { get; }

        public MissionProgressSignal(MissionObjectiveKey key, MissionObjectiveType type, int amount, GameObject source, PlayerController actor)
        {
            ObjectiveKey = key;
            ObjectiveType = type;
            Amount = amount;
            Source = source;
            Actor = actor;
        }
    }
}