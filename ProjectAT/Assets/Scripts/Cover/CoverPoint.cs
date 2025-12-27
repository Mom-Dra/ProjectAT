using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CoverPoint : MonoBehaviour
{
    private DecalProjector decalProjector;

    private bool isOccupied;
    private GameObject owner;
    private bool isHighCover;

    public bool IsOccupied => isOccupied;

    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();
        decalProjector.enabled = false;
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
        decalProjector.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
