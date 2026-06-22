using UnityEngine;
using UnityEngine.EventSystems;
using PlayerStateMachine;
using PlayerStateCapabilities;
using System;


public enum PlayerInputType : ushort { LeftClick, RightClick, DesignatedFireKey }

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovementModule myMovementModule;
    [SerializeField] private PlayerAnimator myPlayerAnimator;
    [SerializeField] private PlayerCombatModule myCombatModule;
    [SerializeField] private PlayerSkillModule mySkillModule;
    [SerializeField] private PlayerCoverModule myCoverModule;
    [SerializeField] private PlayerInteractionModule myInteractionModule;
    [SerializeField] private EntityStatus myStatus;
    [SerializeField] private Camera myCamera;

    [Header("Enemy")]
    public Enemy SelectedEnemy {get; private set;}

    private const float AttackChaseRangeOffset = 1f;
    private const float AttackAimReleaseMargin = 0.75f;

    [Header("Layers")]
    [SerializeField] private LayerMask rightClickInteractableLayer;

    #region StateMachine States
    public PlayerState CurrentState { get; private set; }
    public NormalState NormalState{ get; private set; }
    public SkillChaseState SkillChaseState { get; private set; }
    public SkillCastState SkillCastingState { get; private set; }
    public SkillExecuteState SkillExecuteState { get; private set; }

    public DeadState DeadState { get; private set; }
    public InteractChaseState InteractChaseState { get; private set; }
    public InteractingState InteractingState { get; private set; }
    public CoverState CoverState { get; private set; }
    public CarryState CarryState { get; private set; }
    #endregion

    #region Module Getters
    public PlayerMovementModule MyMovementModule => myMovementModule;
    public PlayerAnimator MyAnimModule => myPlayerAnimator;
    public PlayerCombatModule MyCombatModule => myCombatModule;
    public PlayerSkillModule MySkillModule => mySkillModule;
    public PlayerCoverModule MyCoverModule => myCoverModule;
    public PlayerInteractionModule MyInteractionModule => myInteractionModule;
    public EntityStatus MyStatus => myStatus;
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
        myStatus = GetComponent<EntityStatus>();
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
        Managers.Instance.InputManager.onMouseRightClicked += HandlePlayerRightClickInput;
        Managers.Instance.InputManager.onMouseLeftClicked += HandleLeftClickInput;
        Managers.Instance.InputManager.OnInteractableObjectDropInput += HandleDropObjectInput;
        Managers.Instance.InputManager.OnReloadEvent += HandleReloadInput;
    }

    private void UnLinkInputEventsAll()
    {
        Managers.Instance.InputManager.onSkillInputed -= HandlePlayerSkillInput;
        Managers.Instance.InputManager.onMouseRightClicked -= HandlePlayerRightClickInput;
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
        LinkInputEventsAll();
        Managers.Instance.UIManager.InitPlayerStatusInfo(myStatus);
        //myStatus.onRevive += () => Debug.Log("Player Revived!"); // TODO : Revive 이벤트 활용
    }

    private void OnDisable()
    {
        myStatus.onDeath -= HandleDeath;
        //Managers.Instance.UIManager.ClearPlayerStatusInfo(); //TODO : UI 제거 함수 구현해야함.
        UnLinkInputEventsAll();
        //myStatus.onRevive -= () => Debug.Log("Player Revived!");
    }

    private void Update()
    {
        myCoverModule.HandleCoverRaycast(Managers.Instance.InputManager.MousePosition);
        
        CurrentState.OnUpdate();
        if(mySkillModule.IsTargetting) UpdateSkillIndicator();
        myPlayerAnimator.SetSpeed(myMovementModule.GetVelocity()); //애니메이션을 위한 이동속도 조절.
    }

    #endregion
    #region 입력 관련 함수

    public void HandlePlayerRightClickInput()
    {
        if(EventSystem.current.IsPointerOverGameObject()) return;
        
        if(CurrentState is IRightClickHandler state && RaycastAtMouseLocation(out RaycastHit ray))
        {
            state.OnRightClick(ray);
        }
    }

    public bool RaycastAtMouseLocation(out RaycastHit ray)
    {
        return Physics.Raycast(myCamera.ScreenPointToRay(Managers.Instance.InputManager.MousePosition), out ray, 100f, rightClickInteractableLayer);
    }

    public void HandleLeftClickInput()
    {
        if((CurrentState is ILeftClickHandler state) && RaycastAtMouseLocation(out RaycastHit ray))
        {
            state.OnLeftClick(ray);
        }
    }

    public void HandlePlayerSkillInput(SkillNumber index)
    {
        if(CurrentState is ISkillInputHandler state)
        {
            state.OnSkillInput(index);
        }
    }

    public void HandleDropObjectInput()
    {
        if(CurrentState is IDropObjectHandler state)
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
        if (isRun) myMovementModule.PlayerRun(pos);
        else myMovementModule.PlayerWalk(pos);
    }

    public void PlayerMoveWithIndicator(Vector3 pos, bool isRun)
    {        
        PlayerMove(pos, isRun);
        IndicatorManager.Instance.ShowMoveIndicator(pos);
    }

    public void HandleReloadInput()
    {
        if(CurrentState is IReloadInputHandler state)
        {
            state.OnReloadInput();
        }
    }
    #endregion

    #region 전투관련 함수
    public void SetTargetEnemy(Enemy castedEnemy)
    {
        if (!castedEnemy) return;
        if (!myCombatModule.HasNormalAttackAmmo())
        {
            CancelEnemySelect();
            return;
        }

        SelectedEnemy = castedEnemy;
        myPlayerAnimator.SetAiming(false, SelectedEnemy.transform);
    }

    public void CancelEnemySelect()
    {
        SelectedEnemy = null;
        myPlayerAnimator.SetAiming(false, null);
    }

    /// <summary>
    /// 공격 시도 함수. 사거리 내에 적이 있으면 공격 로직 수행 후 true 반환, 사거리 밖이면 false 반환 (즉, 공격 실패)
    /// </summary>
    /// <returns></returns>
    public bool TryExecuteAttack()
    {
        if (SelectedEnemy == null) 
        {
            return false;
        }

        if (myCombatModule.IsEnemyInWeaponSight(SelectedEnemy))
        {
            myMovementModule.PlayerMoveStop();
            AimingEnemy(true, SelectedEnemy.transform);

            if (myMovementModule.PlayerRotateToward(SelectedEnemy.transform.position))
            {
                NormalAttackEnemy();
            }
            return true;
        }

        return false;
    }

    public void UpdateNormalAttack(bool canChase)
    {
        if (SelectedEnemy == null)
        {
            AimingEnemy(false);
            return;
        }

        if (!myCombatModule.HasNormalAttackAmmo())
        {
            CancelEnemySelect();
            return;
        }

        bool enemyInSight = myCombatModule.IsEnemyInWeaponSight(SelectedEnemy);

        if (!myCombatModule.IsAiming)
        {
            if (enemyInSight)
            {
                myMovementModule.PlayerMoveStop();
                AimingEnemy(true, SelectedEnemy.transform);
                return;
            }

            AimingEnemy(false);

            if (canChase)
            {
                ChaseEnemy();
            }

            return;
        }

        if (!myCombatModule.CheckAimingTargetEnough())
        {
            myMovementModule.PlayerMoveStop();
            myMovementModule.PlayerRotateToward(SelectedEnemy.transform.position);
            return;
        }

        if (!enemyInSight)
        {
            AimingEnemy(false);

            if (canChase)
            {
                ChaseEnemy();
            }

            return;
        }

        myMovementModule.PlayerMoveStop();

        if (myMovementModule.PlayerRotateToward(SelectedEnemy.transform.position))
        {
            NormalAttackEnemy();
        }
    }

    public bool CanKeepAimingSelectedEnemy()
    {
        if (SelectedEnemy == null) return false;

        return myCombatModule.IsEnemyInWeaponSight(SelectedEnemy, AttackAimReleaseMargin);
    }

    public void ChaseEnemy()
    {
        if (SelectedEnemy == null) return;

        Vector3 playerPosition = transform.position;
        Vector3 enemyPosition = SelectedEnemy.transform.position;
        Vector3 directionFromEnemyToPlayer = playerPosition - enemyPosition;
        directionFromEnemyToPlayer.y = 0f;

        if (directionFromEnemyToPlayer.sqrMagnitude < 0.001f)
        {
            myMovementModule.PlayerWalk(enemyPosition);
            return;
        }

        float chaseDistance = Mathf.Max(0f, myCombatModule.MyWeapon.Range - AttackChaseRangeOffset);
        Vector3 chasePosition = enemyPosition + directionFromEnemyToPlayer.normalized * chaseDistance;

        myMovementModule.PlayerWalk(chasePosition);
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
        myMovementModule.PlayerMoveStop();
        mySkillModule.CancelCurrentSkill();
        myCombatModule.RequestCancelReload();
        CancelEnemySelect();
    }

    private void HandleDeath()
    {
        CancelAllPlayerAction();
        ChangeState(PlayerStateType.Dead);
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
            PlayerStateType.SkillExecute => SkillExecuteState,
            PlayerStateType.Dead => DeadState,
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
