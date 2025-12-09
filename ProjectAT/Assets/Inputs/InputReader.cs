using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static PlayerControls;

[CreateAssetMenu(fileName ="New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<SkillNumber> SkillInputEvent;
    public event Action MouseRightClickEvent;
    public event Action MouseLeftClickEvent;

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
            MouseRightClickEvent?.Invoke();
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
            SkillInputEvent?.Invoke(SkillNumber.DesignatedFire);
        }
    }

    public void OnLeftClicked(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MouseLeftClickEvent?.Invoke();
        }
    }

    public void OnSkillOne(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SkillInputEvent?.Invoke(SkillNumber.MainSkillOne);
        }
    }
}
