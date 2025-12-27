using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    public string Name;
    //public Sprite Icon;
    public float MaxCoolTime;
    public float Damage;
    public GameObject SkillEffectPrefab;
    public Texture2D CursorSkin;
    public SkillType SkillType;
    public float CastingTime;
    public LayerMask TargetLayer;
    public SkillAnimationType AnimationType;
}
