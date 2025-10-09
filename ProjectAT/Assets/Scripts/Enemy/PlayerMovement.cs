using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 6f;

    private Rigidbody rigid;
    private MomDra.Input.PlayerInput playerInput;

    private Vector2 currentInput;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        playerInput = GetComponent<MomDra.Input.PlayerInput>();
    }

    private void FixedUpdate()
    {
        currentInput = playerInput.Move;
        currentInput.Normalize();

        Vector3 displacement = new Vector3(currentInput.x, 0f, currentInput.y) * moveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + displacement);
    }
}
