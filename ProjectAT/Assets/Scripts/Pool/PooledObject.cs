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

        Managers.Instance.PoolManager.ReturnObject(gameObject, prefab);
    }
}
