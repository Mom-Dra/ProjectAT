using UnityEngine;

public enum SkillType : ushort
{
    Targetting,
    Ground,
 }

public abstract class Skill
{
    protected PlayerSkillModule context;
    [SerializeField] protected SkillData skillData;
    //protected GameObject target;
    public float CurrSkillTime { get; protected set; }
    public SkillType SkillType => skillData.SkillType;
    public float SkillCastingTime => skillData.CastingTime;
    public LayerMask TargetLayer => skillData.TargetLayer;
    public SkillAnimationType AnimationType => skillData.AnimationType;

    public Skill(PlayerSkillModule context, SkillData skillData)
    {
        this.context = context;
        this.skillData = skillData;
        CurrSkillTime = 0f;
    }

    public virtual bool CanActivateSkill()
    {
        return skillData.MaxCoolTime <= Time.time - CurrSkillTime;
    }

    public abstract void OnUiActivate();
    public abstract void OnUiUpdate();
    public abstract void OnUiDeactivate();

    public abstract void CancelSkill();
    public abstract void OnChasing();
    public abstract bool CanSelectTarget(in RaycastHit hit);
    public abstract bool CanExecute();
    public abstract void Execute();

}
