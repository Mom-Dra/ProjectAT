using UnityEngine;

public class EffectModule : MonoBehaviour
{
    [Header("Firing Effect")]
    [SerializeField] private ParticleSystem firingEffect;
    [SerializeField] private Transform firingEffectSpawnPoint;

    private void Awake()
    {
        Initiate();
    }

    private void Initiate()
    {
        LinkParticles();

        firingEffect.transform.position = firingEffectSpawnPoint.position;
        firingEffect.transform.rotation = firingEffectSpawnPoint.rotation;
    }

    private void LinkParticles()
    {
        firingEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        firingEffectSpawnPoint = transform.parent.GetChild(3).transform;
    }

    public void PlayFiringEffect()
    {
        //Instantiate(firingEffect, firingEffectSpawnPoint.position, firingEffectSpawnPoint.rotation);
        firingEffect.Play(firingEffectSpawnPoint);
    }
}
