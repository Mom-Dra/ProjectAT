using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Skills/Basic Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Stats")]
    public string Name;
    public int BaseDamage;                  // 자기버프형의 경우 버프량으로 사용
    public float CastingTime;
    public float MaxCoolTime;
    public LayerMask TargetLayer;

    [Header("Visual & Animation")]
    public IndicatorType IndicatorType;
    public Texture2D CursorSkin;    //TargetingSkill
    public Sprite SkillIcon;
    public string AnimationTriggerName; //안쓰이면 삭제 할 것.
}
