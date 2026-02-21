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
    public float CurrSkillTime { get; protected set; }

    public SkillType SkillType => skillData.SkillType;
    public float SkillCastingTime => skillData.CastingTime;
    public LayerMask TargetLayer => skillData.TargetLayer;

    public abstract Vector3 TargetPosition {get;} //목표 대상의 위치. 스킬들은 반드시 이 값을 주기적으로 갱신할 수 있도록 해야함.

    public Skill(PlayerSkillModule context, SkillData skillData)
    {
        this.context = context;
        this.skillData = skillData;
        CurrSkillTime = 0f;
    }
    public float GetSkillCooldownPercent()
    {
        float cooldownProgress = (Time.time - CurrSkillTime) / skillData.MaxCoolTime;
        return Mathf.Clamp01(cooldownProgress);
    }
    public virtual bool CanActivateSkill()
    {
        return skillData.MaxCoolTime <= Time.time - CurrSkillTime;
    }

    public abstract void OnUiActivate();
    public abstract void OnUiUpdate();
    public abstract void OnUiDeactivate();

    public abstract void CancelSkill();

    public abstract void OnChasingStart();
    public abstract void OnChasing();

    public abstract bool CanSelectTarget(in RaycastHit hit);
    public abstract bool CanExecute();

    public abstract void OnCastingStart(); //캐스팅을 시작할 때 호출
    public abstract void Execute();
    public abstract void OnCastingEnd(); //Execute가 호출된 후 호출
}
