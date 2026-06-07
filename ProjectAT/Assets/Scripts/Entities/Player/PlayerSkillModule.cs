using System;
using System.Collections.Generic;
using PlayerStateMachine;
using SkillDataOptionInterfaces;
using SkillOptionInterfaces;
using UnityEngine;

public enum SkillNumber : short
{
    None = -1,
    MainSkillOne,   //q
    MainSkillTwo,   //w
    Grenade,        //e
    UseBandage,     //d
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
    [SerializeField] public Inventory MyInventory {get; private set;}

    private SkillChaseState skillChaseState;
    private SkillCastState skillCastState;

    [Header("Skills")]
    private Skill[] mySkills = new Skill[5]; //갯수 조정 필요
    private SkillNumber currentActivateSkillNumber;
    private Dictionary<Skill, float> skillCooldownTimers = new Dictionary<Skill, float>();
    public WeaponHolder MyWeapon => MyCombatModule.MyWeapon;

    [Header("SkillDatas")]
    [SerializeField] private SkillData[] skillDatas;     //Addressables 패키지를 이용하여 에셋을 읽어오는 방법 고려

    [Header("Params")]
    private SkillNumber lastSkillInput;
    public bool IsTargetting {get{ return lastSkillInput != SkillNumber.None; }}
    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;

    public event Action<SkillNumber,float> OnSkillCooldownStart;
    public event Action<SkillNumber, int> OnSkillItemCountChange;

    private void Awake()
    {
        MyPlayerController = GetComponent<PlayerController>();
        MyMovementModule = GetComponent<PlayerMovementModule>();
        MyCombatModule = GetComponent<PlayerCombatModule>();
        MyAnimModule = GetComponent<PlayerAnimator>();
        MyStatus = GetComponent<EntityStatus>();
        MyInventory = GetComponent<Inventory>();

        groundLayer = LayerMask.GetMask("Ground");
    }

    private void InitiateSkills()
    {
     
        for(int i = (int)SkillNumber.MainSkillOne ; i < mySkills.Length ; i++)
        {
            mySkills[i] = SkillFactory.Create(this, skillDatas[i]);
            if(mySkills[i] == null)
            {
                Debug.LogError($"Failed to create skill for SkillNumber {(SkillNumber)i} with SkillData {skillDatas[i]?.name}. Check if the SkillData is correct and if the SkillFactory has a creation method for this skill.");
                mySkills[i] = new DummySkill(this, skillDatas[i]);
            }
            skillCooldownTimers.Add(mySkills[i], Time.time);
        }
        
        Managers.Instance.UIManager.InitPlayerSkillInfo(this, skillDatas);

        for(int i = (int)SkillNumber.MainSkillOne ; i < mySkills.Length ; i++)
        {
            OnSkillCooldownStart?.Invoke((SkillNumber)i, mySkills[i].SkillMaxCoolTime);
            if(mySkills[i] is IInventoryCostSkill inventoryCostSkill)
            {
                OnSkillItemCountChange?.Invoke((SkillNumber)i, MyInventory.GetItemCount(inventoryCostSkill.NeededItemData));
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
            //Debug.Log($"Cannot Activate Skill:{skillIndex}");
            return;
        }
        if(lastSkillInput != SkillNumber.None || lastSkillInput == skillIndex)
        {
            CancelTargettingMode();
            return;
        }

        lastSkillInput = skillIndex;
        switch (mySkills[(int)skillIndex].IndicatorType)
        {
            case IndicatorType.SkillAoEIndicator:
                IndicatorManager.Instance.ShowAreaIndicator(MyCombatModule.ThrowPoint, (skillDatas[(int)lastSkillInput] as IAoESkillData).AoERadius);
                break;
            case IndicatorType.TargettingSkillIndicator:
                IndicatorManager.Instance.ShowAimingCursor();
                break;
            default:
                break;
        }

    }

    public void UpdateSkillIndicator(Ray mouseToScreenPosRay)
    {
        if(mySkills[(int)lastSkillInput].IndicatorType == IndicatorType.SkillAoEIndicator)
        {
            if(Physics.Raycast(mouseToScreenPosRay, out RaycastHit hit, 100f, groundLayer))
            {
                IndicatorManager.Instance.UpdateAoeIndicator(transform.position, hit.point, Vector3.zero, MyStatus.ThrowRange);
            }
        }
    }

    public void CancelTargettingMode()
    {
        IndicatorManager.Instance.HideIndicator(mySkills[(int)lastSkillInput].IndicatorType);
        lastSkillInput = SkillNumber.None;
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
        return mySkills[(int)lastSkillInput].IsValidTarget(hit, out target, out point);
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
            return context.SkillToExecute.ExtraCastingCondition(context);
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

        // TODO : 아이템 사용형 스킬 사용 시 인벤토리 아이템 갯수 변경 이벤트 로직 구현하기
        if(mySkills[(int)currentActivateSkillNumber] is IConsumableSkillData consumableSkill)
        {
            OnSkillItemCountChange?.Invoke(currentActivateSkillNumber, MyInventory.GetItemCount(consumableSkill.NeededItemData));
            //TODO : 만약 아이템이 부족한 경우라면 아예 비활성화 시키기.
        }
    }

    public void SetUpSkillContext(in GameObject target, in Vector3 point)
    {
        SkillContext skillContext = new SkillContext
        {
            SkillToExecute = mySkills[(int)currentActivateSkillNumber],
            TargetObject = target,
            CastedPosition = point,
            FinalDamage = skillDatas[(int)currentActivateSkillNumber].BaseDamage, //데미지 계산 로직 필요 -> skill.CalCulateFinalDamage()로 바꾸는 것.
            FinalRange = (skillDatas[(int)currentActivateSkillNumber] is ProjectileSkillData) ? MyStatus.ThrowRange : MyWeapon.Range, //사거리 계산 로직 필요 => skill.CalculateFinalRange()로 바꾸는 것
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
