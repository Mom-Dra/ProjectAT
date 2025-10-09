using Biostart.Bullet;
using Unity.Netcode;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField]
    private GunData gunData;
    public GunData GunData => gunData;

    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private GameObject hitPrefab;

    private IGunState gunState = IGunState.ReadyState;

    private LineRenderer lineRenderer;
    private AudioSource audioSource;
    private ParticleSystem muzzleParticleSystem;

    private int remainAmmo; // 남은 전체 탄약
    private int magAmmo; // 탄창에 남은 탄약

    internal int RemainAmmo { get => remainAmmo; set => remainAmmo = value; }
    internal int MagAmmo { get => magAmmo; set => magAmmo = value; }

    public bool CanFire => gunState == IGunState.ReadyState;

    // Animator 쪽 실제 에셋 붙여보고 생각해보자

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        muzzleParticleSystem = GetComponentInChildren<ParticleSystem>();

        //lineRenderer.positionCount = 2;
        //lineRenderer.enabled = false;

        remainAmmo = gunData.StartRemainAmmo;
        magAmmo = gunData.MagCapacity;
    }

    private void RemainAmmoValueChanged(int previousRemainAmmo, int currentRemainAmmo)
    {
        // UI Update...
    }

    private void MagAmmoValueChanged(int previousMagAmmo, int currentMagAmmo)
    {
        // UI Update...
    }

    //[Rpc(SendTo.Server)]

    private void Fire()
    {
        gunState.Fire(this);
    }

    public override void Attack()
    {
        Fire();
    }

    public void Reload()
    {
        gunState.Reload(this);
    }

    internal void ChangeState(IGunState gunState)
    {
        this.gunState = gunState;
        gunState.Enter(this);
    }

    // Only Server
    internal void PerformFire()
    {
        --magAmmo;

        RaycastHit hit;
        Debug.DrawRay(muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.forward * gunData.MaxDistance, Color.blue, 2f);
        if (Physics.Raycast(muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.forward, out hit, gunData.MaxDistance, LayerMask.GetMask("Player")))
        {
            Debug.Log("맞았다!!");

            Debug.DrawRay(muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.forward * Vector3.Distance(muzzleParticleSystem.transform.position, hit.point), Color.red, 2f);

            // Hit Particle
            GameObject hitObject = PoolManager.Instance.GetObject(hitPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            if (hitObject.TryGetComponent(out ParticleSystem hitParticle))
                hitParticle.Play();

            // Bullet 날리기
            GameObject bulletObject = PoolManager.Instance.GetObject(bulletPrefab, muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.rotation);
            if(bulletObject.TryGetComponent(out Bullet bullet))
            {
                bullet.Initialize(hit.point, 5f);
                bullet.SetVelocity(transform.forward * 100f);
            }

            // 데미지 감소
            if (hit.transform.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(gunData.Damage);
        }
        else
        {
            GameObject bulletObject = PoolManager.Instance.GetObject(bulletPrefab, muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.rotation);
            if (bulletObject.TryGetComponent(out Bullet bullet))
            {
                bullet.Initialize(muzzleParticleSystem.transform.position + muzzleParticleSystem.transform.forward * gunData.MaxDistance, 5f);
                bullet.SetVelocity(transform.forward * 100f);
            }
        }
    }

    internal bool CanReload()
    {
        return remainAmmo > 0 && magAmmo < gunData.MagCapacity;
    }

    internal void PerformReload()
    {
        int neededAmmo = gunData.MagCapacity - magAmmo;
        int ammoToMove = Mathf.Min(neededAmmo, remainAmmo);

        magAmmo += ammoToMove;
        remainAmmo -= ammoToMove;
    }

    //Client 쪽에서 Effect만 재생할 거임
    //[Rpc(SendTo.ClientsAndHost)]
    internal void PlayFireRpc()
    {
        Debug.Log("PlayFireRpc");
        audioSource.PlayOneShot(gunData.ShotClip);

        // 화염
        // 암살 게임 이므로 muzzle 이펙트는 없는 걸로
        //muzzleParticleSystem.Play();
    }

    //[Rpc(SendTo.ClientsAndHost)]
    internal void PlayReloadRpc()
    {
        Debug.Log("PlayReloadRpc");
        audioSource.PlayOneShot(gunData.ReloadClip);

        // Reload Animation
    }
    // Ready -> 
}
