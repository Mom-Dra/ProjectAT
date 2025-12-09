using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    private bool isOccupied;
    private GameObject owner;
    private bool isHighCover;

    public bool IsOccupied => isOccupied;


    public bool Reserve(GameObject npc)
    {
        if (isOccupied) return false;

        isOccupied = true;
        owner = npc;

        return true;
    }

    public void Vacate()
    {
        isOccupied = false;
        owner = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
