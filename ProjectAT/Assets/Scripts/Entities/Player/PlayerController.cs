using EPOOutline.Demo;
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
    [SerializeField] private EffectModule myEffectModule;
    [SerializeField] private Camera myCamera;

    [Header("Enemy")]
    public Enemy SelectedEnemy;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Params")]
    [SerializeField] private float TickRate = 0.2f;
    private float LastTickTime = 0f;
    private SkillNumber lastSkillInput;

    private CoverObject currCoverObject;

    #region 초기화
    private void InitiateComponents()
    {
        myMovementModule = GetComponent<PlayerMovementModule>();
        myPlayerAnimator = GetComponent<PlayerAnimator>();
        myEffectModule = GetComponent<EffectModule>();
        myCombatModule = GetComponent<PlayerCombatModule>();
        mySkillModule = GetComponent<PlayerSkillModule>();
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
            LastTickTime = Time.time;
        }

        myPlayerAnimator.SetSpeed(myMovementModule.GetVelocity());

        RayToCover();
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
                default:
                    break;
            }
        }
    }

    public bool RaycastAtMouseLocation(out RaycastHit ray)
    {
        return Physics.Raycast(myCamera.ScreenPointToRay(inputReader.MousePosition), out ray, 100f);
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


    public void PlayerMove(Vector3 pos, bool isRun)
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

    private void RayToCover()
    {
        Ray ray = Camera.main.ScreenPointToRay(inputReader.MousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        // 2. 광선 발사 (Cover 레이어만 충돌 체크)
        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("CoverPoint")))
        {
            if (hit.transform.TryGetComponent(out CoverObject coverObject))
            {
                if(currCoverObject != coverObject)
                {
                    currCoverObject = coverObject;
                    coverObject.ShowCoverPoint();
                }
            }
        }
        else
        {
            if(currCoverObject is not null)
            {
                currCoverObject.HideCoverPoint();
                currCoverObject = null;
            }
        }

        //{
        //GameObject hitObject = hit.collider.gameObject;

        // 3. 최적화: 새로운 오브젝트일 때만 로직 실행 (상태 변화 감지)
        //if (currentHoveredCover != hitObject)
        //{
        //    // 이전 엄폐물의 인디케이터 끄기
        //    if (currentHoveredCover != null)
        //    {
        //        HideIndicators(currentHoveredCover);
        //    }

        //    // 새로운 엄폐물 등록 및 인디케이터 켜기
        //    currentHoveredCover = hitObject;
        //    ShowIndicators(currentHoveredCover);
        //}
        //}
        //else
        {
            // 4. 마우스가 허공이나 땅을 가리킬 때 (엄폐물 벗어남)
            //if (currentHoveredCover != null)
            //{
            //    HideIndicators(currentHoveredCover);
            //    currentHoveredCover = null;
            //}
        }
    }

    #endregion
}
