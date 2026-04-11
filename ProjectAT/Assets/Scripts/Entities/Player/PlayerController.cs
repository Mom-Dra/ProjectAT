using UnityEngine;
using UnityEngine.EventSystems;
using PlayerStateMachine;
using PlayerStatusCapabilities;
using System;


public enum PlayerInputType : ushort { LeftClick, RightClick, DesignatedFireKey }

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovementModule myMovementModule;
    //[SerializeField] private PlayerAnimationModule myAnimationModule;
    [SerializeField] private PlayerAnimator myPlayerAnimator;
    [SerializeField] private PlayerCombatModule myCombatModule;
    [SerializeField] private PlayerSkillModule mySkillModule;
    [SerializeField] private PlayerCoverModule myCoverModule;
    [SerializeField] private PlayerInteractionModule myInteractionModule;
    [SerializeField] private EntityStatus myStatus;
    [SerializeField] private Camera myCamera;

    [Header("Enemy")]
    public Enemy SelectedEnemy;
    public GameObject SelectedObject;

    [Header("Layers")]
    //[SerializeField] private LayerMask groundLayer;
    //[SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask rightClickInteractableLayer;
    [Header("Params")]
    [SerializeField] private float TickRate = 0.2f;
    private float LastTickTime = 0f;

    #region StateMachine States
    public PlayerState CurrentState { get; private set; }
    public NormalState NormalState{ get; private set; }
    public SkillChaseState SkillChaseState { get; private set; }
    public SkillCastState SkillCastingState { get; private set; }
    public DeadState DeadState { get; private set; }
    #endregion

    #region Module Getters
    public PlayerMovementModule MyMovementModule => myMovementModule;
    public PlayerAnimator MyAnimModule => myPlayerAnimator;
    public PlayerCombatModule MyCombatModule => myCombatModule;
    public PlayerSkillModule MySkillModule => mySkillModule;
    public PlayerCoverModule MyCoverModule => myCoverModule;
    public PlayerInteractionModule MyInteractionModule => myInteractionModule;    
    #endregion

    // //일단 로직 다 짜고, 이 로직이 이 클래스에 있는지 검증하자!
    // private CoverObject currCoverObject;
    // private CoverPoint currCoverPoint;
    // private Coroutine moveCoroutine;
    // private CoverPoint targetCoverPoint;

    #region 초기화
    private void InitiateComponents()
    {
        myMovementModule = GetComponent<PlayerMovementModule>();
        myPlayerAnimator = GetComponent<PlayerAnimator>();
        myCombatModule = GetComponent<PlayerCombatModule>();
        mySkillModule = GetComponent<PlayerSkillModule>();
        myCoverModule = GetComponent<PlayerCoverModule>();
        myInteractionModule = GetComponent<PlayerInteractionModule>();
        myStatus = GetComponent<EntityStatus>();
    }
    private void InitiateStateMachine()
    {
        NormalState = new NormalState(this);
        SkillChaseState = new SkillChaseState(this);
        SkillCastingState = new SkillCastState(this);
        DeadState = new DeadState(this);
        CurrentState = NormalState;
        CurrentState.OnEnter();
    }

    private void LinkInputEventsAll()
    {
        //inputReader.InputEvent += HandleInput;
        if (Managers.Instance.InputManager is null)
            Debug.LogError("Managers.Instance.InputManager is null");

        Managers.Instance.InputManager.onSkillInputed += HandlePlayerSkillInput;
        Managers.Instance.InputManager.onMouseRightClicked += HandlePlayerRightClickInput;
        Managers.Instance.InputManager.onMouseLeftClicked += HandleLeftClickInput;
    }

    private void UnLinkInputEventsAll()
    {
        //inputReader.InputEvent -= HandleInput;
        Managers.Instance.InputManager.onSkillInputed -= HandlePlayerSkillInput;
        Managers.Instance.InputManager.onMouseRightClicked -= HandlePlayerRightClickInput;
        Managers.Instance.InputManager.onMouseLeftClicked -= HandleLeftClickInput;
    }
    #endregion

    #region 유니티 이벤트
    private void Awake()
    {
        InitiateComponents();
        InitiateStateMachine();
        myCamera = Camera.main;
        LastTickTime = Time.time;

    }

    private void Start()
    {
        LinkInputEventsAll();
        Managers.Instance.UIManager.InitPlayerStatusInfo(myStatus);
        myStatus.onDeath += CancelAllPlayerAction;
        myStatus.onRevive += () => Debug.Log("Player Revived!"); // TODO : Revive 이벤트 활용
    }

    private void OnDisable()
    {
        UnLinkInputEventsAll();
        myStatus.onDeath -= CancelAllPlayerAction;
        myStatus.onRevive -= () => Debug.Log("Player Revived!");
    }

    private void Update()
    {
        // //myAnimationModule.SetRunningAnimation(myMovementModule.IsAgentMoving());
        // if (mySkillModule.ModuleState != SkillModuleState.Ready)
        // {
        //     mySkillModule.SkillOnUpdate();
        // }
        // else if (SelectedEnemy != null)
        // {
        //     EnemyAttackingSequence();
        //     //ChaseEnemy();
        //     //NormalAttackEnemy();
        // }    

        // if(Time.time - LastTickTime > TickRate)
        // {
        //     myCoverModule.HandleCoverRaycast(Managers.Instance.InputManager.MousePosition);
        //     myInteractionModule.HandleInteractionRaycast(Managers.Instance.InputManager.MousePosition);

        //     LastTickTime = Time.time;
        // }

        // myPlayerAnimator.SetSpeed(myMovementModule.GetVelocity());
        // if(mySkillModule.IsTargetting) mySkillModule.SkillIndicatorUpdate();
        myCoverModule.HandleCoverRaycast(Managers.Instance.InputManager.MousePosition);
        myInteractionModule.HandleInteractionRaycast(Managers.Instance.InputManager.MousePosition);
        
        CurrentState.OnUpdate();
        if(mySkillModule.IsTargetting) UpdateSkillIndicator(); //나중에 Sniping 스킬을 
        myPlayerAnimator.SetSpeed(myMovementModule.GetVelocity()); //애니메이션을 위한 이동속도 조절.
    }

    #endregion
    #region 입력 관련 함수

    public void HandlePlayerRightClickInput()
    {
        if(EventSystem.current.IsPointerOverGameObject()) return;
        
        
        //if(mySkillModule.IsTargetting) //스킬 타겟팅 모드에서 우클릭하면 스킬 취소
        // {
        //     mySkillModule.CancelTargettingMode();
        //     return;
        // }

        // mySkillModule.CancelCurrentSkill(); //스킬 casting 또는 스킬 chasing의 exit 부분
        // AimingEnemy(false); 
        // CancelEnemySelect();
 
        // NormalRightClickAction();
        if(CurrentState is IRightClickHandler state)
        {
            state.OnRightClick(RaycastAtMouseLocation(out RaycastHit ray) ? ray : new RaycastHit());
        }

    }

    // private void NormalRightClickAction()
    // {
    //     RaycastHit ray;
    //     if (RaycastAtMouseLocation(out ray))
    //     {
    //         myCoverModule.CancelCurrentCoverAction();

    //         Debug.Log($"NormalRightClickAction: {ray.collider.gameObject.layer}");

    //         switch (ray.collider.gameObject.layer)
    //         {
    //             case 6: //Ground Layer
    //                 Debug.Log("플레이어 컨트롤러 : PlayerMove");
    //                 PlayerMove(ray.point, false);
    //                 break;
    //             case 7: //Enemy Layer
    //                 SetTargetEnemy(ray.collider.GetComponent<Enemy>());
    //                 break;
    //             case 10: //Indicator Layer
    //                 PlayerMove(ray.point, true);
    //                 break;
    //             case 11: // CoverPoint Layer
    //                 if (ray.transform.TryGetComponent(out CoverPoint coverPoint))
    //                     myCoverModule.StartMoveToCover(coverPoint);
    //                 break;
    //             case 13: // Interactable Layer
    //                 Debug.Log("Interactable Object Clicked");
    //                 myInteractionModule.HandleRightClick();
    //                 break;
    //             default:
    //                 break;
    //         }
    //     }
    // }

    public bool RaycastAtMouseLocation(out RaycastHit ray)
    {
        return Physics.Raycast(myCamera.ScreenPointToRay(Managers.Instance.InputManager.MousePosition), out ray, 100f, rightClickInteractableLayer);
    }

    public void HandleLeftClickInput()
    {
        // if(mySkillModule.IsTargetting)
        // {
        //     mySkillModule.SelectTarget();
        // }
        if(CurrentState is ILeftClickHandler state)
        {
            state.OnLeftClick(RaycastAtMouseLocation(out RaycastHit ray) ? ray : new RaycastHit());
        }
    }

    public void HandlePlayerSkillInput(SkillNumber index)
    {
        // if(myStatus.IsDead) return;
        // mySkillModule.ActivateTargettingMode(index);
        if(CurrentState is ISkillInputHandler state)
        {
            state.OnSkillInput(index);
        }
    }

    private void UpdateSkillIndicator()
    {
        Ray ray = myCamera.ScreenPointToRay(Managers.Instance.InputManager.MousePosition);
        mySkillModule.UpdateSkillIndicator(ray);
    }

    public void PlayerMove(Vector3 pos, bool isRun)
    {
        if (isRun) myMovementModule.PlayerRun(pos);
        else myMovementModule.PlayerWalk(pos);
    }

    public void PlayerMoveWithIndicator(Vector3 pos, bool isRun)
    {        
        PlayerMove(pos, isRun);
        IndicatorManager.Instance.ShowMoveIndicator(pos, IndicatorType.MoveIndicator, 1.0f);
    }
    #endregion

    #region 전투관련 함수
    public void SetTargetEnemy(Enemy castedEnemy)
    {
        if (!castedEnemy) return;
        SelectedEnemy = castedEnemy;        
    }

    public void CancelEnemySelect()
    {
        SelectedEnemy = null;
    }

    public void EnemyAttackingSequence()
    {
        if (myCombatModule.IsEnemyInWeaponSight(SelectedEnemy))
        {
            myMovementModule.PlayerMoveStop();
            AimingEnemy(true, SelectedEnemy.transform);

            if (myMovementModule.PlayerRotateToward(SelectedEnemy.transform.position))
            {
                NormalAttackEnemy();
            }
        }
        else
        {
            AimingEnemy(false);
            ChaseEnemy();
        }
    }

    private void ChaseEnemy()
    {
        myMovementModule.PlayerWalk(SelectedEnemy.transform.position);
    }

    public void AimingEnemy(bool isAiming, Transform targetTf = default)
    {
        myPlayerAnimator.SetAiming(isAiming, targetTf);
        myCombatModule.SetAiming(isAiming);
    }

    private void NormalAttackEnemy()
    {
        if (myCombatModule.CanFire())
        {
            myCombatModule.NormalAttackEnemy(SelectedEnemy);
            //myEffectModule.PlayFiringEffect(SelectedEnemy.transform.position);                
        }
        if(myCombatModule.MyWeapon.NowWeapon.RemainAmmo <= 0)
        {
            //재장전 로직
            
        }
    }

    private void CancelAllPlayerAction()
    {
        myMovementModule.PlayerMoveStop();
        mySkillModule.CancelCurrentSkill();
        CancelEnemySelect();
    }
    #endregion
    #region StateMachine관련 함수
    public void ChangeState(PlayerStateType newState)
    {
        CurrentState?.OnExit();
        CurrentState = GetState(newState);
        CurrentState.OnEnter();
    }

    public PlayerState GetState(PlayerStateType type)
    {
        PlayerState nextState = type switch
        {
            PlayerStateType.Normal => NormalState,
            PlayerStateType.SkillChase => SkillChaseState,
            PlayerStateType.SkillCast => SkillCastingState,
            PlayerStateType.Dead => DeadState,
            _ => throw new ArgumentException($"Undefined State Type: {type}"),
        };

        return nextState;
    }
    #endregion
}
