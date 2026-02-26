using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public enum  SkillModuleState : ushort
{
    Ready,
    Chasing,
    Casting,
}

public enum SkillNumber : short
{
    None = -1,

    MainSkillOne,   //q
    MainSkillTwo,   //w
    Grenade,        //e
    UseBandage,     //r
    DesignatedFire, //a
}

public class PlayerSkillModule : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public PlayerController MyPlayerController { get; private set; }
    [SerializeField] public PlayerMovementModule MyMovementModule { get; private set; }
    [SerializeField] public PlayerCombatModule MyCombatModule { get; private set; }
    [SerializeField] public PlayerAnimator MyAnimModule { get; private set; }
    [SerializeField] public EntityStatus MyStatus { get; private set; }
    [SerializeField] public EffectModule MyEffectModule {get; private set;}
    [SerializeField] public Inventory MyInventory {get; private set;}

    [Header("Skills")]
    private Skill[] mySkills = new Skill[5]; //갯수 조정 필요
    //private Skill CurrentActivateSkill;
    private SkillNumber currentActivateSkillNumber;
    private float currentSkillTimer = 0.0f;

    [Header("SkillDatas")]
    [SerializeField] private SkillData[] datas;     //Addressables 패키지를 이용하여 에셋을 읽어오는 방법 고려

    [Header("Params")]
    public SkillModuleState ModuleState { get; private set; }
    private SkillNumber lastSkillInput;
    public bool IsTargetting {get{ return lastSkillInput != SkillNumber.None; }}

    public event Action<SkillNumber,float> OnSkillCooldownStart;
    public event Action<SkillNumber, int> OnSkillItemCountChange;

    private void Awake()
    {
        MyPlayerController = GetComponent<PlayerController>();
        MyMovementModule = GetComponent<PlayerMovementModule>();
        MyCombatModule = GetComponent<PlayerCombatModule>();
        MyAnimModule = GetComponent<PlayerAnimator>();
        MyEffectModule = GetComponent<EffectModule>();
        MyStatus = GetComponent<EntityStatus>();
        MyInventory = GetComponent<Inventory>();
    }

    private void InitiateSkills()
    {
        mySkills[(int)SkillNumber.MainSkillOne] = new DummySkill(this, datas[(int)SkillNumber.MainSkillOne]); // 팩토리 패턴 필요?
        mySkills[(int)SkillNumber.MainSkillTwo] = new DummySkill(this, datas[(int)SkillNumber.MainSkillTwo]);
        mySkills[(int)SkillNumber.Grenade] = new ThrowGrenade(this, datas[(int)SkillNumber.Grenade]);
        mySkills[(int)SkillNumber.UseBandage] = new UseBandage(this, datas[(int)SkillNumber.UseBandage]);
        mySkills[(int)SkillNumber.DesignatedFire] = new DesignatedFire(this, datas[(int)SkillNumber.DesignatedFire]);
        
        Managers.Instance.UIManager.InitPlayerSkillInfo(this, datas);

        for(int i = 0 ; i < mySkills.Length ; i++)
        {
            OnSkillCooldownStart?.Invoke((SkillNumber)i, mySkills[i].SkillMaxCoolTime);
            if(mySkills[i] is ConsumableSkill consumableSkill)
            {
                OnSkillItemCountChange?.Invoke((SkillNumber)i, MyInventory.GetItemCount(consumableSkill.NeededItemData));
            }
        }
    }

    private void Start()
    {
        InitiateSkills();
        ModuleState = SkillModuleState.Ready;
        lastSkillInput = SkillNumber.None;

    }

    public void SkillOnUpdate() //리펙토링 요소 : 상태패턴으로 정의 가능
    {
        if(currentActivateSkillNumber == SkillNumber.None) return;

        switch (ModuleState)
        {
            case SkillModuleState.Ready:
                break;
            case SkillModuleState.Chasing:
                SkillOnChasing();
                break;
            case SkillModuleState.Casting:
                SkillOnCasting();
                break;
        }
    }

    private void SkillOnChasing()
    {
        if (mySkills[(int)currentActivateSkillNumber].CanExecute())
        {
            ModuleState = SkillModuleState.Casting;
            MyMovementModule.PlayerMoveStop();
            mySkills[(int)currentActivateSkillNumber].OnCastingStart();
        }
        else
        {
            mySkills[(int)currentActivateSkillNumber].OnChasing();
        }        
    }

    private void SkillOnCasting()
    {
        if(!mySkills[(int)currentActivateSkillNumber].CanExecute())
        {
            ChangeToChasingState();
            return;
        }
        
        //회전체크, 잔류 속도 체크
        if (mySkills[(int)currentActivateSkillNumber].SkillType != SkillType.Self 
        && !MyMovementModule.PlayerRotateToward(mySkills[(int)currentActivateSkillNumber].TargetPosition)
        && MyAnimModule.GetSpeedValue() > 0.005f)
        {
            return;
        }
        currentSkillTimer += Time.deltaTime;

        if (currentSkillTimer >= mySkills[(int)currentActivateSkillNumber].SkillCastingTime)
        {
            mySkills[(int)currentActivateSkillNumber].Execute();
            mySkills[(int)currentActivateSkillNumber].OnCastingEnd();

            //후처리
            OnSkillCooldownStart?.Invoke(currentActivateSkillNumber, mySkills[(int)currentActivateSkillNumber].SkillMaxCoolTime);
            if(mySkills[(int)currentActivateSkillNumber] is ConsumableSkill consumableSkill)
            {
                OnSkillItemCountChange?.Invoke(currentActivateSkillNumber, MyInventory.GetItemCount(consumableSkill.NeededItemData));
            }
            ModuleState = SkillModuleState.Ready;
            currentActivateSkillNumber = SkillNumber.None;
            currentSkillTimer = 0f;
        }
    }

    private void ChangeToChasingState()
    {
        ModuleState = SkillModuleState.Chasing;
        mySkills[(int)currentActivateSkillNumber].OnChasingStart();
        currentSkillTimer = 0f;
    }

    public void ActivateTargettingMode(SkillNumber skillIndex)
    {
        if (!CanActivateSkill(skillIndex))
        {
            Debug.Log($"Cannot Activate Skill:{skillIndex}");
            return;
        }
        //if (!CanActivateSkill(skillIndex) || 1 > (int)skillIndex || (int)skillIndex >= mySkills.Length) return;
        if(lastSkillInput != SkillNumber.None || lastSkillInput == skillIndex)
        {
            CancelTargettingMode();
        }
        else 
        {
            mySkills[(int)skillIndex].OnUiActivate();
            lastSkillInput = skillIndex;
            //IsTargetting = true;
        }
    }

    public void CancelTargettingMode()
    {
        mySkills[(int)lastSkillInput].OnUiDeactivate();
        lastSkillInput = SkillNumber.None;
        //IsTargetting = false;
    }

    public void ActivateSelectedSkill()
    {
        //ModuleState = SkillModuleState.Casting;
        ModuleState = SkillModuleState.Chasing;
        
        currentActivateSkillNumber = lastSkillInput;
        CancelTargettingMode();
    }

    public void SkillIndicatorUpdate()
    {
        mySkills[(int)lastSkillInput].OnUiUpdate();
    }

    public void CancelCurrentSkill()
    {
        if(ModuleState == SkillModuleState.Ready) return;

        mySkills[(int)currentActivateSkillNumber].CancelSkill();
        currentActivateSkillNumber = SkillNumber.None;

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
