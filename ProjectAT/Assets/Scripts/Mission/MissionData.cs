using UnityEngine;

public class MissionData : ScriptableObject
{
    [SerializeField]
    private string missionName;
    [SerializeField]
    [TextArea]
    private string description;
}