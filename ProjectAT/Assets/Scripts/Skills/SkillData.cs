using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Skills/Basic Skill Data")]
public class SkillData : ScriptableObject
{
#region common Datas
    public string Name;
    //public Sprite Icon;
    public float MaxCoolTime;
    public int Damage; // 자기버프형의 경우 버프량으로 사용
    public GameObject SkillEffectPrefab;
    public Texture2D CursorSkin;
    public Sprite SkillIcon;
    public SkillType SkillType;
    public float CastingTime;
    public LayerMask TargetLayer;
#endregion
#region Ground Skill Datas
    public float AOERadius = 1.0f;
#endregion
}
