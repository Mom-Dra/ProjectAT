using System.Collections;
using UnityEngine;

public enum  SkillModuleState : ushort
{
    Ready,
    Chasing,
    Casting,
}

public enum SkillNumber : ushort
{
    None,
    DesignatedFire, //a
    UseBandage,     //r
    Grenade,        //e
    MainSkillOne,
    //MainSkillTwo,
}

public class PlayerSkillModule : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public PlayerController MyPlayerController { get; private set; }
    [SerializeField] public PlayerMovementModule MyMovementModule { get; private set; }
    [SerializeField] public PlayerCombatModule MyCombatModule { get; private set; }
    [SerializeField] public PlayerAnimator MyAnimModule { get; private set; }
    [SerializeField] public EntityStatus MyStatus { get; private set; }

    [Header("Skills")]
    private Skill[] mySkills = new Skill[5];
    private Skill CurrentActivateSkill;
    private float currentSkillTimer = 0.0f;

    [Header("SkillDatas")]
    [SerializeField] private SkillData[] datas;     //Addressables 패키지를 이용하여 에셋을 읽어오는 방법 고려

    [Header("Params")]
    public SkillModuleState ModuleState { get; private set; }
    public bool isTargetting {get; private set;}
    private SkillNumber lastSkillInput;

    private void Awake()
    {
        MyPlayerController = GetComponent<PlayerController>();
        MyMovementModule = GetComponent<PlayerMovementModule>();
        MyCombatModule = GetComponent<PlayerCombatModule>();
        MyAnimModule = GetComponent<PlayerAnimator>();
        MyStatus = GetComponent<EntityStatus>();
    }

    private void InitiateSkills()
    {
        mySkills[(int)SkillNumber.DesignatedFire] = new DesignatedFire(this, datas[0]);
        mySkills[(int)SkillNumber.UseBandage] = new UseBandage(this, datas[1]);
        mySkills[(int)SkillNumber.Grenade] = new ThrowGrenade(this, datas[2]);
    }

    private void Start()
    {
        InitiateSkills();
        ModuleState = SkillModuleState.Ready;
        lastSkillInput = SkillNumber.None;
        isTargetting = false;
    }

    public void SkillOnUpdate() //리펙토링 요소 : 상태패턴으로 정의 가능
    {
        if (CurrentActivateSkill == null) return;
        if(ModuleState == SkillModuleState.Ready) return;
        
        if(ModuleState == SkillModuleState.Chasing)
        {
            SkillOnChasing();
            return;
        }
        
        if(ModuleState == SkillModuleState.Casting)
        {
            SkillOnCasting();
        }
    }

    private void SkillOnChasing()
    {
        if (CurrentActivateSkill.CanExecute())
        {
            ModuleState = SkillModuleState.Casting;
            MyMovementModule.PlayerMoveStop();
            CurrentActivateSkill.OnCastingStart();
        }
        else
        {
            CurrentActivateSkill.OnChasing();
        }        
    }

    private void SkillOnCasting()
    {
        if(!CurrentActivateSkill.CanExecute())
        {
            ChangeToChasingState();
            return;
        }
        
        //회전체크, 잔류 속도 체크
        if (CurrentActivateSkill.SkillType != SkillType.Self 
        && !MyMovementModule.PlayerRotateToward(CurrentActivateSkill.TargetPosition)
        && MyAnimModule.GetSpeedValue() > 0.005f)
        {
            return;
        }
        currentSkillTimer += Time.deltaTime;

        if (currentSkillTimer >= CurrentActivateSkill.SkillCastingTime)
        {
            CurrentActivateSkill.Execute();
            CurrentActivateSkill.OnCastingEnd();

            //후처리
            ModuleState = SkillModuleState.Ready;
            CurrentActivateSkill = null;
            currentSkillTimer = 0f;
        }
    }

    private void ChangeToChasingState()
    {
        ModuleState = SkillModuleState.Chasing;
        CurrentActivateSkill.OnChasingStart();
        currentSkillTimer = 0f;
    }

    public void ActivateTargettingMode(SkillNumber skillIndex)
    {
        //if (!CanActivateSkill(skillIndex) || 1 > (int)skillIndex || (int)skillIndex >= mySkills.Length) return;
        if(lastSkillInput != SkillNumber.None || lastSkillInput == skillIndex)
        {
            CancelTargettingMode();
        }
        else 
        {
            mySkills[(int)skillIndex].OnUiActivate();
            lastSkillInput = skillIndex;
            isTargetting = true;
        }
    }

    public void CancelTargettingMode()
    {
        mySkills[(int)lastSkillInput].OnUiDeactivate();
        lastSkillInput = SkillNumber.None;
        isTargetting = false;
    }

    public void ActivateSelectedSkill()
    {
        //ModuleState = SkillModuleState.Casting;
        ModuleState = SkillModuleState.Chasing;
        
        CurrentActivateSkill = mySkills[(int)lastSkillInput];
        CancelTargettingMode();
    }

    public void CancelCurrentSkill()
    {
        if(ModuleState == SkillModuleState.Ready) return;

        CurrentActivateSkill.CancelSkill();
        CurrentActivateSkill = null;

        currentSkillTimer = 0f;
        
        MyAnimModule.CancelAnimation();
        ModuleState = SkillModuleState.Ready;
    }

    public bool CanActivateSkill (SkillNumber skillIndex)
    {
        return mySkills[(int)skillIndex].CanActivateSkill();
    }

    private bool CanSelectTarget(in RaycastHit hit)
    {
        return mySkills[(int)lastSkillInput].CanSelectTarget(hit);
    }

    public void SelectTarget()
    {
        MyPlayerController.RaycastAtMouseLocation(out RaycastHit hit);
        
        if(CanSelectTarget(hit))
        {
            CancelCurrentSkill();
            ActivateSelectedSkill();
        }
    }
}
