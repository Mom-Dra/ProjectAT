using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public enum WeaponSlot : short
    {
        None = -1,
        Primary,
        Secondary,
        Grenade
    }

    [Header("References")]
    [SerializeField] private Transform gunHolder;
    [SerializeField] private Gun nowWeapon;

    [field:SerializeField] public WeaponSlot NowWeaponSlot { get; private set; }

    [SerializeField] private Gun[] playerWeapons;

    [SerializeField] private List<GameObject> gunPrefabs = new List<GameObject>(); //임시로 만든 것. 만약에 플레이어가 원하는 무기 리스트를 선택할 수 있으면 해당 리스트를 넘겨받아 초기화 하는걸로 할수도..

    [Header("Now Gun Stats")]
    [SerializeField] private int totalDamage;
    [SerializeField] private int currentAmmo;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float range;

    #region Stat_Getters
    public int Damage { get { return totalDamage; } }
    public int CurrentAmmo { get { return currentAmmo; } }
    public int MaxAmmo { get { return maxAmmo; } }
    public float Range { get { return range; } }
    public float FireRate => nowWeapon.GunData.TimeBetFire;
    public Gun NowWeapon => nowWeapon;
    #endregion

    public Action<Gun> OnWeaponFired; //NOTE : Gun을 진짜 넘겨줘야하는지 검토 필요.
    public Action<Gun> OnWeaponChanged;
    public Action<Gun> OnWeaponReloadStart;
    public Action<Gun> OnWeaponReloaded;
    

    private void Awake()
    {
        InitiateGuns();
    }

    public void ChangeWeapon(WeaponSlot newSlot)
    {

        NowWeaponVisible(false);
        UnSubScribeWeaponEvents(nowWeapon);
        nowWeapon = playerWeapons[(int)newSlot];

        NowWeaponSlot = newSlot;
        totalDamage = CalculateTotalDamage();
        maxAmmo = CalCuateMaxAmmo(); //수정 필요
        currentAmmo = maxAmmo;       //수정 필요
        range = CalculateRange();

        SubScribeWeaponEvents(nowWeapon);
        OnWeaponChanged?.Invoke(nowWeapon);
        NowWeaponVisible(true);
    }

    public void NowWeaponVisible(bool visible)
    {
        nowWeapon.transform.GetChild(0).gameObject.SetActive(visible);
    }

    #region 초기화함수
    private int CalculateTotalDamage()
    {
        // Example calculation, modify as needed
        return nowWeapon.GunData.Damage;
    }

    private int CalCuateMaxAmmo()
    {
        // Example calculation, modify as needed
        return nowWeapon.GunData.MagCapacity;
    }

    private float CalculateRange()
    {
        // Example calculation, modify as needed
        return nowWeapon.GunData.MaxDistance;
    }
    private void InitiateGuns()
    {
        playerWeapons = new Gun[Enum.GetValues(typeof(WeaponSlot)).Length];

        for (int i = 0; i < playerWeapons.Length - 1; ++i)
        {
            playerWeapons[i] = Instantiate(gunPrefabs[i], gunHolder).GetComponent<Gun>();
            playerWeapons[i].transform.GetChild(0).gameObject.SetActive(false);
        }

        NowWeaponSlot = WeaponSlot.None;    //NOTE : 첫번째 changeWeapon()을 원활하게 실행되기 위해 None으로 설정.
        nowWeapon = playerWeapons[(int)WeaponSlot.Primary];
        ChangeWeapon(WeaponSlot.Primary);
    }

    private void SubScribeWeaponEvents(Gun gun)
    {
        gun.OnWeaponFired += WeaponFired;
        gun.OnReloadedEnd += ReloadedEnd;
        gun.OnReloadStart += WeaponReloadStart;
    }

    private void UnSubScribeWeaponEvents(Gun gun)
    {
        gun.OnWeaponFired -= WeaponFired;
        gun.OnReloadedEnd -= ReloadedEnd;
        gun.OnReloadStart -= WeaponReloadStart;
    }
    #endregion

    public void FireWeapon()
    {
        nowWeapon.Attack();
    }

    public void FireWeapon(float skillDamage, LayerMask targetLayer)
    {
        nowWeapon.Attack(skillDamage, targetLayer);
    }

    public bool CanFire()
    {
        return nowWeapon.CanFire();
    }

    public void ReloadingWeapon()
    {
        if (nowWeapon.TryReloadStart())
        {
            WeaponReloadStart(nowWeapon); //이벤트 구독자들에게 리로드 시작 알림
        }
    }

    public void CancelReloadingWeapon()
    {
        if (nowWeapon.TryCancelReload())
        {
            ReloadedEnd(nowWeapon);
        }
    }

    public bool HasAmmoInMagazine()
    {
        return currentAmmo > 0;
    }

    #region 이벤트용 함수
    private void WeaponReloadStart(Gun gun)
    {
        OnWeaponReloadStart?.Invoke(gun);
    }

    private void ReloadedEnd(Gun gun)
    {
        OnWeaponReloaded?.Invoke(gun);
    }

    private void WeaponFired(Gun gun)
    {
        OnWeaponFired?.Invoke(gun);
    }
    #endregion
}
