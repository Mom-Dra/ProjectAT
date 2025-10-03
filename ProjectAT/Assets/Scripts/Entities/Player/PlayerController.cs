using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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

    //for battel
    public Enemy SelectedEnemy { get; private set; }
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;

    #region 유니티 이벤트
    private void Awake()
    {
        InitComponents();
    }

    private void OnEnable()
    {
        LinkInputEventsAll();
        ChangePlayerSpeed(MyStatus.WalkSpeed);
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

    private void InitComponents()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        MyStatus = GetComponent<EntityStatus>();
        MyAnim = transform.GetChild(0).GetComponent<Animator>();
        MyWeapon = transform.GetChild(1).GetComponent<WeaponStatus>();
        MyEffectModule = transform.GetChild(2).GetComponent<EffectModule>();
        cameraController = FindFirstObjectByType<CameraController>();

        myStateMachine = new PlayerStateMachine(this);
    }
    #endregion
    #region 상태머신 관련

    private void HandleInput(PlayerInputType type)
    {
        myStateMachine.HandleInput(type);
    }

    public PlayerStateMachine.StateId CalCulateNextStateByMouseRaycast()
    {
        RaycastHit casted = MouseRaycast();

        if (casted.collider != null)
        {
            if (((1 << casted.collider.gameObject.layer) & enemyLayer.value) > 0)
            {
                Debug.Log("Raycast Enemy");
                SetTargetEnemy(casted.collider.GetComponent<Enemy>());
                return PlayerStateMachine.StateId.Chase;
            }
            else if (((1 << casted.collider.gameObject.layer) & groundLayer.value) > 0) 
            {
                Debug.Log("Raycast Ground");
                if (IsClickSameDestination(casted.point))
                {
                    return PlayerStateMachine.StateId.Run;
                }
                else
                {
                    return PlayerStateMachine.StateId.Walk;
                }
            }
        }
        Debug.Log("Raycast Failed");
        return PlayerStateMachine.StateId.None;
    }
    #endregion

    #region 이동 관련 함수
    public Vector3 GetMouseWorldPosition()
    {
        RaycastHit ray;
        if (Physics.Raycast(cameraController.MyCamera.ScreenPointToRay((Vector3)inputReader.MousePosition), out ray, groundLayer))
        {
            return ray.point;
        }
        else
            return Vector3.zero;
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
    public void MovePosition(Vector3 pos)
    {
        Vector3 moveVec = (pos - transform.position).normalized;

        MyAgent.velocity = moveVec * MyAgent.speed;
        MyAgent.SetDestination(pos);
    }
    public bool IsClickSameDestination(Vector3 dest)
    {
        return (MyAgent.destination - dest).sqrMagnitude <= 0.01f;
    }

    public void StopMoving()
    {
        MyAgent.ResetPath();
        MyAgent.velocity = Vector3.zero;
        ChangePlayerSpeed(MyStatus.WalkSpeed);
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
    #endregion
    #region 전투 관련 함수

    public Enemy FindNearestEnemy()
    {
        if (Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 0.5f, MyWeapon.Radius, detectedCollider, enemyLayer) != 0)
        {
            detectedCollider.OrderBy(c => (c.transform.position - transform.position).sqrMagnitude);
            return detectedCollider[0].GetComponent<Enemy>();
        }
        return null;
    }

    public void SetTargetEnemy(Enemy enemy)
    {
        SelectedEnemy = enemy;
    }

    public RaycastHit MouseRaycast()
    {
        RaycastHit hit;
        Physics.Raycast(
            cameraController.MyCamera.ScreenPointToRay((Vector3)inputReader.MousePosition), out hit, Mathf.Infinity, (enemyLayer | groundLayer));
        return hit;
    }

    /*public RaycastHit MouseRaycast(LayerMask layer)
    {
        RaycastHit hit;
        Physics.Raycast(
            cameraController.MyCamera.ScreenPointToRay((Vector3)inputReader.MousePosition), out hit, Mathf.Infinity, layer);
        return hit;
    }*/

    public void NormalAttackEnemy(Enemy target)
    {
        AttackEnemy(target, MyWeapon.Damage);
    }

    public bool IsInAttackRange(Enemy target)
    {
        if (target == null) return false;

        return (target.transform.position - transform.position).sqrMagnitude <= MyWeapon.Radius * MyWeapon.Radius;
    }

    private void AttackEnemy(Enemy target, int damage)
    {
        if (MyStatus.CanFire())
        {
            //target.GetComponent<Enemy>().TakeDamage(damage); //enemy가 패치되면 주석해제하기
            MyEffectModule.PlayFiringEffect();
            MyStatus.ResetAttackCoolTime();
            Debug.Log("공격");
        }
    }
    #endregion
}
