using EPOOutline.Demo;
using System.Collections;
using System.Linq;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public enum PlayerInputType : ushort { LeftClick, RightClick, DesignatedFireKey }

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerMovementModule myMovementModule;
    //[SerializeField] private PlayerAnimationModule myAnimationModule;
    [SerializeField] private PlayerAnimator myPlayerAnimator;
    [SerializeField] private PlayerCombatModule myCombatModule;
    [SerializeField] private PlayerSkillModule mySkillModule;
    [SerializeField] private PlayerCoverModule myCoverModule;
    [SerializeField] private EffectModule myEffectModule;
    [SerializeField] private Camera myCamera;

    [Header("Enemy")]
    public Enemy SelectedEnemy;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask rightClickInteractableLayer;

    [Header("Params")]
    [SerializeField] private float TickRate = 0.2f;
    private float LastTickTime = 0f;
    private SkillNumber lastSkillInput;


    // 일단 로직 다 짜고
    // 이 로직이 이 클래스에 있는지 검증하자!
    private CoverObject currCoverObject;
    private CoverPoint currCoverPoint;

    private Coroutine moveCoroutine;
    private CoverPoint targetCoverPoint;

    #region 초기화
    private void InitiateComponents()
    {
        myMovementModule = GetComponent<PlayerMovementModule>();
        myPlayerAnimator = GetComponent<PlayerAnimator>();
        myEffectModule = GetComponent<EffectModule>();
        myCombatModule = GetComponent<PlayerCombatModule>();
        mySkillModule = GetComponent<PlayerSkillModule>();
        myCoverModule = GetComponent<PlayerCoverModule>();
    }

    private void LinkInputEventsAll()
    {
        //inputReader.InputEvent += HandleInput;
        inputReader.MouseRightClickEvent += HandlePlayerRightClickInput;
        inputReader.SkillInputEvent += HandlePlayerSkillInput;
        inputReader.MouseLeftClickEvent += HandleLeftClickInput;
    }

    private void UnLinkInputEventsAll()
    {
        //inputReader.InputEvent -= HandleInput;
        inputReader.MouseRightClickEvent -= HandlePlayerRightClickInput;
        inputReader.SkillInputEvent -= HandlePlayerSkillInput;
        inputReader.MouseLeftClickEvent -= HandleLeftClickInput;
    }
    #endregion

    #region 유니티 이벤트
    private void Awake()
    {
        InitiateComponents();
        myCamera = Camera.main;
        LastTickTime = Time.time;
        lastSkillInput = SkillNumber.None;
    }

    private void OnEnable()
    {
        LinkInputEventsAll();
    }

    private void OnDisable()
    {
        UnLinkInputEventsAll();
    }

    private void Update()
    {
        if(Time.time - LastTickTime > TickRate)
        {
            //myAnimationModule.SetRunningAnimation(myMovementModule.IsAgentMoving());
            
            if (mySkillModule.ModuleState == SkillModuleState.Casting)
            {
                mySkillModule.SkillOnUpdate();
                return;
            }
            else if (SelectedEnemy != null)
            {
                ChaseEnemy();
                NormalAttackEnemy();
            }

            myCoverModule.HandleCoverRaycast(inputReader.MousePosition);

            LastTickTime = Time.time;
        }

        myPlayerAnimator.SetSpeed(myMovementModule.GetVelocity());

    }

    #endregion
    #region 입력 관련 함수

    public void HandlePlayerRightClickInput()
    {
        if(EventSystem.current.IsPointerOverGameObject()) return;
        
        if(mySkillModule.isTargetting)
        {
            mySkillModule.CancelTargettingMode();
            return;
        }
        mySkillModule.CancelCurrentSkill();
        CancelNormalAttack();
        CancelEnemySelect();

        NormalRightClickAction();
    }

    private void NormalRightClickAction()
    {
        RaycastHit ray;
        if (RaycastAtMouseLocation(out ray))
        {
            myCoverModule.CancelCurrentCoverAction();

            switch (ray.collider.gameObject.layer)
            {
                case 6: //Ground Layer
                    PlayerMove(ray.point, false);
                    break;
                case 7: //Enemy Layer
                    SetTargetEnemy(ray.collider.GetComponent<Enemy>());
                    break;
                case 10: //Indicator Layer
                    PlayerMove(ray.point, true);
                    break;
                case 11: // CoverPoint Layer
                    if (ray.transform.TryGetComponent(out CoverPoint coverPoint))
                        myCoverModule.StartMoveToCover(coverPoint);
                    break;

                default:
                    break;
            }
        }
    }

    public bool RaycastAtMouseLocation(out RaycastHit ray)
    {
        return Physics.Raycast(myCamera.ScreenPointToRay(inputReader.MousePosition), out ray, 100f, rightClickInteractableLayer);
    }

    public bool RaycastAtMouseLocation()
    {
        RaycastHit ray;
        if (Physics.Raycast(myCamera.ScreenPointToRay(inputReader.MousePosition), out ray, 100f, enemyLayer))
        {
            SetTargetEnemy(ray.collider.GetComponent<Enemy>());
            return true;
        }
        return false;
    }

    public void HandleLeftClickInput()
    {
        if(mySkillModule.isTargetting)
        {
            if(RaycastAtMouseLocation())
            {
                mySkillModule.ActivateSelectedSkill();
            }
            else
            {
                Debug.Log("Skill 사용 실패 : Enemy가 아님.");
            }
        }
    }

    public void HandlePlayerSkillInput(SkillNumber index)
    {
        mySkillModule.ActivateTargettingMode(index);
    }

    private void PlayerMove(Vector3 pos, bool isRun)
    {
        if (isRun) myMovementModule.PlayerRun(pos);
        else myMovementModule.PlayerWalk(pos);

        myEffectModule.PlayMoveIndicatorEffect(pos);
    }
    #endregion

    #region 전투관련 함수
    private void SetTargetEnemy(Enemy castedEnemy)
    {
        if (!castedEnemy) return;
        SelectedEnemy = castedEnemy;
    }

    private void CancelEnemySelect()
    {
        SelectedEnemy = null;
    }

    private void ChaseEnemy()
    {
        if (myCombatModule.IsEnemyInWeaponSight(SelectedEnemy))
        {
            myMovementModule.PlayerMoveStop();
        }
        else
        {
            myMovementModule.PlayerWalk(SelectedEnemy.transform.position);
        }
    }

    private void NormalAttackEnemy()
    {
        if (myCombatModule.IsEnemyInWeaponSight(SelectedEnemy) && myMovementModule.PlayerRotateToward(SelectedEnemy.transform.position))
        {
            myPlayerAnimator.SetShoot(true);

            if (myCombatModule.CanFire())
            {
                myCombatModule.NormalAttackEnemy(SelectedEnemy);
                myEffectModule.PlayFiringEffect(SelectedEnemy.transform.position);                
            }
        }
        else
        {
            CancelNormalAttack();
        }
    }

    private void CancelNormalAttack()
    {
        myPlayerAnimator.SetShoot(true);
    }

    #endregion
}
