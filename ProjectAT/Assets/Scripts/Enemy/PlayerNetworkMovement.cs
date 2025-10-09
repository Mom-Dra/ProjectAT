using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]
public class PlayerNetworkMovement : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 6f;

    private Rigidbody rigid;
    private MomDra.Input.PlayerInput playerInput;

    private Vector2 serverCurrentInput;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        playerInput = GetComponent<MomDra.Input.PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (TryGetComponent(out UnityEngine.InputSystem.PlayerInput playerInput))
                playerInput.enabled = false;

            if (TryGetComponent(out MomDra.Input.PlayerInput input))
                input.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            SendInputServerRpc(playerInput.Move);
        }
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            serverCurrentInput.Normalize();

            Vector3 displacement = new Vector3(serverCurrentInput.x, 0f, serverCurrentInput.y) * moveSpeed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + displacement);
        }
    }

    [Rpc(SendTo.Server)]
    private void SendInputServerRpc(Vector2 input)
    {
        serverCurrentInput = input;
    }
}