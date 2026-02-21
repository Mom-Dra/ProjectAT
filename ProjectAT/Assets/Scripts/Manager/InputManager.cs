using System;
using UnityEngine;

public class InputManager
{
    public event Action<SkillNumber> onSkillInputed;
    public event Action onMouseRightClicked;
    public event Action onMouseLeftClicked;

    private InputReader inputReader;

    public Vector2 MousePosition => inputReader.MousePosition;

    public InputManager(InputReader inputReader)
    {
        this.inputReader = inputReader;

        inputReader.SkillInputEvent += SkillInputed;
        inputReader.MouseRightClickEvent += RightClicked;
        inputReader.MouseLeftClickEvent += LeftClicked;
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
}
