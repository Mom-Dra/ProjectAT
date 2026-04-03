using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayerStateMachine;
using Unity.VisualScripting;
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

    private SkillChaseState skillChaseState;
    private SkillCastState skillCastState;

    [Header("Skills")]
    private Skill[] mySkills = new Skill[5]; //갯수 조정 필요
    //private Skill CurrentActivateSkill;
    private SkillNumber currentActivateSkillNumber;
    private float currentSkillTimer = 0.0f;
    private Dictionary<Skill, float> skillCooldownTimers = new Dictionary<Skill, float>();
    public WeaponHolder MyWeapon => MyCombatModule.MyWeapon;

    [Header("SkillDatas")]
    [SerializeField] private SkillData[] skillDatas;     //Addressables 패키지를 이용하여 에셋을 읽어오는 방법 고려

    [Header("Params")]
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
        mySkills[(int)SkillNumber.MainSkillOne] = new DummySkill(this, skillDatas[(int)SkillNumber.MainSkillOne]); // 팩토리 패턴 필요?
        mySkills[(int)SkillNumber.MainSkillTwo] = new DummySkill(this, skillDatas[(int)SkillNumber.MainSkillTwo]);
        mySkills[(int)SkillNumber.Grenade] = new ThrowGrenade(this, skillDatas[(int)SkillNumber.Grenade]);
        mySkills[(int)SkillNumber.UseBandage] = new UseBandage(this, skillDatas[(int)SkillNumber.UseBandage]);
        mySkills[(int)SkillNumber.DesignatedFire] = new DesignatedFire(this, skillDatas[(int)SkillNumber.DesignatedFire]);
        
        Managers.Instance.UIManager.InitPlayerSkillInfo(this, skillDatas);

        for(int i = 0 ; i < mySkills.Length ; i++)
        {
            OnSkillCooldownStart?.Invoke((SkillNumber)i, mySkills[i].SkillMaxCoolTime);
            if(mySkills[i] is ConsumableSkill consumableSkill)
            {
                OnSkillItemCountChange?.Invoke((SkillNumber)i, MyInventory.GetItemCount(consumableSkill.NeededItemData));
            }
        }

        skillChaseState = MyPlayerController.GetState(PlayerStateType.SkillChase) as SkillChaseState;
        skillCastState = MyPlayerController.GetState(PlayerStateType.SkillCast) as SkillCastState;

    }

    private void Start()
    {
        InitiateSkills();
        lastSkillInput = SkillNumber.None;
    }

    public void ActivateTargettingMode(SkillNumber skillIndex)
    {
        if (!CanActivateSkill(skillIndex))
        {
            Debug.Log($"Cannot Activate Skill:{skillIndex}");
            return;
        }
        if(lastSkillInput != SkillNumber.None || lastSkillInput == skillIndex)
        {
            CancelTargettingMode();
        }
        else 
        {
            //MyEffectModule.ShowIndicator(mySkills[(int)currentActivateSkillNumber].IndicatorType);
            lastSkillInput = skillIndex;
            //IsTargetting = true;
        }
    }

    public void CancelTargettingMode()
    {
        MyEffectModule.HideIndicator(mySkills[(int)lastSkillInput].IndicatorType);
        lastSkillInput = SkillNumber.None;
        //IsTargetting = false;
    }

    public void ActivateSelectedSkill()
    {
        // //ModuleState = SkillModuleState.Casting;
        // ModuleState = SkillModuleState.Chasing;
        
        // currentActivateSkillNumber = lastSkillInput;
        // MyCombatModule.SetAiming(false);
        // CancelTargettingMode();
        currentActivateSkillNumber = lastSkillInput;
        CancelTargettingMode();
    }

    public void SkillIndicatorUpdate()
    {
        //MyEffectModule.UpdateIndicator(mySkills[(int)currentActivateSkillNumber].TargetPosition, MyMovementModule.MyRigidbody.velocity, mySkills[(int)currentActivateSkillNumber].IndicatorType);
    }

    public void CancelCurrentSkill()
    {
        CancelSkillContext();
        currentActivateSkillNumber = SkillNumber.None;
        MyAnimModule.CancelAnimation();
    }

    public bool CanActivateSkill (SkillNumber skillIndex)
    {
        return mySkills[(int)skillIndex].CanActivate();
    }

    public bool CanSelectTarget(in RaycastHit hit, out GameObject target, out Vector3 point)
    {
        return mySkills[(int)currentActivateSkillNumber].IsValidTarget(hit, out target, out point);
    }
    //아래부터 stateMachine을 위한 함수

    /// <summary>
    /// 스킬 시전이 가능한지 체크하는 함수. 스킬 시전 가능 범위 내에 있는지, 벽 등으로 가려져 있지는 않은지 등을 체크한다. ChaseState에서 지속적으로 체크하면서 범위 내에 들어왔을 때 CastState로 전환하는 로직에서 사용한다.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool CanCastingSkill(SkillContext context)
    {
        if (context == null || context.SkillToExecute == null) return false;
        if(context.TargetObject != null && !context.TargetObject.activeInHierarchy) return false; //타겟이 비활성화된 상태면 시전 불가능

        Vector3 destination = (context.TargetObject != null) ? context.TargetObject.transform.position : context.CastedPosition;
        float sqrtDistance = Vector3.SqrMagnitude(transform.position - destination);

        if (sqrtDistance <= context.FinalRange * context.FinalRange)
        {
            // 중간에 벽이 있는지 체크
            return MyCombatModule.IsTargetInWeaponSight(context.TargetObject);
        }

        return false;    
    }

    public bool IsCooldownReady(Skill skill)
    {
        return Time.time - skillCooldownTimers[skill] >= skill.SkillMaxCoolTime;
    }

    public void SetSkillCooldownTimer(Skill skill)
    {
        skillCooldownTimers[skill] = Time.time;
        OnSkillCooldownStart?.Invoke(currentActivateSkillNumber, skill.SkillMaxCoolTime);

        // 갯수 제거형 스킬 사용 시 인벤토리 아이템 갯수 변경 이벤트 로직 구현하기
        // if(mySkills[(int)currentActivateSkillNumber] is ConsumableSkill consumableSkill)
        // {
        //     OnSkillItemCountChange?.Invoke(currentActivateSkillNumber, MyInventory.GetItemCount(consumableSkill.NeededItemData));
        // }
    }

    public void SetUpSkillContext(in GameObject target, in Vector3 point)
    {
        SkillContext skillContext = new SkillContext
        {
            SkillToExecute = mySkills[(int)currentActivateSkillNumber],
            TargetObject = target,
            CastedPosition = point,
        };

        skillChaseState.SetSkillContext(skillContext);
        skillCastState.SetSkillContext(skillContext);
    }

    private void CancelSkillContext()
    {
        skillChaseState.SetSkillContext(null);
        skillCastState.SetSkillContext(null);
    }
}
