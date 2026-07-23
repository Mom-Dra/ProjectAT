using System.Collections.Generic;
using UnityEngine;

namespace ProjectAT.Mission
{    
    [CreateAssetMenu(fileName = "NewMissionData", menuName = "Scriptable Objects/Mission/Mission Data")]
    public class MissionData : ScriptableObject
    {
        [SerializeField] private string missionName;
        [SerializeField, TextArea] private string description;

        [SerializeField] private List<MissionObjectiveDefinition> objectives = new List<MissionObjectiveDefinition>();

        public string MissionName => missionName;
        public string Description => description;
        public IReadOnlyList<MissionObjectiveDefinition> Objectives => objectives;
    }
}
