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
    DesignatedFire,
    MainSkillOne,
    //MainSkillTwo,
    //Heal
}

public class PlayerSkillModule : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public PlayerController MyPlayerController { get; private set; }
    [SerializeField] public PlayerMovementModule MyMovementModule { get; private set; }
    [SerializeField] public PlayerCombatModule MyCombatModule { get; private set; }
    [SerializeField] public PlayerAnimator MyAnimModule { get; private set; }

    [Header("Skills")]
    private Skill[] mySkills = new Skill[5];
    private Skill CurrentActivateSkill;

    [Header("SkillDatas")]
    [SerializeField] private SkillData[] datas;     //Addressables 패키지를 이용하여 에셋을 읽어오는 방법 고려

    [Header("Params")]
    public SkillModuleState ModuleState { get; private set; }
    public bool isTargetting {get; private set;}
    private SkillNumber lastSkillInput;

    private Coroutine nowActivatedSkillCoroutine; 

    private void Awake()
    {
        MyPlayerController = GetComponent<PlayerController>();
        MyMovementModule = GetComponent<PlayerMovementModule>();
        MyCombatModule = GetComponent<PlayerCombatModule>();
        MyAnimModule = GetComponent<PlayerAnimator>();
    }

    private void InitiateSkills()
    {

        mySkills[(int)SkillNumber.DesignatedFire] = new DesignatedFire(this, datas[0]);
        mySkills[(int)SkillNumber.MainSkillOne] = new ThrowGrenade(this, datas[1]);
    }

    private void Start()
    {
        InitiateSkills();
        ModuleState = SkillModuleState.Ready;
        lastSkillInput = SkillNumber.None;
        isTargetting = false;
        nowActivatedSkillCoroutine = null;
    }

    public void SkillOnUpdate()
    {
        if (CurrentActivateSkill == null) return;
        if(ModuleState == SkillModuleState.Ready || nowActivatedSkillCoroutine != null) return;
        
        Debug.Log("Skill On update");
        if (CurrentActivateSkill.CanExecute())
        {   
            MyMovementModule.PlayerMoveStop();
            UsingCurrentSkill();
        }
        else
        {
            CurrentActivateSkill.OnChasing();
        }
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
        ModuleState = SkillModuleState.Casting;
        CurrentActivateSkill = mySkills[(int)lastSkillInput];
        CancelTargettingMode();
    }

    public void CancelCurrentSkill()
    {
        if(ModuleState != SkillModuleState.Casting) return;

        if(nowActivatedSkillCoroutine != null)
        {
            StopCoroutine(nowActivatedSkillCoroutine);
            nowActivatedSkillCoroutine = null;
        }

        CurrentActivateSkill.CancelSkill();
        CurrentActivateSkill = null;

        MyAnimModule.CancelAnimation();
        ModuleState = SkillModuleState.Ready;
    }

    private void UsingCurrentSkill()
    {
        nowActivatedSkillCoroutine = StartCoroutine(SkillActionCoroutine());
    }

    public bool CanActivateSkill (SkillNumber skillIndex)
    {
        return mySkills[(int)skillIndex].CanActivateSkill();
    }

    private IEnumerator SkillActionCoroutine()
    {
        //TODO : 스킬 사용을 코루틴 이용하려고함. 애니메이션 동기화 때문에ㅠㅠ 그러니 잘 구현해보기
        
        Debug.Log("Skill Coroutine Start");
        MyAnimModule.PlaySkillAnimation(CurrentActivateSkill.AnimationType);
        yield return new WaitForSeconds(CurrentActivateSkill.SkillCastingTime);
        
        CurrentActivateSkill.Execute();

        //후처리
        ModuleState = SkillModuleState.Ready;
        MyAnimModule.PlayIdle();
        CurrentActivateSkill = null;
        //MyPlayerController.CancelEnemySelect();
        nowActivatedSkillCoroutine = null;
        Debug.Log("Skill Coroutine End");
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
            ActivateSelectedSkill();
        }
    }
}
