using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager
{
    private HashSet<GameObject> prefabs = new HashSet<GameObject>();

    private Dictionary<GameObject, ObjectPool<GameObject>> pooledObjects = new Dictionary<GameObject, ObjectPool<GameObject>>();

    public PoolManager(PoolConfigObject[] pooledPrefabs)
    {
        foreach (PoolConfigObject pooledPrefab in pooledPrefabs)
            RegisterPrefabInternal(pooledPrefab.Prefab, pooledPrefab.PrewarmCount, pooledPrefab.PoolParentTransform);
    }

    public GameObject GetObject(GameObject prefab)
    {
        GameObject returnObject = pooledObjects[prefab].Get();
        return returnObject;
    }

    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject returnObject = GetObject(prefab);

        returnObject.transform.position = position;
        returnObject.transform.rotation = rotation;

        if (returnObject.TryGetComponent(out PooledObject pooledObject))
        {
            pooledObject.Prefab = prefab;
        }
        else
        {
            pooledObject = returnObject.AddComponent<PooledObject>();
            pooledObject.Prefab = prefab;
        }

        return returnObject;
    }

    public void ReturnObject(GameObject gameObject, GameObject prefab)
    {
        pooledObjects[prefab].Release(gameObject);
    }

    private void RegisterPrefabInternal(GameObject prefab, int prewarmCount, Transform poolParentTransform)
    {
        GameObject CreateFunc()
        {
            return Object.Instantiate(prefab, poolParentTransform);
        }

        void ActionOnGet(GameObject gameObject)
        {
            gameObject.SetActive(true);
        }

        void ActionOnRelease(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

        void ActionOnDestroy(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }

        prefabs.Add(prefab);

        // Create the pool
        pooledObjects[prefab] = new ObjectPool<GameObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

        // Populate the pool
        GameObject[] prewarmObjects = new GameObject[prewarmCount];

        for (int i = 0; i < prewarmCount; ++i)
            prewarmObjects[i] = pooledObjects[prefab].Get();

        foreach (GameObject prewarmObject in prewarmObjects)
            pooledObjects[prefab].Release(prewarmObject);
    }
}
