using UnityEngine;

public interface IFormation
{
    public void CalculateSlots(Vector3 center, Vector3 forward, int count, Vector3[] slots);
}
