using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private Transform meshHolder;
    [SerializeField] private Gun nowWeapon;

    [field:SerializeField] public WeaponSlot NowWeaponSlot { get; private set; }

    // [SerializeField] private Gun primaryWeapon;
    // [SerializeField] private Gun secondaryWeapon;
    // [SerializeField] private Gun grenade;
    [SerializeField] private Gun[] playerWeapons;

    [SerializeField] private List<GameObject> gunPrefabs = new List<GameObject>(); //임시로 만든 것. 만약에 플레이어가 원하는 무기 리스트를 선택할 수 있으면 해당 리스트를 넘겨받아 초기화 하는걸로 할수도..

    [Header("Stats")]
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
    public Transform GunHolderTf => meshHolder;
    #endregion

    public Action<Gun> OnWeaponFired;
    public Action<Gun> OnWeaponChanged;

    private void Awake()
    {
        InitiateGuns();
    }

    public void ChangeWeapon(WeaponSlot newSlot)
    {

        nowWeapon.transform.GetChild(0).gameObject.SetActive(false); //GetChild(0) = Gun의 MeshObject
        nowWeapon = playerWeapons[(int)newSlot];

        NowWeaponSlot = newSlot;
        totalDamage = CalculateTotalDamage();
        maxAmmo = CalCuateMaxAmmo(); //수정 필요
        currentAmmo = maxAmmo;       //수정 필요
        range = CalculateRange();

        OnWeaponChanged?.Invoke(nowWeapon);

        nowWeapon.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ChangeProjectileWeapon(WeaponSlot newSlot)
    {
        if (newSlot != WeaponSlot.Grenade) return;

        nowWeapon.transform.GetChild(0).gameObject.SetActive(false); //GetChild(0) = Gun의 MeshObject
        nowWeapon = playerWeapons[(int)newSlot];
        nowWeapon.transform.GetChild(0).gameObject.SetActive(true);
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
            playerWeapons[i] = Instantiate(gunPrefabs[i], meshHolder).GetComponent<Gun>();
            playerWeapons[i].transform.GetChild(0).gameObject.SetActive(false);
        }

        // playerWeapons[(int)WeaponSlot.Primary] = Instantiate(gunPrefabs[0], meshHolder).GetComponent<Gun>();
        // playerWeapons[(int)WeaponSlot.Secondary] = Instantiate(gunPrefabs[1], meshHolder).GetComponent<Gun>();
        // playerWeapons[(int)WeaponSlot.Grenade] = Instantiate(gunPrefabs[2], meshHolder).GetComponent<Gun>();

        // primaryWeapon = Instantiate(gunPrefabs[0], meshHolder).GetComponent<Gun>();
        // //secondaryWeapon = Instantiate(gunPrefabs[1], meshHolder).GetComponent<Gun>();
        // grenade = Instantiate(gunPrefabs[1], meshHolder).GetComponent<Gun>();

        // primaryWeapon.gameObject.SetActive(false);
        // //secondaryWeapon.gameObject.SetActive(false);
        // grenade.gameObject.SetActive(false);

        NowWeaponSlot = WeaponSlot.None; //첫번째 changeWeapon()을 원활하게 실행되기 위해 None으로 설정.
        nowWeapon = playerWeapons[(int)WeaponSlot.Primary];
        ChangeWeapon(WeaponSlot.Primary);
    }

    #endregion

    public void FireWeapon()
    {
        nowWeapon.Attack();
        OnWeaponFired?.Invoke(nowWeapon);
    }

    // public void SpecialFireWeapon()
    // {
    //     nowWeapon.PerformSpecialFire();
    //     OnWeaponFired?.Invoke(nowWeapon);
    // }

    // public void ReloadingAmmo()
    // {
    //     currentAmmo = maxAmmo;
    // }

    public bool IsAmmoLoaded()
    {
        return nowWeapon.RemainAmmo > 0;
    }
}
