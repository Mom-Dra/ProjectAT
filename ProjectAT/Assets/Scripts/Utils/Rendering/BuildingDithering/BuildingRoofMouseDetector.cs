using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Rendering.BuildingDithering
{
    
    public class BuildingRoofMouseDetector : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float maxDistance = 200f;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        private BuildingRoofDitherController currentDitheredController;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (targetCamera == null) return;

            BuildingRoofDitherController nextController = FindRoofUnderMouse();
            SetCurrentController(nextController);
        }

        private void OnDisable()
        {
            SetCurrentController(null);
        }

        private BuildingRoofDitherController FindRoofUnderMouse()
        {
            Ray ray = targetCamera.ScreenPointToRay(GetMousePosition());

            if(Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, ~0, triggerInteraction))
            {
                BuildingRoofHitbox hitbox = hitInfo.collider.GetComponentInParent<BuildingRoofHitbox>();
                if (hitbox != null)
                {
                    return hitbox.Controller;
                }
            }

            return null;
        }

        private void SetCurrentController(BuildingRoofDitherController nextController)
        {
            if (currentDitheredController == nextController) return;

            if (currentDitheredController != null) currentDitheredController.SetMouseOverRoof(false);
            currentDitheredController = nextController;
            if (currentDitheredController != null) currentDitheredController.SetMouseOverRoof(true);
        }

        private Vector2 GetMousePosition()
        {
            if (Managers.Instance != null && Managers.Instance.InputManager != null)
            {
                return Managers.Instance.InputManager.MousePosition;
            }

    #if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }
    #endif
            return Input.mousePosition;
        }
    }
}
