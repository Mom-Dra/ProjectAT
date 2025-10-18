using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerControls;

[CreateAssetMenu(fileName ="New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<PlayerInputType> InputEvent;

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

    public void OnRightClicked(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InputEvent?.Invoke(PlayerInputType.RightClick);
        }
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        MousePosition = context.ReadValue<Vector2>();
    }

    public void OnDesignatedFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InputEvent?.Invoke(PlayerInputType.DesignatedFireKey);
        }
    }

    public void OnLeftClicked(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InputEvent?.Invoke(PlayerInputType.LeftClick);
        }
    }
}
