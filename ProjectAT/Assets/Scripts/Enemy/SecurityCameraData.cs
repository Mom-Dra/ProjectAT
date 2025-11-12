using UnityEngine;

[CreateAssetMenu(fileName = "SecurityCameraData", menuName = "Scriptable Objects/SecurityCameraData")]
public class SecurityCameraData : ScriptableObject
{
    [SerializeField, Range(0f, 90f)]
    private float viewAngle;

    [SerializeField, Range(0f, 180f)]
    private float rotateAngle;

    [SerializeField]
    private float rotateSpeed;

    [SerializeField]
    private float detectionRange = 10f;

    [SerializeField, Range(0f, 180f)]
    private float detectionAngle = 120f;

    [SerializeField]
    private float detectionTime;

    [SerializeField]
    private LayerMask playerMask;

    [SerializeField]
    private LayerMask obstacleMask;

    public float ViewAngle => viewAngle;
    public float RotateAngle => rotateAngle;
    public float RotateSpeed => rotateSpeed;
    public float DetectionRange => detectionRange;
    public float DetectionAngle => detectionAngle;
    public float DetectionTime => detectionTime;
    public LayerMask PlayerMask => playerMask;
    public LayerMask ObstacleMask => obstacleMask;
}
