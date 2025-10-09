using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    [SerializeField]
    private bool collectionCheck = true;

    [SerializeField]
    private List<PoolConfigObject> PooledPrefabsList;

    private HashSet<GameObject> prefabs = new HashSet<GameObject>();

    private Dictionary<GameObject, ObjectPool<GameObject>> pooledObjects = new Dictionary<GameObject, ObjectPool<GameObject>>();

    [SerializeField]
    private Transform poolParentTransform;

    protected override void Awake()
    {
        base.Awake();

        foreach (PoolConfigObject configObject in PooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
        }
    }

    private void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
    {
        GameObject CreateFunc()
        {
            return Instantiate(prefab, poolParentTransform);
        }

        void ActionOnGet(GameObject networkObject)
        {
            networkObject.gameObject.SetActive(true);
        }

        void ActionOnRelease(GameObject networkObject)
        {
            networkObject.gameObject.SetActive(false);
        }

        void ActionOnDestroy(GameObject networkObject)
        {
            Destroy(networkObject.gameObject);
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

    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject returnObject = pooledObjects[prefab].Get();

        returnObject.transform.position = position;
        returnObject.transform.rotation = rotation;

        return returnObject;
    }
}
