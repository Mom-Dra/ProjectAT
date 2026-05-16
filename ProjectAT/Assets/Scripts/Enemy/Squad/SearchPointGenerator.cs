using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public static class SearchPointGenerator
{
    public static void Generate(Vector3 center, float radius, int count, Vector3[] outPoints, float sampleMaxDistance = 2f)
    {
        Assert.IsNotNull(outPoints);
        Assert.IsTrue(outPoints.Length >= count, "outPoints buffer too small");

        if (count <= 0) return;

        for (int i = 0; i < count; ++i)
        {
            float angle = 2f * Mathf.PI / count * i;

            Vector3 candidate = center + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
                outPoints[i] = hit.position;
            else outPoints[i] = center;
        }
    }
}
