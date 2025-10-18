using UnityEngine;

public class EffectModule : MonoBehaviour
{
    [Header("Firing Effect")]
    [SerializeField] private GameObject bulletProjectile;
    [SerializeField] private Transform firingEffectSpawnPoint;

    private void Awake()
    {
        Initiate();
    }

    private void Initiate()
    {
        LinkParticles();

    }

    private void LinkParticles()
    {
        firingEffectSpawnPoint = transform.parent.GetChild(3).transform;
    }

    public void PlayFiringEffect(Vector3 dest)
    {
        Bullet bulletComponent 
            = Instantiate(bulletProjectile, 
            firingEffectSpawnPoint.position, 
            firingEffectSpawnPoint.rotation
            ).GetComponent<Bullet>();   //나중에 오브젝트 풀링할 것이므로 생성요청 보내는 로직 적어야함.
        
        bulletComponent.Initialize(dest, 2f);
        bulletComponent.SetVelocity((dest - firingEffectSpawnPoint.position).normalized * 100f);

    }
}
