using UnityEngine;

public class EffectModule : MonoBehaviour
{
    [Header("Firing Effect")]
    [SerializeField] private GameObject bulletProjectile;
    [SerializeField] private Transform firingEffectSpawnPoint;

    [Header("Mouse Effect")]
    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private FieldIndicator moveIndicatorInstance;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (firingEffectSpawnPoint is null)
            firingEffectSpawnPoint = transform.GetChild(2).transform;

        moveIndicatorInstance = Instantiate(moveIndicatorPrefab).GetComponent<FieldIndicator>();
    }

    public void PlayFiringEffect(Vector3 dest)
    {
        Bullet bulletComponent 
            = Instantiate(bulletProjectile, 
            firingEffectSpawnPoint.position, 
            firingEffectSpawnPoint.rotation
            ).GetComponent<Bullet>();   //나중에 오브젝트 풀링할 것이므로 생성요청 보내는 로직 적어야함.
        
        //이부분도 Bullet 함수 안에..
        bulletComponent.Initialize(dest, 2f);
        bulletComponent.transform.forward = (dest - firingEffectSpawnPoint.position).normalized;
        bulletComponent.SetVelocity((dest - firingEffectSpawnPoint.position).normalized * 100f);
    }

    public void PlayMoveIndicatorEffect(Vector3 dest)
    {
        moveIndicatorInstance.SpawnIndicator(dest);
    }
}
