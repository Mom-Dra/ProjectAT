using UnityEngine;
using UnityEngine.EventSystems;
using PlayerStateMachine;
using PlayerStateCapabilities;
using System;
using System.Collections;

[RequireComponent(typeof(CrowdControlModule))]
[RequireComponent(typeof(StatusEffectScreenUI))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovementModule myMovementModule;
    [SerializeField] private PlayerAnimator myPlayerAnimator;
    [SerializeField] private PlayerCombatModule myCombatModule;
    [SerializeField] private PlayerSkillModule mySkillModule;
    [SerializeField] private PlayerCoverModule myCoverModule;
    [SerializeField] private PlayerInteractionModule myInteractionModule;
    [SerializeField] private PlayerSoundController mySoundController;
    [SerializeField] private EntityStatus myStatus;
    [SerializeField] private CrowdControlModule myCrowdControlModule;
    [SerializeField] private Camera myCamera;

    [Header("Enemy")]
    public Enemy SelectedEnemy { get; private set; }
    public Collider SelectedEnemyCollider { get; private set; }

    private Coroutine chaseCoroutine;
    private WaitForSeconds nextChaseWait = new WaitForSeconds(0.2f);
    public bool IsChasingEnemy => chaseCoroutine != null;


    [Header("Layers")]
    [SerializeField] private LayerMask rightClickInteractableLayer;

    #region StateMachine States
    public PlayerState CurrentState { get; private set; }
    public NormalState NormalState { get; private set; }
    public SkillChaseState SkillChaseState { get; private set; }
    public SkillCastState SkillCastingState { get; private set; }
    public SkillExecuteState SkillExecuteState { get; private set; }

    public DeadState DeadState { get; private set; }
    public InteractChaseState InteractChaseState { get; private set; }
    public InteractingState InteractingState { get; private set; }
    public CoverState CoverState { get; private set; }
    public CarryState CarryState { get; private set; }
    public StunnedState StunnedState { get; private set; }
    #endregion

    #region Module Getters
    public PlayerMovementModule MyMovementModule => myMovementModule;
    public PlayerAnimator MyAnimModule => myPlayerAnimator;
    public PlayerCombatModule MyCombatModule => myCombatModule;
    public PlayerSkillModule MySkillModule => mySkillModule;
    public PlayerCoverModule MyCoverModule => myCoverModule;
    public PlayerInteractionModule MyInteractionModule => myInteractionModule;
    public PlayerSoundController MySoundController => mySoundController;
    public EntityStatus MyStatus => myStatus;
    public CrowdControlModule MyCrowdControlModule => myCrowdControlModule;
    public bool IsStunned => myCrowdControlModule != null && myCrowdControlModule.IsStunned;
    #endregion

    #region 초기화
    private void InitiateComponents()
    {
        myMovementModule = GetComponent<PlayerMovementModule>();
        myPlayerAnimator = GetComponent<PlayerAnimator>();
        myCombatModule = GetComponent<PlayerCombatModule>();
        mySkillModule = GetComponent<PlayerSkillModule>();
        myCoverModule = GetComponent<PlayerCoverModule>();
        myInteractionModule = GetComponent<PlayerInteractionModule>();
        mySoundController = GetComponent<PlayerSoundController>();
        myStatus = GetComponent<EntityStatus>();
        myCrowdControlModule = GetComponent<CrowdControlModule>();
    }

    private void InitiateStateMachine()
    {
        NormalState = new NormalState(this);
        SkillChaseState = new SkillChaseState(this);
        SkillCastingState = new SkillCastState(this);
        SkillExecuteState = new SkillExecuteState(this);
        DeadState = new DeadState(this);
        InteractChaseState = new InteractChaseState(this);
        InteractingState = new InteractingState(this);
        CoverState = new CoverState(this);
        CarryState = new CarryState(this);
        StunnedState = new StunnedState(this);

        CurrentState = NormalState;
        CurrentState.OnEnter();
    }

    private void LinkInputEventsAll()
    {
        if (Managers.Instance.InputManager is null)
        {
            Debug.LogError("Managers.Instance.InputManager is null");
        }

        Managers.Instance.InputManager.onSkillInputed += HandlePlayerSkillInput;
        Managers.Instance.InputManager.onMouseRightClicked += HandleRightClickInput;
        Managers.Instance.InputManager.onMouseLeftClicked += HandleLeftClickInput;
        Managers.Instance.InputManager.OnInteractableObjectDropInput += HandleDropObjectInput;
        Managers.Instance.InputManager.OnReloadEvent += HandleReloadInput;
    }

    private void UnLinkInputEventsAll()
    {
        Managers.Instance.InputManager.onSkillInputed -= HandlePlayerSkillInput;
        Managers.Instance.InputManager.onMouseRightClicked -= HandleRightClickInput;
        Managers.Instance.InputManager.onMouseLeftClicked -= HandleLeftClickInput;
        Managers.Instance.InputManager.OnInteractableObjectDropInput -= HandleDropObjectInput;
        Managers.Instance.InputManager.OnReloadEvent -= HandleReloadInput;
    }
    #endregion

    #region 유니티 이벤트
    private void Awake()
    {
        InitiateComponents();
        InitiateStateMachine();
        myCamera = Camera.main;
    }

    private void OnEnable()
    {
        myStatus.onDeath += HandleDeath;
        myCrowdControlModule.OnStunStarted += HandleStunStarted;
        myCrowdControlModule.OnStunEnded += HandleStunEnded;
        LinkInputEventsAll();
        Managers.Instance.UIManager.InitPlayerStatusInfo(myStatus);
        //myStatus.onRevive += () => Debug.Log("Player Revived!"); // TODO : Revive 이벤트 활용
    }

    private void OnDisable()
    {
        myStatus.onDeath -= HandleDeath;
        if (myCrowdControlModule != null)
        {
            myCrowdControlModule.OnStunStarted -= HandleStunStarted;
            myCrowdControlModule.OnStunEnded -= HandleStunEnded;
        }

        //Managers.Instance.UIManager.ClearPlayerStatusInfo(); //TODO : UI 제거 함수 구현해야함.
        UnLinkInputEventsAll();
        //myStatus.onRevive -= () => Debug.Log("Player Revived!");
    }

    private void Update()
    {
        myCoverModule.HandleCoverRaycast(Managers.Instance.InputManager.MousePosition);

        CurrentState.OnUpdate();
        if (mySkillModule.IsTargetting) UpdateSkillIndicator();
        myPlayerAnimator.SetSpeed(myMovementModule.GetVelocity()); //애니메이션을 위한 이동속도 조절.
    }

    #endregion
    #region 입력 관련 함수

    public bool RaycastAtMouseLocation(out RaycastHit ray)
    {
        return Physics.Raycast(myCamera.ScreenPointToRay(Managers.Instance.InputManager.MousePosition), out ray, 100f, rightClickInteractableLayer);
    }

    
    public void HandleRightClickInput()
    {
        if (IsStunned) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (CurrentState is IRightClickHandler state && RaycastAtMouseLocation(out RaycastHit ray))
        {
            StopNormalAttackChase();
            state.OnRightClick(ray);
        }
    }

    public void HandleLeftClickInput()
    {
        if (IsStunned) return;

        if ((CurrentState is ILeftClickHandler state) && RaycastAtMouseLocation(out RaycastHit ray))
        {
            state.OnLeftClick(ray);
        }
    }

    public void HandlePlayerSkillInput(SkillNumber index)
    {
        if (IsStunned) return;

        if (CurrentState is ISkillInputHandler state)
        {
            state.OnSkillInput(index);
        }
    }

    public void HandleDropObjectInput()
    {
        if (IsStunned) return;

        if (CurrentState is IDropObjectHandler state)
        {
            state.OnDropObjectInput();
        }
    }

    private void UpdateSkillIndicator()
    {
        Ray ray = myCamera.ScreenPointToRay(Managers.Instance.InputManager.MousePosition);
        mySkillModule.UpdateSkillIndicator(ray);
    }

    public void PlayerMove(Vector3 pos, bool isRun)
    {
        if (IsStunned) return;

        if (isRun) myMovementModule.PlayerRun(pos);
        else myMovementModule.PlayerWalk(pos);
    }

    public void PlayerMoveWithIndicator(Vector3 pos, bool isRun)
    {
        if (IsStunned) return;

        PlayerMove(pos, isRun);
        IndicatorManager.Instance.ShowMoveIndicator(pos);
    }

    public void HandleReloadInput()
    {
        if (IsStunned) return;

        if (CurrentState is IReloadInputHandler state)
        {
            state.OnReloadInput();
        }
    }
    #endregion

    #region 전투관련 함수
    public void SetTargetEnemy(Enemy castedEnemy)
    {
        if (IsStunned) return;
        if (!castedEnemy) return;
        if (!myCombatModule.HasNormalAttackAmmo())
        {
            CancelEnemySelect();
            return;
        }
        if (!castedEnemy.TryGetComponent<Collider>(out _))
        {
            return;
        }

        SelectedEnemy = castedEnemy;
        SelectedEnemyCollider = castedEnemy.GetComponent<Collider>();
        myPlayerAnimator.SetAiming(false, SelectedEnemy.transform);
    }

    public void CancelEnemySelect()
    {
        SelectedEnemy = null;
        SelectedEnemyCollider = null;
        myPlayerAnimator.SetAiming(false, null);
    }

    public void UpdateNormalAttack(bool canChase)
    {
        if (IsStunned)
        {
            StopNormalAttackChase();
            return;
        }

        if (SelectedEnemy == null)
        {
            StopNormalAttackChase();
            AimingEnemy(false);
            return;
        }

        if (chaseCoroutine != null)
        {
            return;
        }

        if (!myCombatModule.HasNormalAttackAmmo())
        {
            StopNormalAttackChase();
            CancelEnemySelect();
            return;
        }

        if (!(myCombatModule.IsEnemyInWeaponRange(SelectedEnemy) && myCombatModule.IsTargetVisible(SelectedEnemyCollider)))
        {
            AimingEnemy(false);

            if (canChase)
            {
                chaseCoroutine = StartCoroutine(ChaseEnemyCoroutine());
            }

            return;
        }

        if (!myCombatModule.IsAiming)
        {
            myMovementModule.PlayerMoveStop();
            AimingEnemy(true, SelectedEnemy.transform);
            return;
        }

        if (!myCombatModule.CheckAimingTargetEnough())
        {
            myMovementModule.PlayerMoveStop();
            myMovementModule.PlayerRotateToward(SelectedEnemy.transform.position);
            return;
        }

        myMovementModule.PlayerMoveStop();

        if (myMovementModule.PlayerRotateToward(SelectedEnemy.transform.position))
        {
            NormalAttackEnemy();
        }
    }

    private IEnumerator ChaseEnemyCoroutine()
    {
        while (!IsStunned
        && SelectedEnemy != null
        && SelectedEnemyCollider != null
        && !(myCombatModule.IsEnemyInWeaponRange(SelectedEnemy) && myCombatModule.IsTargetVisible(SelectedEnemyCollider)))
        {
            myMovementModule.PlayerWalk(SelectedEnemy.transform.position);
            yield return nextChaseWait;
        }

        chaseCoroutine = null;
    }

    private void StopNormalAttackChase()
    {
        if (chaseCoroutine == null)
        {
            return;
        }

        StopCoroutine(chaseCoroutine);
        chaseCoroutine = null;
    }

    public void AimingEnemy(bool isAiming, Transform targetTf = default)
    {
        myPlayerAnimator.SetAiming(isAiming, targetTf);
        myCombatModule.SetAiming(isAiming);
    }

    private void NormalAttackEnemy()
    {
        if (myCombatModule.CheckWeaponFireReady())
        {
            myCombatModule.NormalAttackEnemy(SelectedEnemy);
        }
    }

    private void CancelAllPlayerAction()
    {
        InterruptCurrentAction();
    }

    public void InterruptCurrentAction()
    {
        myMovementModule.PlayerMoveStop();

        if (CurrentState is IInterruptiblePlayerState interruptibleState)
        {
            interruptibleState.Interrupt();
        }

        StopNormalAttackChase();

        if (mySkillModule.IsTargetting)
        {
            mySkillModule.CancelTargettingMode();
        }

        mySkillModule.CancelCurrentSkill();
        myCombatModule.RequestCancelReload();
        AimingEnemy(false);
        CancelEnemySelect();
    }

    private void HandleDeath()
    {
        CancelAllPlayerAction();
        ChangeState(PlayerStateType.Dead);
    }

    private void HandleStunStarted()
    {
        if (CurrentState == DeadState || (myStatus != null && myStatus.IsDead))
        {
            return;
        }

        InterruptCurrentAction();
        ChangeState(PlayerStateType.Stunned);
    }

    private void HandleStunEnded()
    {
        if (CurrentState == StunnedState && (myStatus == null || !myStatus.IsDead))
        {
            ChangeState(PlayerStateType.Normal);
        }
    }

    #endregion

    #region StateMachine관련 함수
    public void ChangeState(PlayerStateType newState)
    {
        if (IsStunned && newState != PlayerStateType.Stunned && newState != PlayerStateType.Dead)
        {
            return;
        }

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
            PlayerStateType.SkillExecute => SkillExecuteState,
            PlayerStateType.Dead => DeadState,
            PlayerStateType.Stunned => StunnedState,
            PlayerStateType.InteractChasing => InteractChaseState,
            PlayerStateType.Interacting => InteractingState,
            PlayerStateType.Cover => CoverState,
            PlayerStateType.Carry => CarryState,

            _ => throw new ArgumentException($"Undefined State Type: {type}"),
        };

        return nextState;
    }
    #endregion
}
