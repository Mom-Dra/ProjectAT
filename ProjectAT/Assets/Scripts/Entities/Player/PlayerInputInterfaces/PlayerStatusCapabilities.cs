using UnityEngine;

namespace PlayerStatusCapabilities
{
    public interface IRightClickHandler { void OnRightClick(RaycastHit castedObject); }
    public interface ILeftClickHandler  { void OnLeftClick(RaycastHit castedObject); }
    public interface ISkillInputHandler   { void OnSkillInput(SkillNumber skillNumber); }
    public interface IDroppable     { void OnDropPressed(); }
}
