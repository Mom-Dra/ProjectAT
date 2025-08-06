using System.Globalization;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] InputReader inputReader;
    [SerializeField] NavMeshAgent myAgent;

    [Header("factors")]
    [SerializeField] private float stopSpeed;


    private void Awake()
    {
        myAgent = GetComponent<NavMeshAgent>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            inputReader.MoveEvent += HandleMovement;
        }
        
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.MoveEvent -= HandleMovement;
        }
    }

    private void HandleMovement(bool triggered)
    {
        if (!triggered) return;

        Vector3 position = GetMovePosition();
        if (position != Vector3.zero)
            PlayerMoveServerRpc(position);
    }

    private Vector3 GetMovePosition()
    {
        RaycastHit ray;
        Physics.Raycast(Camera.main.ScreenPointToRay((Vector3)inputReader.MousePosition), out ray, LayerMask.GetMask("Ground"));
        return ray.point;
    }


    [Rpc(SendTo.Server)]
    private void PlayerMoveServerRpc(Vector3 pos)
    {
        PlayerMoveClientRpc(pos);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerMoveClientRpc(Vector3 pos)
    {
        Move(pos);
    }

    private void Move(Vector3 pos)
    {
        myAgent.velocity *= Mathf.Min(Mathf.Cos(Vector3.Angle(transform.forward, myAgent.destination - transform.forward)), 0.0f);
        myAgent.SetDestination(pos);
    }
}
