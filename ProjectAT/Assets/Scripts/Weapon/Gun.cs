using Biostart.Bullet;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;

public class Gun : Weapon
{
    [SerializeField]
    private GunData gunData;
    public GunData GunData => gunData;

    private Coroutine reloadCoroutine;
    private float currentFireTime;
    [SerializeField] private ParticleSystem muzzleParticleSystem;

    private int remainAmmo; // 현재 보유 총알(즉, totalAmmo - magAmmo)
    private int magAmmo; // 탄창 속 남아있는 총알

    internal int RemainAmmo { get => remainAmmo; set => remainAmmo = value; }
    internal int MagAmmo { get => magAmmo; set => magAmmo = value; }

    public override bool IsReloading => reloadCoroutine != null;
    public override bool IsReady => Time.time - currentFireTime >= gunData.TimeBetFire;
    public bool HasAnyAmmo => magAmmo > 0 || remainAmmo > 0;


    public event Action<Gun> OnReloadStart;
    public event Action<Gun> OnReloadedEnd;
    public event Action<Gun> OnWeaponFired;

    public string Statename;

    private void Awake()
    {
        muzzleParticleSystem = GetComponentInChildren<ParticleSystem>();

        //lineRenderer.positionCount = 2;
        //lineRenderer.enabled = false;

        remainAmmo = gunData.StartRemainAmmo;
        magAmmo = gunData.MagCapacity;
    }

    private void Start()
    {
    }

    //[Rpc(SendTo.Server)]
    public override void Attack()
    {
        if (CanFire())
        {
            PerformFire();

            if(MagAmmo <= 0)
            {
                StartReloading();
            }
        }
    }

    public void AttackOnlyVFX(Vector3 targetPosition, bool FullAuto = false)
    {
        if (CanFire(FullAuto))
        {
            PerformFire(targetPosition);

            if(MagAmmo <= 0)
            {
                TryReloadStart();
            }
        }
    }

    public bool TryReloadStart()
    {
        if (CanReload())
        {
            StartReloading();
            return true;
        }
        return false;
    }

    public bool TryCancelReload()
    {
        if (IsReloading)
        {
            StopReloading();
            return true;
        }
        return false;
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
            GameObject hitObject = InGameManager.Instance.PoolManager.GetObject(gunData.HitPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            if (hitObject.TryGetComponent(out ParticleSystem hitParticle))
                hitParticle.Play();

            // Bullet ������
            GameObject bulletObject = InGameManager.Instance.PoolManager.GetObject(gunData.BulletPrefab, muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.rotation);
            if (bulletObject.TryGetComponent(out Bullet bullet))
            {
                bullet.Initialize(hit.point, 5f);
                bullet.SetVelocity(transform.forward * 100f);
            }

            // ������ ����
            if (hit.transform.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(gunData.Damage);
        }
        else
        {
            GameObject bulletObject = InGameManager.Instance.PoolManager.GetObject(gunData.BulletPrefab, muzzleParticleSystem.transform.position, muzzleParticleSystem.transform.rotation);
            if (bulletObject.TryGetComponent(out Bullet bullet))
            {
                Vector3 dest = muzzleParticleSystem.transform.position + muzzleParticleSystem.transform.forward * gunData.MaxDistance;
                bullet.Initialize(dest, 5f);
                bullet.SetVelocity(transform.forward * 100f);
            }
        }

        currentFireTime = Time.time;
        OnWeaponFired?.Invoke(this);
    }

    //NOTE : 스킬 공격용을 비롯한 특별한 공격력을 주는 사격이 필요할 때 이것을 사용. 물론 DesignatedFire 스킬은 눈속임을 위해 0데미지를 줄 예정.
    internal void PerformFire(Vector3 targetPosition)
    {
        Vector3 origin = muzzleParticleSystem.transform.position;
        Vector3 direction = targetPosition - origin;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = muzzleParticleSystem.transform.forward;
            targetPosition = origin + direction * gunData.MaxDistance;
        }
        else
        {
            direction.Normalize();
        }

        Debug.DrawRay(origin, direction * gunData.MaxDistance, Color.blue, 2f);

        GameObject bulletObject = InGameManager.Instance.PoolManager.GetObject(
            gunData.BulletPrefab,
            origin,
            Quaternion.LookRotation(direction, Vector3.up)
        );

        if (bulletObject.TryGetComponent(out Bullet bullet))
        {
            bullet.Initialize(origin + direction * gunData.MaxDistance, 5f);
            bullet.SetVelocity(direction * 100f);
        }

        --magAmmo;
        currentFireTime = Time.time;
        OnWeaponFired?.Invoke(this);
    }

    internal bool CanReload()
    {
        return !IsReloading && remainAmmo > 0 && magAmmo < gunData.MagCapacity;
    }

    public bool CanFire()
    {
        return IsReady && magAmmo > 0;
    }

    public bool CanFire(bool FullAuto)
    {
        if(FullAuto)
            return Time.time - currentFireTime >= gunData.FullAutoFireRate && magAmmo > 0;
        else
            return CanFire();
    }

    internal void PerformReload()
    {
        int neededAmmo = gunData.MagCapacity - magAmmo;
        int ammoToMove = Mathf.Min(neededAmmo, remainAmmo);

        magAmmo += ammoToMove;
        remainAmmo -= ammoToMove;
        
        OnReloadedEnd?.Invoke(this);
    }

    internal void StartReloading()
    {
        if (reloadCoroutine == null)
        {
            ClearMagAmmo();
            reloadCoroutine = StartCoroutine(ReloadingCoroutine());
            OnReloadStart?.Invoke(this);
        }
    }

    internal void StopReloading()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
    }

    private IEnumerator ReloadingCoroutine()
    {
        yield return new WaitForSeconds(gunData.ReloadTime);

        PerformReload();
        reloadCoroutine = null;
    }

    private void ClearMagAmmo()
    {
        remainAmmo += MagAmmo;
        MagAmmo = 0;
    }
}
