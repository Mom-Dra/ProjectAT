using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
/// <summary>
/// Object Pool for networked objects, used for controlling how objects are spawned by Netcode. Netcode by default
/// will allocate new memory when spawning new objects. With this Networked Pool, we're using the ObjectPool to
/// reuse objects.
/// Boss Room uses this for projectiles. In theory it should use this for imps too, but we wanted to show vanilla spawning vs pooled spawning.
/// Hooks to NetworkManager's prefab handler to intercept object spawning and do custom actions.
/// </summary>
public class NetworkObjectPool : NetworkBehaviour
{
    public static NetworkObjectPool Instance { get; private set; }

    [SerializeField]
    private List<PoolConfigObject> PooledPrefabsList;

    private HashSet<GameObject> m_Prefabs = new HashSet<GameObject>();

    private Dictionary<GameObject, ObjectPool<NetworkObject>> m_PooledObjects = new Dictionary<GameObject, ObjectPool<NetworkObject>>();

    [SerializeField]
    private Transform poolParentTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        // Registers all objects in PooledPrefabsList to the cache.
        foreach (PoolConfigObject configObject in PooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
        }
    }

    public override void OnNetworkDespawn()
    {
        // Unregisters all objects in PooledPrefabsList from the cache.
        foreach (GameObject prefab in m_Prefabs)
        {
            // Unregister Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
            m_PooledObjects[prefab].Clear();
        }

        m_PooledObjects.Clear();
        m_Prefabs.Clear();
    }

    public void OnValidate()
    {
        for (int i = 0; i < PooledPrefabsList.Count; i++)
        {
            GameObject prefab = PooledPrefabsList[i].Prefab;

            if (prefab != null)
            {
                Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
            }
        }
    }

    /// <summary>
    /// Gets an instance of the given prefab from the pool. The prefab must be registered to the pool.
    /// </summary>
    /// <remarks>
    /// To spawn a NetworkObject from one of the pools, this must be called on the server, then the instance
    /// returned from it must be spawned on the server. This method will then also be called on the client by the
    /// PooledPrefabInstanceHandler when the client receives a spawn message for a prefab that has been registered
    /// here.
    /// </remarks>
    /// <param name="prefab"></param>
    /// <param name="position">The position to spawn the object at.</param>
    /// <param name="rotation">The rotation to spawn the object with.</param>
    /// <returns></returns>
    public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        NetworkObject networkObject = m_PooledObjects[prefab].Get();

        Transform noTransform = networkObject.transform;
        noTransform.position = position;
        noTransform.rotation = rotation;

        return networkObject;
    }

    /// <summary>
    /// Return an object to the pool (reset objects before returning).
    /// </summary>
    public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
    {
        m_PooledObjects[prefab].Release(networkObject);
    }

    /// <summary>
    /// Builds up the cache for a prefab.
    /// </summary>
    private void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
    {
        NetworkObject CreateFunc()
        {
            Debug.Log("Why Generate?");
            return Instantiate(prefab, poolParentTransform).GetComponent<NetworkObject>();
        }

        void ActionOnGet(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(true);
        }

        void ActionOnRelease(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(false);
        }

        void ActionOnDestroy(NetworkObject networkObject)
        {
            Destroy(networkObject.gameObject);
        }

        m_Prefabs.Add(prefab);

        // Create the pool
        m_PooledObjects[prefab] = new ObjectPool<NetworkObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

        // Populate the pool
        List<NetworkObject> prewarmNetworkObjects = new List<NetworkObject>();

        for (int i = 0; i < prewarmCount; i++)
        {
            prewarmNetworkObjects.Add(m_PooledObjects[prefab].Get());
        }

        foreach (NetworkObject networkObject in prewarmNetworkObjects)
        {
            m_PooledObjects[prefab].Release(networkObject);
        }

        // Register Netcode Spawn handlers
        NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab, this));
    }
}

[Serializable]
struct PoolConfigObject
{
    public GameObject Prefab;
    public int PrewarmCount;
}

class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    private GameObject prefab;
    private NetworkObjectPool pool;

    public PooledPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool)
    {
        this.prefab = prefab;
        this.pool = pool;
    }

    NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        return pool.GetNetworkObject(prefab, position, rotation);
    }

    void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
    {
        pool.ReturnNetworkObject(networkObject, prefab);
    }
}
