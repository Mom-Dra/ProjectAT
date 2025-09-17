using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(ParticleSystem))]
public class ReturnParticleSystemToPool : MonoBehaviour
{
    private ParticleSystem system;
    private IObjectPool<ParticleSystem> pool;

    internal void Initialize(ParticleSystem particleSystem, IObjectPool<ParticleSystem> pool)
    {
        this.pool = pool;
        system = particleSystem;

        ParticleSystem.MainModule main = particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void OnParticleSystemStopped()
    {
        pool.Release(system);
    }
}
