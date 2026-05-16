using UnityEngine;

public class CircleFormation : IFormation
{
    private float radius;

    public CircleFormation(float radius)
    {
        this.radius = radius;
    }

    public void CalculateSlots(Vector3 center, Vector3 forward, int count, Vector3[] slots)
    {
        if (count <= 0) return;

        if (slots is null)
        {
            Debug.LogError("slots is null");
            return;
        }

        if (slots.Length < count)
        {
            Debug.LogError("slots.Length < count");
            return;
        }

        float baseAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        float step = 360f / count;

        for (int i = 0; i < count; ++i)
        {
            float angleDeg = baseAngle + step * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            slots[i] = center + new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad)) * radius;
        }
    }
}