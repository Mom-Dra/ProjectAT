using UnityEngine;

public class EffectModule : MonoBehaviour
{
    [Header("Firing Effect")]
    [SerializeField] private GameObject firingEffect;
    [SerializeField] private Transform firingEffectSpawnPoint;

    public void GenerateFiringEffect()
    {
        Instantiate(firingEffect, firingEffectSpawnPoint.position, firingEffectSpawnPoint.rotation);
    }
}
