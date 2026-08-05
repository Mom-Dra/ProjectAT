using System;
using UnityEngine;

public class InputManager
{
    public event Action<SkillNumber> onSkillInputed;
    public event Action onMouseRightClicked;
    public event Action onMouseLeftClicked;
    public event Action OnInteractableObjectDropInput;
    public event Action OnReloadEvent;
    public event Action OnPauseInputEvent;

    private InputReader inputReader;

    public Vector2 MousePosition => inputReader.MousePosition;
    public Vector2 MouseDelta => inputReader.MouseDelta;
    public Vector2 MouseWheelDelta => inputReader.MouseWheelDelta;
    public bool IsWheelClickHolding => inputReader.IsWheelClickHolding;
    

    public InputManager(InputReader inputReader)
    {
        this.inputReader = inputReader;

        inputReader.SkillInputEvent += SkillInputed;
        inputReader.MouseRightClickEvent += RightClicked;
        inputReader.MouseLeftClickEvent += LeftClicked;
        inputReader.OnInteractableObjectDropEvent += OnInteractableObjectDrop;
        inputReader.OnReloadEvent += OnReload;
        inputReader.OnPauseInputEvent += PauseInput;
    }

    ~InputManager()
    {
        inputReader.SkillInputEvent -= SkillInputed;
        inputReader.MouseRightClickEvent -= RightClicked;
        inputReader.MouseLeftClickEvent -= LeftClicked;
        inputReader.OnInteractableObjectDropEvent -= OnInteractableObjectDrop;
        inputReader.OnReloadEvent -= OnReload;
        inputReader.OnPauseInputEvent -= PauseInput;
    }

    #region Player Input Action Callbacks
    private void RightClicked()
    {
        onMouseRightClicked?.Invoke();
    }

    private void LeftClicked()
    {
        onMouseLeftClicked?.Invoke();
    }

    private void SkillInputed(SkillNumber skillNumber)
    {
        onSkillInputed?.Invoke(skillNumber);
    }

    private void OnInteractableObjectDrop()
    {
        OnInteractableObjectDropInput?.Invoke();
    }

    private void OnReload()
    {
        OnReloadEvent?.Invoke();
    }
    #endregion ===================================
    
    #region Menu Input Action Callbacks
    public void SetGameplayInputEnabled(bool enabled)
    {
        inputReader.SetGameplayInputEnabled(enabled);
    }

    private void PauseInput()
    {
        OnPauseInputEvent?.Invoke();
    }
    #endregion ===================================
}
