using System.CodeDom.Compiler;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoverGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject coverPointPrefab;

    [SerializeField]
    private float offset = 1f;

    [SerializeField]
    private List<CoverPoint> generatedPoint = new List<CoverPoint>();

    [ContextMenu("Generate Points")]
    private void GenerateCoverPoints()
    {
        ClearPoints();

        Collider collider = GetComponent<Collider>();

        CreatePoint(Vector3.forward, collider.bounds);
        CreatePoint(Vector3.back, collider.bounds);
        CreatePoint(Vector3.left, collider.bounds);
        CreatePoint(Vector3.right, collider.bounds);
    }

    private void CreatePoint(Vector3 direction, Bounds bounds)
    {
        // ��ġ ���: �߽� + (���� * (������ + ������))
        // bounds.extends�� �߽ɿ��� �������� �Ÿ�(������ ����)
        float dist = (direction.x != 0) ? bounds.extents.x : bounds.extents.z;
        Vector3 spawnPos = transform.position + (direction * (dist + offset));

        spawnPos.y = transform.position.y;

        GameObject coverPointObject = Instantiate(coverPointPrefab, spawnPos, Quaternion.LookRotation(Vector3.down), transform);
        
        if(coverPointObject.TryGetComponent(out CoverPoint coverPoint))
        {
            generatedPoint.Add(coverPoint);
        }
    }

    [ContextMenu("Clear Points")]
    private void ClearPoints()
    {
        foreach (CoverPoint point in generatedPoint)
            DestroyImmediate(point.gameObject);

        generatedPoint.Clear();
    }
}
