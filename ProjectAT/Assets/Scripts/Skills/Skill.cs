using System;
using UnityEngine;

public enum SkillType : ushort
{
    Targetting,
    Ground,
    Self
 }

public abstract class Skill
{
    protected PlayerSkillModule context;
    [SerializeField] protected SkillData skillData;
    //protected GameObject target;
    public LayerMask TargetLayer => skillData.TargetLayer;
    public IndicatorType IndicatorType => skillData.IndicatorType;
    public float SkillMaxCoolTime => skillData.MaxCoolTime;
    public string AnimationName => skillData.AnimationTriggerName;
    public float CastTime => skillData.CastingTime;
    public int BaseDamage => skillData.BaseDamage;
    
    public Skill(PlayerSkillModule context, SkillData skillData)
    {
        this.context = context;
        this.skillData = skillData;
    }
    public virtual bool CanActivate()
    {
        return context.IsCooldownReady(this);
    }

    public abstract bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point);
    public virtual bool ExtraCastingCondition(SkillContext context) {return true;} // 스킬 고유의 추가적인 캐스팅 조건이 필요한 경우 오버라이드해서 사용. 예를 들어 투척류 스킬은 던질 위치에 장애물이 없는지 체크할 수 있음. 사거리 체크는 기본적으로 SkillModule에서 수행함.
    public abstract bool CanExecute(SkillContext skillContext);
    public abstract void Execute(SkillContext skillContext);
    public abstract float CalCulateFinalDamage();
    public abstract float CalculateFinalRange();

    #region Animation Functions
    public virtual void OnCastingStart(SkillContext skillContext) { }
    public virtual void OnCastingEnd(SkillContext skillContext) { }
    #endregion

}
