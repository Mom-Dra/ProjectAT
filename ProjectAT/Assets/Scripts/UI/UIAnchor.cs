using UnityEngine;

public class UIAnchor : MonoBehaviour
{
    [SerializeField]
    private Transform targetUIAnchor;

    public Transform TargetAnchor => targetUIAnchor;
}
