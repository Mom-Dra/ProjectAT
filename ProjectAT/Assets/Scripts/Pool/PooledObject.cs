using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private GameObject prefab;
    public GameObject Prefab { get => prefab; set => prefab = value; }

    public void ReturnToPool()
    {
        if (prefab is null)
        {
            Debug.LogWarning("prefab is null");
            return;
        }

        if (InGameManager.Instance == null || InGameManager.Instance.PoolManager == null)
        {
            Debug.LogWarning("InGameManager or PoolManager is null");
            return;
        }

        InGameManager.Instance.PoolManager.ReturnObject(gameObject, prefab);
    }
}
