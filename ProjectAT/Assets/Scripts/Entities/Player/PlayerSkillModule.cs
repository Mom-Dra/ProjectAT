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
    [SerializeField] public PlayerSoundController MySoundController { get; private set;}
    [SerializeField] public EntityStatus MyStatus { get; private set; }
    [SerializeField] public Inventory MyInventory {get; private set;}

    private SkillChaseState skillChaseState;
    private SkillCastState skillCastState;
    private SkillExecuteState skillExecuteState;


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
        MySoundController = GetComponent<PlayerSoundController>();

        groundLayer = LayerMask.GetMask("Ground");
    }

    private void Start()
    {
        InitiateSkills();
        InitiateSkillState();
        lastSkillInput = SkillNumber.None;
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
    }

    private void InitiateSkillState()
    {
        skillChaseState = MyPlayerController.GetState(PlayerStateType.SkillChase) as SkillChaseState;
        skillCastState = MyPlayerController.GetState(PlayerStateType.SkillCast) as SkillCastState;
        skillExecuteState = MyPlayerController.GetState(PlayerStateType.SkillExecute) as SkillExecuteState;
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
            case IndicatorType.ThrowingIndicator:
                IndicatorManager.Instance.ShowThrowingIndicator(MyCombatModule.ThrowPoint, (skillDatas[(int)lastSkillInput] as IAoESkillData).AoERadius);
                break;
            case IndicatorType.TargettingSkillIndicator:
                IndicatorManager.Instance.ShowAimingCursor();
                break;
            case IndicatorType.SectorAoEIndicator:
                IAoESkillData aoeSkillData = skillDatas[(int)lastSkillInput] as IAoESkillData;
                IndicatorManager.Instance.ShowSectorAoEIndicator(transform, aoeSkillData.AoERadius, aoeSkillData.AoELength);
                break;
            default:
                break;
        }

    }

    public void UpdateSkillIndicator(Ray mouseToScreenPosRay)
    {
        if(Physics.Raycast(mouseToScreenPosRay, out RaycastHit hit, 100f, groundLayer))
        {
            switch (mySkills[(int)lastSkillInput].IndicatorType)
            {
                case IndicatorType.ThrowingIndicator:
                    IndicatorManager.Instance.UpdateThrowingIndicator(hit.point, MyStatus.ThrowRange);
                    break;
                case IndicatorType.SectorAoEIndicator:
                    IndicatorManager.Instance.UpdateSectorAoEIndicator(hit.point);
                    break;
                default:
                    break;
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

    private bool CheckAnotherModuleCondition()
    {
        return !MyCombatModule.MyWeapon.IsReloading;
    }

    public bool CanSelectTarget(in RaycastHit hit, out Collider castedCollider, out Vector3 point)
    {
        castedCollider = null;
        point = Vector3.zero;

        if (!CheckAnotherModuleCondition()) return false;

        if(skillDatas[(int)lastSkillInput] is IAoESkillData 
        && (((1 << hit.collider.gameObject.layer) & groundLayer.value) != 0))
        {
            point = hit.point;
            return true;
        }

        return mySkills[(int)lastSkillInput].IsValidTarget(hit, out castedCollider, out point);
    }
    //아래부터 stateMachine을 위한 함수

    /// <summary>
    /// 스킬 시전이 가능한지 체크하는 함수. ChaseState에서 지속적으로 체크하면서 범위 내에 들어왔을 때 CastState로 전환하는 로직에서 사용한다.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool CanCastingSkill(SkillContext context)
    {
        if (context == null || context.SkillToExecute == null) return false;
        if(context.TargetCollider != null && !context.TargetCollider.gameObject.activeInHierarchy) return false; //타겟이 비활성화된 상태면 시전 불가능


        Vector3 destination = (context.TargetCollider != null) ? context.TargetCollider.transform.position : context.CastedPosition;
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
        float cooldownDuration = skill.SkillMaxCoolTime;

        if(mySkills[(int)currentActivateSkillNumber] is IInventoryCostSkill  inventorySkill)
        {
            int itemCount = MyInventory.GetItemCount(inventorySkill.NeededItemData);
            if(itemCount <= 0)
            {
                itemCount = 0;
                cooldownDuration = -1f; //SkillDisabled
            }
            OnSkillItemCountChange?.Invoke(currentActivateSkillNumber, itemCount);
        }

        OnSkillCooldownStart?.Invoke(currentActivateSkillNumber, cooldownDuration);
    }

    public void SetUpSkillContext(in Collider targetCollider, in Vector3 point)
    {
        Skill skill = mySkills[(int)currentActivateSkillNumber];
        SkillData skillData = skillDatas[(int)currentActivateSkillNumber];
        
        SkillContext skillContext = new SkillContext
        {
            SkillToExecute = skill,
            TargetCollider = targetCollider,
            CastedPosition = point,
            FinalDamage = skillData.BaseDamage, //데미지 계산 로직 필요 -> skillData.CalCulateFinalDamage()로 바꾸는 것.
            FinalRange = skill.CalculateFinalRange(), //사거리 계산 로직 필요 => skillData.CalculateFinalRange()로 바꾸는 것
        };

        SetSkillContext(skillContext);
    }

    private void SetSkillContext(SkillContext skillContext)
    {
        skillChaseState.SetSkillContext(skillContext);
        skillCastState.SetSkillContext(skillContext);
        skillExecuteState.SetSkillContext(skillContext);
    }

    private void CancelSkillContext()
    {
        SetSkillContext(null);
    }

}
