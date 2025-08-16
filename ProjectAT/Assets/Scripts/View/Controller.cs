using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    private Rigidbody rigid;
    private Camera viewCamera;
    private Vector3 velocity;

    [SerializeField]
    private int moveSpeed = 6;

    private Vector2 moveInput;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        viewCamera = Camera.main;
    }

    private void Update()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, viewCamera.transform.position.y));

        Debug.DrawRay(transform.position, transform.forward, Color.red);
        transform.LookAt(mousePos + Vector3.up * transform.position.y);
        velocity = new Vector3(moveInput.x, 0, moveInput.y).normalized * moveSpeed;
    }

    private void FixedUpdate()
    {
        rigid.MovePosition(rigid.position + velocity * Time.fixedDeltaTime);
    }

    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        moveInput = newMoveDirection;
    }
}
