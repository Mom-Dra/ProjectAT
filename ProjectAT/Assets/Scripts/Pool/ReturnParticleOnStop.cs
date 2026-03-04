using UnityEngine;

[RequireComponent(typeof(ParticleSystem), typeof(PooledObject))]
public class ReturnParticleOnStop : MonoBehaviour
{
    private PooledObject pooledObject;

    private void Awake()
    {
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;

        pooledObject = GetComponent<PooledObject>();
    }

    private void OnParticleSystemStopped()
    {
        Debug.Log("OnParticleSystemStopped");
        pooledObject.ReturnToPool();
    }
}
