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

    private NetworkVariable<int> remainAmmo = new NetworkVariable<int>(0); // 남은 전체 탄약
    private NetworkVariable<int> magAmmo = new NetworkVariable<int>(0); // 탄창에 남은 탄약

    internal int RemainAmmo { get => remainAmmo.Value; set => remainAmmo.Value = value; }
    internal int MagAmmo { get => magAmmo.Value; set => magAmmo.Value = value; }

    public bool CanFire => gunState == IGunState.ReadyState;

    // Animator 쪽 실제 에셋 붙여보고 생각해보자

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        muzzleParticleSystem = GetComponentInChildren<ParticleSystem>();

        //lineRenderer.positionCount = 2;
        //lineRenderer.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            remainAmmo.Value = gunData.StartRemainAmmo;
            magAmmo.Value = gunData.MagCapacity;
        }

        remainAmmo.OnValueChanged += RemainAmmoValueChanged;
        magAmmo.OnValueChanged += MagAmmoValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        remainAmmo.OnValueChanged -= RemainAmmoValueChanged;
        magAmmo.OnValueChanged -= MagAmmoValueChanged;
    }

    private void RemainAmmoValueChanged(int previousRemainAmmo, int currentRemainAmmo)
    {
        // UI Update...
    }

    private void MagAmmoValueChanged(int previousMagAmmo, int currentMagAmmo)
    {
        // UI Update...
    }

    [Rpc(SendTo.Server)]

    private void FireRpc()
    {
        gunState.Fire(this);
    }

    public override void Attack()
    {
        FireRpc();
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
        --magAmmo.Value;

        RaycastHit hit;
        Debug.DrawRay(muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.forward * gunData.MaxDistance, Color.blue, 2f);
        if (Physics.Raycast(muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.forward, out hit, gunData.MaxDistance, LayerMask.GetMask("Player")))
        {
            Debug.Log("맞았다!!");

            Debug.DrawRay(muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.forward * Vector3.Distance(muzzleParticleSystem.transform.position, hit.point), Color.red, 2f);

            // Hit Particle
            ParticleSystem hitParticle = HitObjectPool.Instance.Pool.Get();
            hitParticle.transform.position = hit.point;
            hitParticle.transform.rotation = Quaternion.LookRotation(hit.normal);
            hitParticle.Play();

            // Bullet 날리기
            NetworkObject bulletNetworkObject = NetworkObjectPool.Instance.GetNetworkObject(bulletPrefab, muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.rotation);
            bulletNetworkObject.Spawn(true);

            Bullet bullet = bulletNetworkObject.GetComponent<Bullet>();
            bullet.Initialize(hit.point, 5f);
            bullet.SetVelocity(transform.forward * 100f);

            // 데미지 감소
            if (hit.transform.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(gunData.Damage);
        }
        else
        {
            NetworkObject bulletNetworkObject = NetworkObjectPool.Instance.GetNetworkObject(bulletPrefab, muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.rotation);
            bulletNetworkObject.Spawn(true);

            Bullet bullet = bulletNetworkObject.GetComponent<Bullet>();
            bullet.Initialize(muzzleParticleSystem.transform.position + muzzleParticleSystem.transform.forward * gunData.MaxDistance, 5f);
            bullet.SetVelocity(transform.forward * 100f);
        }
    }

    internal bool CanReload()
    {
        return RemainAmmo > 0 && MagAmmo < gunData.MagCapacity;
    }

    internal void PerformReload()
    {
        int neededAmmo = gunData.MagCapacity - MagAmmo;
        int ammoToMove = Mathf.Min(neededAmmo, RemainAmmo);

        MagAmmo += ammoToMove;
        RemainAmmo -= ammoToMove;
    }

    // Client 쪽에서 Effect만 재생할 거임
    [Rpc(SendTo.ClientsAndHost)]
    internal void PlayFireRpc()
    {
        Debug.Log("PlayFireRpc");
        audioSource.PlayOneShot(gunData.ShotClip);

        // 화염
        // 암살 게임 이므로 muzzle 이펙트는 없는 걸로
        //muzzleParticleSystem.Play();
    }

    [Rpc(SendTo.ClientsAndHost)]
    internal void PlayReloadRpc()
    {
        Debug.Log("PlayReloadRpc");
        audioSource.PlayOneShot(gunData.ReloadClip);

        // Reload Animation
    }
    // Ready -> 
}
