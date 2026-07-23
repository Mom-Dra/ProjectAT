using UnityEngine;
using System;

namespace ProjectAT.Mission
{
    /// <summary>
    /// 하나의 미션 목표를 정의하는 클래스입니다. MissionData 클래스가 이 Definition들을 모아 하나의 미션을 구성합니다.
    /// </summary>
    [Serializable]
    public sealed class MissionObjectiveDefinition
    {
        [SerializeField] private MissionObjectiveKey objectiveKey;
        [SerializeField] private MissionObjectiveType objectiveType;
        [SerializeField, Min(1)] private int requiredAmount = 1;

        public MissionObjectiveKey ObjectiveKey => objectiveKey;
        public MissionObjectiveType ObjectiveType => objectiveType;
        public int RequiredAmount => Mathf.Max(1, requiredAmount);

        public bool Matches(MissionProgressSignal signal)
        {
            return objectiveKey != null 
                && objectiveKey == signal.ObjectiveKey
                && objectiveType == signal.ObjectiveType;
        }
    }
}
