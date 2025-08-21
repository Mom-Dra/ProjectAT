using UnityEngine;
using UnityEngine.AI;

public enum PlayerInputType : ushort { LeftClick, RightClick}

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] public NavMeshAgent MyAgent { get; private set; }
    [SerializeField] public EntityStatus MyStatus { get; private set; }
    [SerializeField] public WeaponStatus MyWeapon { get; private set; }

    [SerializeField] private CameraController cameraController;
    private PlayerStateMachine myStateMachine;
    private readonly Collider[] detectedCollider = new Collider[32];
    private void Awake()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        MyStatus = GetComponent<EntityStatus>();
        myStateMachine = GetComponent<PlayerStateMachine>();
        MyWeapon = transform.GetChild(1).GetComponent<WeaponStatus>();
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

    public void RotateTo(Transform targetTf)
    {
        Quaternion newRotation = Quaternion.LookRotation(targetTf.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, 20.0f);
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

    public void AttackEnemy(EntityController target)
    {
        Debug.Log($"{myStateMachine.myName} : Attack Enemy");
        target.GetComponent<EntityStatus>().TakeDamage(MyWeapon.Damage);
        MyStatus.ResetAttackCoolTime();
    }

    public bool IsClickSamePosition()
    {
        Vector3 pos = GetMouseWorldPosition();
        Vector3 dest = MyAgent.destination;

        return pos.x == dest.x && pos.z == dest.z && (Mathf.Abs(pos.y - dest.y) <= 0.1);
    }
}
