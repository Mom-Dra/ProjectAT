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
    public SkillType SkillType;         //이는 스킬의 커서 종류를 결정하는데 이용되야함. 따라서 커서 스킨은 제거하고 이걸 EffectModule에서 처리할 수 있도록 함.
    public IndicatorType IndicatorType;

    [Header("Visual & Animation")]
    public Texture2D CursorSkin;    //TargetingSkill
    public Sprite SkillIcon;
    public string AnimationTriggerName; //안쓰이면 삭제 할 것.
}
