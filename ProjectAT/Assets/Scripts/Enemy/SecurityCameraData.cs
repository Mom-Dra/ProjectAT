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
    private LayerMask playerLayer;

    [SerializeField]
    private LayerMask obstacleLayer;

    public float ViewAngle => viewAngle;
    public float RotateAngle => rotateAngle;
    public float RotateSpeed => rotateSpeed;
    public float DetectionRange => detectionRange;
    public float DetectionAngle => detectionAngle;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask ObstacleLayer => obstacleLayer;
}
