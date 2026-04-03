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
    public abstract bool CanExecute(SkillContext skillContext);
    public abstract void Execute(SkillContext skillContext);
}
