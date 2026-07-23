using UnityEngine;

namespace ProjectAT.Mission
{
    /// <summary>
    /// 미션 목표를 나타내는 키입니다. 미션 이름이 되기도 합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "MissionObjectiveKey", menuName = "Scriptable Objects/Mission/MissionObjectiveKey")]
    public class MissionObjectiveKey : ScriptableObject
    {
        [SerializeField] private string displayName;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    }
}