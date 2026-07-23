using UnityEngine;

namespace ProjectAT.Mission
{
    public interface IMissionProgressSource
    {
        MissionObjectiveKey ObjectiveKey {get;}
        MissionObjectiveType ObjectiveType {get;}
    }
}