using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public enum PlayerInputType : ushort { LeftClick, RightClick}

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
    private PlayerStateMachine myStateMachine;

    //utils
    private readonly Collider[] detectedCollider = new Collider[32];

    private void Awake()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        MyStatus = GetComponent<EntityStatus>();
        myStateMachine = GetComponent<PlayerStateMachine>();
        MyWeapon = transform.GetChild(1).GetComponent<WeaponStatus>();
        MyAnim = transform.GetChild(0).GetComponent<Animator>();
        MyEffectModule = GetComponent<EffectModule>();
    }
    private void Start()
    {
        MyAgent.speed = MyStatus.WalkSpeed.Value;

    }

    public void InitiateSettings()
    {
        cameraController = FindAnyObjectByType<CameraController>();
        //cameraController.SetCameraTarget(transform);
        LinkInputEventsAll();
    }

    public void LinkInputEventsAll()
    {
        inputReader.ClickEvent += HandleClickInput;
    }

    public void UnLinkInputEventsAll()
    {
        inputReader.ClickEvent -= HandleClickInput;
    }

    private void HandleClickInput(PlayerInputType type)
    {
        myStateMachine.HandleClickInput(type);
    }

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

    //NavMeshAgent 목표 설정 함수
    public void MovePosition(Vector3 pos)
    {
        Vector3 moveVec = (pos - transform.position).normalized;

        MyAgent.velocity = moveVec * MyAgent.speed;
        MyAgent.SetDestination(pos);
    }

    public EntityController FindNearEnemy()
    {
        if (Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 0.5f, MyWeapon.Radius, detectedCollider, LayerMask.GetMask("Enemy")) != 0)
        {
            foreach(var coll in detectedCollider)
            {
                if (coll)
                {
                    EntityController enemyController = coll.transform.GetComponent<EntityController>();
                    if(enemyController.IsAlive()) return enemyController;
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

    public void AttackEnemy(EntityController target)
    {
        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0; //보정

        if (Vector3.Angle(transform.forward, toTarget) > 10.0f)
        {
            Debug.Log(Vector3.Angle(transform.forward, toTarget));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget, Vector3.up), 20.0f);
        }
        else if (MyStatus.CanFire())
        {
            target.GetComponent<EntityStatus>().TakeDamage(MyWeapon.Damage);
            MyStatus.ResetAttackCoolTime();
            myStateMachine.AttackEnemyClientRpc(target);
        }
    }

    public bool IsClickSamePosition()
    {
        Vector3 pos = GetMouseWorldPosition();
        Vector3 dest = MyAgent.destination;

        return pos.x == dest.x && pos.z == dest.z && (Mathf.Abs(pos.y - dest.y) <= 0.1);
    }
}
