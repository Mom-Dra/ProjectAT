using System;
using UnityEngine;

public class InputManager
{
    public event Action<SkillNumber> onSkillInputed;
    public event Action onMouseRightClicked;
    public event Action onMouseLeftClicked;
    public event Action OnInteractableObjectDropInput;
    public event Action OnReloadEvent;

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
    }

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
}
