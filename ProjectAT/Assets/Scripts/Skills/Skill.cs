using UnityEngine;

public enum SkillType : ushort
{
    Throwing,
    Sniping
 }

public abstract class Skill
{
    protected PlayerSkillModule context;
    [SerializeField] protected SkillData skillData;
    public float CurrSkillTime { get; protected set; }
    public SkillType SkillType => skillData.SkillType;
    public float SkillCastingTime => skillData.CastingTime;

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

    public abstract void OnChasing(Enemy target);
    public abstract bool CanExecute(Enemy target);
    public abstract void Execute(Enemy target);

}
