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
    [SerializeField] public PlayerAnimationModule MyAnimModule { get; private set; }

    [Header("Skills")]
    private Skill[] mySkills = new Skill[5];
    private Skill DesignatedFire;
    private Skill CurrentActivateSkill;

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
        MyAnimModule = GetComponent<PlayerAnimationModule>();
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
    }

    public void SkillOnUpdate()
    {
        if (CurrentActivateSkill == null || ModuleState == SkillModuleState.Ready) return;
        Debug.Log("Skill On update");
        if (CurrentActivateSkill.CanExecute(MyPlayerController.SelectedEnemy))
        {
            if(MyMovementModule.PlayerRotateToward(MyPlayerController.SelectedEnemy.transform.position))
            {
                MyAnimModule.PlayFiringAnimation();
                CurrentActivateSkill.Execute(MyPlayerController.SelectedEnemy);
                ModuleState = SkillModuleState.Ready;
            }
        }
        else
        {
            MyMovementModule.PlayerWalk(MyPlayerController.SelectedEnemy.transform.position);
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
        Debug.Log("Skill Selected");
        ModuleState = SkillModuleState.Casting;
        CurrentActivateSkill = mySkills[(int)lastSkillInput];
        CancelTargettingMode();
    }

    public void CancelSkill()
    {
        ModuleState = SkillModuleState.Ready;
        CurrentActivateSkill = null;
    }

    public bool CanActivateSkill (SkillNumber skillIndex)
    {
        return mySkills[(int)skillIndex].CanActivateSkill();
    }
}
