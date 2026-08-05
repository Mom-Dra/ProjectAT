using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static PlayerControls;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions, IMenuActions
{
    public event Action<SkillNumber> SkillInputEvent;
    public event Action MouseRightClickEvent;
    public event Action MouseLeftClickEvent;

    public event Action onMouseWheelClicked;
    public event Action OnInteractableObjectDropEvent;
    public event Action OnReloadEvent;
    public event Action OnPauseInputEvent;

    private PlayerControls controls;
    public Vector2 MousePosition { get; private set; }
    public Vector2 MouseDelta { get; private set; }
    public Vector2 MouseWheelDelta { get; private set; }

    public bool IsWheelClickHolding { get; private set; }

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new PlayerControls();
            controls.Player.SetCallbacks(this);
            controls.Menu.SetCallbacks(this);
        }
        controls.Player.Enable();
        controls.Menu.Enable();
    }

    private void OnDisable()
    {
        if(controls == null) return;

        controls.Player.Disable();
        controls.Menu.Disable();
    }

    #region Player Input Action Callbacks
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

    public void OnGrenadeThrow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SkillInputEvent?.Invoke(SkillNumber.Grenade);
        }
    }

    public void OnUseBandage(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SkillInputEvent?.Invoke(SkillNumber.UseBandage);
        }
    }

    public void OnMouseWheelButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onMouseWheelClicked?.Invoke();
            IsWheelClickHolding = true;
        }
        else if (context.canceled)
        {
            IsWheelClickHolding = false;
        }
    }

    public void OnHaha(InputAction.CallbackContext context)
    {
        if (context.performed) IsWheelClickHolding = true;
        else if (context.canceled) IsWheelClickHolding = false;
    }

    public void OnMouseDelta(InputAction.CallbackContext context)
    {
        MouseDelta = context.ReadValue<Vector2>();
    }

    public void OnMouseWheelDelta(InputAction.CallbackContext context)
    {
        MouseWheelDelta = context.ReadValue<Vector2>();
    }

    public void OnInteractableObjectDrop(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            OnInteractableObjectDropEvent?.Invoke();
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            OnReloadEvent?.Invoke();
        }
    }

    public void OnSkillTwo(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SkillInputEvent?.Invoke(SkillNumber.MainSkillTwo);
        }
    }
    #endregion =================================
    #region Menu Input Action Callbacks
    /// <summary>
    /// Pause 메뉴가 열렸을 때 게임 플레이 입력만 차단한다. Menu 액션 맵은 계속 활성화되어 ESC를 다시 사용할 수 있다.
    /// </summary>
    public void SetGameplayInputEnabled(bool enabled)
    {
        if (controls == null)
        {
            return;
        }

        if (enabled)
        {
            controls.Player.Enable();
        }
        else
        {
            controls.Player.Disable();
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPauseInputEvent?.Invoke();
        }
    }
    #endregion ================================
}
