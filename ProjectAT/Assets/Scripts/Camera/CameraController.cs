using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera myCamera;
    [SerializeField] private CinemachineCamera cinemachine;
    public Camera MyCamera { get { return myCamera; } }

    private void Awake()
    {
        myCamera = transform.GetChild(0).GetComponent<Camera>();
        cinemachine = GetComponent<CinemachineCamera>();
    }

    public void SetCameraTarget(Transform tf)
    {
        cinemachine.Target.TrackingTarget = tf;
    }
}
