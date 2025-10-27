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

    //skills
    private PlayerSkillStrategyMap mySkillMap = new PlayerSkillStrategyMap();
    public PlayerSkillStrategyMap MySkillMap => mySkillMap;

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
        mySkillMap.InitiateSkillStrategy(this);
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

        MyAgent.angularSpeed = 360f;
    }
    #endregion
    #region 상태머신관련

    private void HandleInput(PlayerInputType type)
    {
        myStateMachine.HandleInput(type);
    }

    public void ChangeState(PlayerStateMachine.StateId nextState)
    {
        myStateMachine.ChangeState(nextState);
    }

    /// <summary>
    /// 마우스 레이케스트에 따라 상태를 반환하는 함수.
    /// </summary>
    /// <returns>레이케스트에 의해 감지된 물체에 따른 다음 상태 반환. 적을 선택하면 Chase, 땅을 선택하면 Run 또는 Walk를 반환. </returns>
    public PlayerStateMachine.StateId CalCulateNextStateByMouseRaycast()
    {
        RaycastHit casted = MouseRaycast();

        if (casted.collider != null)
        {
            if (((1 << casted.collider.gameObject.layer) & enemyLayer.value) > 0)
            {
                SetTargetEnemy(casted.collider.GetComponent<Enemy>());
                return PlayerStateMachine.StateId.Chase;
            }
            else if (((1 << casted.collider.gameObject.layer) & groundLayer.value) > 0) 
            {
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
        return PlayerStateMachine.StateId.None;
    }
    #endregion

    #region 이동관련함수
    public Vector3 GetMouseWorldPosition()
    {
        RaycastHit ray;
        if (Physics.Raycast(cameraController.MyCamera.ScreenPointToRay((Vector3)inputReader.MousePosition), out ray,100f, groundLayer.value))
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

    public void PlayerWalk(Vector3 pos)
    {
        PlayerMove(pos, MyStatus.WalkSpeed);
    }

    private void PlayerMove(float speed)
    {
        PlayerMove(GetMouseWorldPosition(), speed);
    }

    private void PlayerMove(Vector3 pos, float speed)
    {
        ChangePlayerSpeed(speed);
        MovePosition(pos);
        MyAnim.SetBool("isWalking", true);
    }

    private void ChangePlayerSpeed(float speed)
    {
        MyAgent.speed = MyStatus.CurrentSpeed = speed;
    }

    //NavMeshAgent 목표설정함수
    public void MovePosition(Vector3 pos)
    {
        Vector3 moveVec = (pos - transform.position).normalized;

        //MyAgent.velocity = transform.forward * MyAgent.speed; //이 코드가 무빙에 버그를 일으킴
        MyAgent.destination = pos;
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

    /// <summary>
    /// 부드러운 회전을 위한 함수. Update()같은 주기적 호출이 일어나는 곳에서 호출해야하며 충분히 회전했으면 true를 반환함.
    /// </summary>
    /// <param name="target">바라볼 대상의 position</param>
    /// <returns></returns>
    public bool SmoothRotateToTarget(Vector3 target)
    {
        Vector3 PlayerToTarget = (target - transform.position);
        Vector3 PlayerForward = transform.forward;
        PlayerToTarget.y = PlayerForward.y;
        if (Vector3.Angle(PlayerForward, PlayerToTarget) >= 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(PlayerToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 부드러운 회전을 위한 함수. Update()같은 주기적 호출이 일어나는 곳에서 호출해야하며 충분히 회전했으면 true를 반환함.
    /// </summary>
    /// <param></param>
    /// <returns></returns>
    public bool SmoothRotateToTarget()
    {
        if(SelectedEnemy == null) return false;
        return SmoothRotateToTarget(SelectedEnemy.transform.position);
    }
    #endregion
    #region 공격관련함수

    public Enemy FindNearestEnemy()
    {
        if (Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 0.5f, MyWeapon.Radius, detectedCollider, enemyLayer.value) != 0)
        {
            detectedCollider.OrderBy(c => (c.transform.position - transform.position).sqrMagnitude);
            foreach(var scanedCollider in detectedCollider)
            {
                if (scanedCollider == null) break;
                Enemy enemy = scanedCollider.GetComponent<Enemy>();
                if (IsInAttackRange(enemy))
                {
                    return enemy;
                }
            }
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
        
        Vector3 dir = (target.transform.position - transform.position);

        if ((dir.sqrMagnitude <= MyWeapon.Radius * MyWeapon.Radius))
        {
            Physics.Raycast(transform.position, dir, out RaycastHit hit, MyWeapon.Radius);
            return hit.collider != null && hit.collider.gameObject == target.gameObject;
        }

        return false;
    }

    private void AttackEnemy(Enemy target, int damage)
    {
        if (MyStatus.CanFire())
        {
            //target.GetComponent<Enemy>().TakeDamage(damage); //enemy�� ��ġ�Ǹ� �ּ������ϱ�
            MyEffectModule.PlayFiringEffect(target.transform.position);
            MyStatus.ResetAttackCoolTime();
            Debug.Log("공격");
        }
    }
    #endregion
    #region 스킬관련함수
    public void DesignateFireToEnemy()
    {
        DesignateFireToEnemy(SelectedEnemy);
    }
    public void DesignateFireToEnemy(Enemy enemy)
    {
        AttackEnemy(enemy, MyWeapon.Damage * 2); //나중에 Damage 대신 스킬 데미지를 적용
    }

    #endregion
}
