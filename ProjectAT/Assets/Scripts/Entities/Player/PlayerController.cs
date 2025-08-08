using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] public NavMeshAgent MyAgent { get; private set; }
    [SerializeField] public EntityStatus MyStatus { get; private set; }
    [SerializeField] private CameraController cameraController;
    
    private PlayerStateMachine myStateMachine;

    private void Awake()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        MyStatus = GetComponent<EntityStatus>();
        myStateMachine = new PlayerStateMachine(this);
    }

    private void Start()
    {
        MyAgent.speed = MyStatus.WalkSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cameraController = FindAnyObjectByType<CameraController>();
            cameraController.SetCameraTarget(transform);
            LinkInputEvents_All();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            UnLinkInputEvents_All();
        }
    }

    private void LinkInputEvents_All()
    {
        inputReader.ClickEvent += HandleClickInput;
    }

    private void UnLinkInputEvents_All()
    {
        inputReader.ClickEvent -= HandleClickInput;
    }

    private void HandleClickInput()
    {
        myStateMachine.HandleClickInput();
    }

    public void PlayerMove()
    {
        Vector3 nextPos = GetDestinationWolrdPosition(inputReader.MousePosition);
        if (nextPos != Vector3.zero) PlayerMoveServerRpc(nextPos);
    }

    private Vector3 GetDestinationWolrdPosition(Vector2 mousePosition)
    {
        RaycastHit ray;
        if (Physics.Raycast(cameraController.MyCamera.ScreenPointToRay((Vector3)mousePosition), out ray, LayerMask.GetMask("Ground")))
            return ray.point;
        return Vector3.zero;
    }

    [Rpc(SendTo.Server)]
    private void PlayerMoveServerRpc(Vector3 nextPos)
    {
        PlayerMoveClientRpc(nextPos);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerMoveClientRpc(Vector3 nextPos)
    {
        MovePosition(nextPos);
    }

    private void MovePosition(Vector3 pos)
    {
        Vector3 moveVec = (pos - transform.position).normalized;

        Debug.Log(MyStatus.WalkSpeed);
        MyAgent.velocity = moveVec * MyStatus.WalkSpeed;
        MyAgent.SetDestination(pos);
    }

    private void Update()
    {
        myStateMachine.OnUpdate();
    }
}
