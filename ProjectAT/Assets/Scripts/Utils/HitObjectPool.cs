using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class HitObjectPool : Singleton<HitObjectPool>
{
    [SerializeField]
    private bool collectionCheck = true;

    [SerializeField]
    private int maxPoolSize = 100;

    [SerializeField]
    private GameObject hitPrefab;

    [SerializeField]
    private Transform parentTransfrom;

    private ObjectPool<ParticleSystem> pool;

    public ObjectPool<ParticleSystem> Pool
    {
        get
        {
            if (pool == null)
                pool = new ObjectPool<ParticleSystem>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionCheck, 50, maxPoolSize);

            return pool;
        }
    }

    private void Start()
    {
        for (int i = 0; i < 10; ++i)
            Instance.Pool.Get();
    }

    private ParticleSystem CreatePooledItem()
    {
        ParticleSystem hitParticles = Instantiate(hitPrefab, transform.position + new Vector3(0f, 0f, -2f), Quaternion.identity.normalized, parentTransfrom).GetComponent<ParticleSystem>();
        hitParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        //This is used to return ParticleSystems to the pool when they have stopped.
        ReturnParticleSystemToPool returnToPool = hitParticles.gameObject.AddComponent<ReturnParticleSystemToPool>();
        returnToPool.Initialize(hitParticles, pool);

        return hitParticles;
    }

    // Called when an item is returned to the pool using Release
    private void OnReturnedToPool(ParticleSystem system)
    {
        system.gameObject.SetActive(false);
        //system.gameObject.GetComponent<AudioSource>().enabled = false;
    }

    // Called when an item is taken from the pool using Get
    private void OnTakeFromPool(ParticleSystem system)
    {
        system.gameObject.SetActive(true);
        //system.gameObject.GetComponent<AudioSource>().enabled = true;
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    private void OnDestroyPoolObject(ParticleSystem system)
    {
        Destroy(system.gameObject);
    }
}
