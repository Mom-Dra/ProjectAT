using EPOOutline.Demo;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public enum PlayerInputType : ushort { LeftClick, RightClick, DesignatedFireKey }

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] public NavMeshAgent MyAgent { get; private set; }
    [SerializeField] public EntityStatus MyStatus { get; private set; }
    [SerializeField] public WeaponStatus MyWeapon { get; private set; }
    [SerializeField] public Animator MyAnim { get; private set; }

    [SerializeField] public EffectModule MyEffectModule { get; private set; }
    [SerializeField] private CameraController cameraController;

    //State Machines
    private PlayerStateMachine myStateMachine;
    [SerializeField] private float tickRate = 0.2f;
    private float lastUpdatedTime = 0f;

    //utils
    private readonly Collider[] detectedCollider = new Collider[8];

    #region 유니티 이벤트
    private void Awake()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        MyStatus = GetComponent<EntityStatus>();
        MyWeapon = transform.GetChild(1).GetComponent<WeaponStatus>();
        MyAnim = transform.GetChild(0).GetComponent<Animator>();
        MyEffectModule = GetComponent<EffectModule>();
        cameraController = FindFirstObjectByType<CameraController>();
        

        myStateMachine = new PlayerStateMachine(this);
    }

    private void OnEnable()
    {
        ChangePlayerSpeed(MyStatus.WalkSpeed);
        LinkInputEventsAll();
    }

    private void Update()
    {
        if (Time.time - lastUpdatedTime <= tickRate)
        {
            myStateMachine.OnUpdate();
            lastUpdatedTime = Time.time;
        }
    }

    private void OnDisable()
    {
        UnLinkInputEventsAll();
    }
    #endregion

    #region 초기화
    private void LinkInputEventsAll()
    {
        inputReader.InputEvent += HandleInput;
    }

    private void UnLinkInputEventsAll()
    {
        inputReader.InputEvent -= HandleInput;
    }
    #endregion
    #region 상태머신 관련


    private void HandleInput(PlayerInputType type)
    {
        myStateMachine.HandleInput(type);
    }
    #endregion

    #region 필요 기능 함수
    public Vector3 GetMouseWorldPosition()
    {
        RaycastHit ray;
        if (Physics.Raycast(cameraController.MyCamera.ScreenPointToRay((Vector3)inputReader.MousePosition), out ray, LayerMask.GetMask("Ground")))
        {
            return ray.point;
        }
        else
            return Vector3.zero;
    }

    public void PlayerIdle()
    {
        StopMoving();
        ChangePlayerSpeed(MyStatus.WalkSpeed);
    }

    public void PlayerWalk()
    {
        PlayerMove(MyStatus.WalkSpeed);
    }
    public void PlayerRun()
    {
        PlayerMove(MyStatus.RunSpeed);
    }

    private void PlayerMove(float speed)
    {
        ChangePlayerSpeed(speed);
        MovePosition(GetMouseWorldPosition());
        MyAnim.SetBool("isWalking", true);
    }

    private void ChangePlayerSpeed(float speed)
    {
        MyAgent.speed = MyStatus.CurrentSpeed = speed;
    }

    //NavMeshAgent 목표 설정 함수
    private void MovePosition(Vector3 pos)
    {
        Vector3 moveVec = (pos - transform.position).normalized;

        MyAgent.velocity = moveVec * MyAgent.speed;
        MyAgent.SetDestination(pos);
    }

    public void StopMoving()
    {
        MyAgent.ResetPath();
        MyAnim.SetBool("isWalking", false);
    }

    public bool IsArrivedDestination()
    {
        if (!MyAgent.pathPending)
        {
            if (MyAgent.remainingDistance <= MyAgent.stoppingDistance)
            {
                return true;
            }
        }
        return false;
    }
    public Enemy FindNearEnemy()
    {
        if (Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 0.5f, MyWeapon.Radius, detectedCollider, LayerMask.GetMask("Enemy")) != 0)
        {

            foreach (var coll in detectedCollider)
            {
                if (coll)
                {
                    Enemy enemyController = coll.transform.GetComponent<Enemy>();
                    //if(enemyController.IsAlive()) return enemyController;
                    return enemyController;
                }
            }
        }
        return null;
    }
    public Enemy RaycastEnemy()
    {
        RaycastHit hit;
        return Physics.Raycast(cameraController.MyCamera.ScreenPointToRay((Vector3)inputReader.MousePosition), out hit, LayerMask.GetMask("Enemy"))? hit.transform.GetComponent<Enemy>() : null;
    }

    public void AttackEnemy(Enemy target, int damage)
    {
        if (MyStatus.CanFire())
        {
            target.GetComponent<Enemy>().TakeDamage(damage);
            MyStatus.ResetAttackCoolTime();
        }
    }

    public void LookAtTarget(Vector3 toTarget)
    {
        //Debug.Log(Vector3.Angle(transform.forward, toTarget));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget, Vector3.up), 20.0f);
    }

    public bool IsClickSameDestination()
    {
        Vector3 pos = GetMouseWorldPosition();
        Vector3 dest = MyAgent.destination;

        //마우스 클릭 인디케이터 프리펩을 만들면 그 인디케이터를 raycast 해서 같은 지점을 확인하는 알고리즘 써도 될듯.

        return (pos - dest).sqrMagnitude <= 0.01f;
    }
    #endregion
}
