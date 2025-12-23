using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private bool isOccupied;
    private GameObject owner;
    private bool isHighCover;

    public bool IsOccupied => isOccupied;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
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

    public void Show()
    {
        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
