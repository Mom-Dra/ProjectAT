using EPOOutline.Demo;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum PlayerInputType : ushort { LeftClick, RightClick, DesignatedFireKey }

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerMovementModule myMovementModule;
    [SerializeField] private PlayerAnimationModule myAnimationModule;
    [SerializeField] private PlayerCombatModule myCombatModule;
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


    #region 초기화
    private void InitiateComponents()
    {
        myMovementModule = GetComponent<PlayerMovementModule>();
        myAnimationModule = GetComponent<PlayerAnimationModule>();
        myEffectModule = GetComponent<EffectModule>();
        myCombatModule = GetComponent<PlayerCombatModule>();
    }
    private void LinkInputEventsAll()
    {
        //inputReader.InputEvent += HandleInput;
        inputReader.MouseRightClickEvent += RaycastAtMouseCursorLocation;
    }

    private void UnLinkInputEventsAll()
    {
        //inputReader.InputEvent -= HandleInput;
        inputReader.MouseRightClickEvent -= RaycastAtMouseCursorLocation;
    }
    #endregion

    #region 유니티 이벤트
    private void Awake()
    {
        InitiateComponents();
        myCamera = Camera.main;
        LastTickTime = Time.time;
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
        if(Time.time - LastTickTime < TickRate)
        {
            myAnimationModule.SetRunningAnimation(myMovementModule.IsAgentMoving());
            if (SelectedEnemy != null)
            {
                ChaseEnemy();
                NormalAttackEnemy();
            }

            LastTickTime = Time.time;
        }
    }

    #endregion
    #region 입력 관련 함수
    public void RaycastAtMouseCursorLocation()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        CancelEnemySelect();

        RaycastHit ray;
        if (Physics.Raycast(myCamera.ScreenPointToRay(inputReader.MousePosition), out ray, 100f))
        {
            switch (ray.collider.gameObject.layer)
            {
                case 6: //Ground Layer
                    PlayerMove(ray.point, false);
                    break;
                case 7: //Enemy Layer
                    SelectEnemy(ray.collider.GetComponent<Enemy>());
                    break;
                case 10: //Indicator Layer
                    PlayerMove(ray.point, true);
                    break;
                default:
                    break;
            }
        }

    }
    private void PlayerMove(Vector3 pos, bool isRun)
    {
        if (isRun) myMovementModule.PlayerRun(pos);
        else myMovementModule.PlayerWalk(pos);
        myEffectModule.PlayMoveIndicatorEffect(pos);
    }

    #region 전투관련 함수
    private void SelectEnemy(Enemy castedEnemy)
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
        //MEMO : 추격이나 이런 상태를 공격모듈의 상태패턴으로..?ㅋㅋ
        if (myCombatModule.IsEnemyInRange(SelectedEnemy))
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
        if (myCombatModule.IsEnemyInRange(SelectedEnemy))
        {
            if(myMovementModule.PlayerRotateToward(SelectedEnemy.transform.position) && myCombatModule.CanFire())
            {
                myCombatModule.NormalAttackEnemy(SelectedEnemy);
                myAnimationModule.PlayFiringAnimation();
                myEffectModule.PlayFiringEffect(SelectedEnemy.transform.position);
            }
        }
    }
    #endregion


    /*public Vector3 GetMouseWorldPosition()
    {
        RaycastHit ray;
        if (Physics.Raycast(myCamera.ScreenPointToRay(inputReader.MousePosition), out ray, 100f, groundLayer.value))
        {
            return ray.point;
        }
        else
            return Vector3.zero;
    }*/
    #endregion


}
