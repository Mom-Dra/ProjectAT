using UnityEngine;

[CreateAssetMenu(fileName = "SecurityCameraData", menuName = "Scriptable Objects/SecurityCameraData")]
public class SecurityCameraData : ScriptableObject
{
    [SerializeField]
    private float scanRange;

    [SerializeField]
    private float viewAngle;

    public float ScanRange => scanRange;
    public float ViewAngle => viewAngle;
}
