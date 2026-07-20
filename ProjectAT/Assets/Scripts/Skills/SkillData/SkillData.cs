using UnityEngine;

public enum SkillId
{
    Dummy,
    ThrowRock,
    ThrowGrenade,
    UseBandage,
    DesignatedFire,
    SuppressiveFire,
    MultiShot,
    ThrowFlashBang,
}


[CreateAssetMenu(fileName = "New Skill Data", menuName = "Skills/Basic Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Stats")]
    [SerializeField] private SkillId skillId;
    [SerializeField] private string skillName;
    [SerializeField] private int baseDamage;
    [SerializeField] private float castingTime;
    [SerializeField] private float maxCoolTime;
    [SerializeField] private LayerMask targetLayer;

    [Header("Visual & Animation")]
    [SerializeField] private IndicatorType indicatorType;
    [SerializeField] private Texture2D cursorSkin;    //TargetingSkill
    [SerializeField] private Sprite skillIcon;
    [SerializeField] private string animationTriggerName; //안쓰이면 삭제 할 것.

    [Header("Audio")]
    [SerializeField] private AudioClip castingStartSound;


    #region Getters
    public SkillId SkillId => skillId;
    public string Name => skillName;
    public int BaseDamage => baseDamage;
    public float CastingTime => castingTime;
    public float MaxCoolTime => maxCoolTime;
    public LayerMask TargetLayer => targetLayer;
    public IndicatorType IndicatorType => indicatorType;
    public Texture2D CursorSkin => cursorSkin;
    public Sprite SkillIcon => skillIcon;
    public string AnimationTriggerName => animationTriggerName; 
    public AudioClip CastingStartSound => castingStartSound;
    #endregion
}
