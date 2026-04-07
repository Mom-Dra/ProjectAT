using Biostart.Bullet;
using Unity.Netcode;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField]
    private GunData gunData;
    public GunData GunData => gunData;

    private IGunState gunState = IGunState.ReadyState;

    private LineRenderer lineRenderer;
    private AudioSource audioSource;
    [SerializeField] private ParticleSystem muzzleParticleSystem;
    private EntityStatus ownerStatus;

    private int remainAmmo; // ���� ��ü ź��
    private int magAmmo; // źâ�� ���� ź��

    internal int RemainAmmo { get => remainAmmo; set => remainAmmo = value; }
    internal int MagAmmo { get => magAmmo; set => magAmmo = value; }

    public override bool IsReady => gunState == IGunState.ReadyState;
    public override bool IsReloading => gunState == IGunState.ReloadState;



    // Animator �� ���� ���� �ٿ����� �����غ���

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        muzzleParticleSystem = GetComponentInChildren<ParticleSystem>();
        ownerStatus = GetComponentInParent<EntityStatus>();

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
            //Debug.Log("�¾Ҵ�!!");

            Debug.DrawRay(muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.forward * Vector3.Distance(muzzleParticleSystem.transform.position, hit.point), Color.red, 2f);

            // Hit Particle
            GameObject hitObject = Managers.Instance.PoolManager.GetObject(gunData.HitPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            if (hitObject.TryGetComponent(out ParticleSystem hitParticle))
                hitParticle.Play();

            // Bullet ������
            GameObject bulletObject = Managers.Instance.PoolManager.GetObject(gunData.BulletPrefab, muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.rotation);
            if (bulletObject.TryGetComponent(out Bullet bullet))
            {
                bullet.Initialize(hit.point, 5f);
                bullet.SetVelocity(transform.forward * 100f);
            }

            // ������ ����
            if (hit.transform.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(gunData.Damage, ownerStatus);
        }
        else
        {
            GameObject bulletObject = Managers.Instance.PoolManager.GetObject(gunData.BulletPrefab, muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.rotation);
            if (bulletObject.TryGetComponent(out Bullet bullet))
            {
                Vector3 dest = muzzleParticleSystem.transform.position + muzzleParticleSystem.transform.forward * gunData.MaxDistance;
                bullet.Initialize(dest, 5f);
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

    //Client �ʿ��� Effect�� ����� ����
    //[Rpc(SendTo.ClientsAndHost)]
    internal void PlayFireRpc()
    {
        Debug.Log("PlayFireRpc");
        audioSource.PlayOneShot(gunData.ShotClip);

        // ȭ��
        // �ϻ� ���� �̹Ƿ� muzzle ����Ʈ�� ���� �ɷ�
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
