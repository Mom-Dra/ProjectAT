using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerControls;

[CreateAssetMenu(fileName ="New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<bool> MoveEvent;
    private PlayerControls controls;
    public Vector2 MousePosition { get; private set; }


    private void OnEnable()
    {
        if(controls == null)
        {
            controls = new PlayerControls();
            controls.Player.SetCallbacks(this);
        }

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MoveEvent?.Invoke(true);
        }
        else
        {
            MoveEvent?.Invoke(false);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        MousePosition = context.ReadValue<Vector2>();
    }
}
