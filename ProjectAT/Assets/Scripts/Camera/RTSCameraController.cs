using UnityEngine;
using Unity.Cinemachine; // Cinemachine 3 전용 네임스페이스

public class RTSCameraController : MonoBehaviour
{
    [Header("Cinemachine 3 Components")]
    [SerializeField]
    private CinemachineCamera cinemachineCamera;

    private CinemachineOrbitalFollow orbitalFollow;

    [Header("Edge Scroll Settings")]
    [SerializeField]
    private float panSpeed = 15f;
    [SerializeField]
    private float edgePanBorder = 20f;

    [Header("Orbit Settings")]
    [SerializeField]
    private float orbitSpeed = 150f;

    [Header("Zoom Settings")]
    [SerializeField]
    private float zoomSpeed = 5f;
    [SerializeField]
    private float minRadius = 5f;
    [SerializeField]
    private float maxRadius = 30f;

    private void Start()
    {
        if (cinemachineCamera is not null)
        {
            orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        }
    }

    private void Update()
    {
        HandleEdgeScroll();
        HandleOrbit();
        HandleZoom();
    }

    private void HandleEdgeScroll()
    {
        Vector3 inputDir = Vector3.zero;
        Vector2 mousePos = Managers.Instance.InputManager.MousePosition;

        if (mousePos.y >= Screen.height - edgePanBorder) inputDir.z = 1f;
        if (mousePos.y <= edgePanBorder) inputDir.z = -1f;
        if (mousePos.x >= Screen.width - edgePanBorder) inputDir.x = 1f;
        if (mousePos.x <= edgePanBorder) inputDir.x = -1f;

        if (inputDir != Vector3.zero)
        {
            Transform mainCamTransform = Camera.main.transform;
            Vector3 forward = mainCamTransform.forward;
            Vector3 right = mainCamTransform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = forward * inputDir.z + right * inputDir.x;
            transform.position += moveDir * panSpeed * Time.deltaTime;
        }
    }

    private void HandleOrbit()
    {
        if (Managers.Instance.InputManager.IsWheelClickHolding)
        {
            float rotateDir = Managers.Instance.InputManager.MouseDelta.x;
            orbitalFollow.HorizontalAxis.Value += rotateDir * orbitSpeed * Time.deltaTime;
        }
    }

    private void HandleZoom()
    {
        float scrollAmount = Managers.Instance.InputManager.MouseWheelDelta.y;
        if (Mathf.Abs(scrollAmount) > 0.01f)
        {
            float targetRadius = orbitalFollow.Radius - (scrollAmount * zoomSpeed);
            orbitalFollow.Radius = Mathf.Clamp(targetRadius, minRadius, maxRadius);
        }
    }
}