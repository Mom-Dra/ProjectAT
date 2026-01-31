using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CoverPoint : MonoBehaviour
{
    private DecalProjector decalProjector;
    private CoverPulse coverPulse;

    private bool isOccupied;
    private GameObject owner;
    private bool isPlayerMovingTarget;

    public bool IsOccupied => isOccupied;

    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();
        decalProjector.enabled = false;

        coverPulse = GetComponent<CoverPulse>();
    }

    public bool Reserve(GameObject npc)
    {
        if (isOccupied) return false;

        isOccupied = true;
        owner = npc;

        return true;
    }

    public void Release()
    {
        isOccupied = false;
        owner = null;
    }

    public void ShowIndicator()
    {
        if (!isOccupied)
            decalProjector.enabled = true;
    }

    public void HideIndicator()
    {
        if (isPlayerMovingTarget) return;

        decalProjector.enabled = false;
    }

    public void ShowPulse()
    {
        coverPulse.enabled = true;
    }

    public void HidePulse()
    {
        if (isPlayerMovingTarget) return;

        coverPulse.enabled = false;
    }

    public void SetMoveTarget(bool active)
    {
        isPlayerMovingTarget = active;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
