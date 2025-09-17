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

    public void OnClicked(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InputEvent?.Invoke(PlayerInputType.LeftClick);
        }
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        MousePosition = context.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext context) //삭제해야할듯
    {
        if (context.performed)
        {
            Debug.Log("DobuleClicked");
        }
    }

    public void OnDesignatedFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InputEvent?.Invoke(PlayerInputType.DesignatedFireKey);
        }
    }
}
