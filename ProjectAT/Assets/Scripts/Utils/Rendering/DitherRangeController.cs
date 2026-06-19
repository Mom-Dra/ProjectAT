using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DitherRangeController : MonoBehaviour
{
    private static readonly int MouseDitherParams = Shader.PropertyToID("_MouseDitherParams");
    private static readonly int MouseDitherActive = Shader.PropertyToID("_MouseDitherActive");

    private static readonly int PlayerDitherPositionRadius = Shader.PropertyToID("_PlayerDitherPositionRadius");
    private static readonly int PlayerDitherSettings = Shader.PropertyToID("_PlayerDitherSettings");

    [Header("Mouse Dither")]

    [SerializeField] private bool useMouseDither = true;
    [SerializeField, Min(0f)]private float mouseRadiusPixels = 120f;
    [SerializeField, Min(0f)] private float mouseSoftnessPixels = 24f;
    [SerializeField] private bool flipMouseY;

    [Header("Player Dither")]
    [SerializeField] private bool usePlayerDither = true;
    [SerializeField] private Transform playerTarget;
    [SerializeField, Min(0f)] private float playerRadiusWorld = 6f;
    [SerializeField, Min(0f)] private float playerSoftnessWorld  = 1f;

    [Header("Common")] 
    [SerializeField, Range(0f, 1f)] private float targetOpacity = 0.45f;

    private Managers managers;
    
    private void Awake()
    {
        managers = FindFirstObjectByType<Managers>();

        if(playerTarget == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if(player != null)
            {
                playerTarget = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        UpdateMouseDither();
        UpdatePlayerDither();
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat(MouseDitherActive, 0f);
        Shader.SetGlobalVector(PlayerDitherSettings, Vector4.zero);
    }

    private void UpdateMouseDither()
    {
        Vector2 mousePosition = GetMousePosition();

        if (flipMouseY)
        {
            mousePosition.y = Screen.height - mousePosition.y;
        }

        Shader.SetGlobalVector(MouseDitherParams, new Vector4(mousePosition.x, mousePosition.y, mouseRadiusPixels, mouseSoftnessPixels));
        Shader.SetGlobalFloat(MouseDitherActive, useMouseDither ? 1f : 0f);
    }

    private void UpdatePlayerDither()
    {
        bool playerActive = usePlayerDither && playerTarget != null;
        Vector3 playerPosition = playerActive ? playerTarget.position : Vector3.zero;

        Shader.SetGlobalVector(PlayerDitherPositionRadius, new Vector4(playerPosition.x, playerPosition.y, playerPosition.z, playerRadiusWorld));
        Shader.SetGlobalVector(PlayerDitherSettings, new Vector4(playerActive ? 1f : 0f, playerSoftnessWorld, targetOpacity, 0f));
    }

    private Vector2 GetMousePosition()
    {
        if(managers == null)
        {
            managers = FindFirstObjectByType<Managers>();
        }
        if(managers != null && managers.InputManager != null)
        {
            return managers.InputManager.MousePosition;
        }

#if ENABLE_INPUT_SYSTEM
        if(Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif
        return Input.mousePosition;
    }

}
