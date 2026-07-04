using UnityEngine;

namespace PlayerStateCapabilities
{
    public interface IRightClickHandler { void OnRightClick(RaycastHit castedObject); }
    public interface ILeftClickHandler  { void OnLeftClick(RaycastHit castedObject); }
    public interface ISkillInputHandler   { void OnSkillInput(SkillNumber skillNumber); }
    public interface IDropObjectHandler     { void OnDropObjectInput(); }
    public interface IReloadInputHandler   { void OnReloadInput(); }
    public interface IInterruptiblePlayerState { void Interrupt(); }
}
