using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{

    public enum WeaponSlot : ushort
    {
        Primary,
        Secondary,
        Grenade
    }

    [Header("References")]
    [SerializeField] private Transform meshHolder;
    [SerializeField] private Gun nowWeapon;

    [SerializeField] private Gun primaryWeapon;
    [SerializeField] private Gun secondaryWeapon;
    [SerializeField] private Gun grenade;

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
    #endregion

    private void Awake()
    {
        InitiateGuns();
        ChangeWeapon(WeaponSlot.Primary); // Default Weapon Set
    }

    public void ChangeWeapon(WeaponSlot type)
    {
        Gun newWeapon = null;
        switch (type)
        {
            case WeaponSlot.Primary:
                newWeapon = primaryWeapon;
                break;
            case WeaponSlot.Secondary:
                newWeapon = primaryWeapon; //primaryWeapon에서 secondaryWeapon으로 나중에 바꾸기. secondaryWeapon 미구현되서 이대로 둠
                break;
            case WeaponSlot.Grenade:
                newWeapon = grenade;
                break;
        }

        nowWeapon.gameObject.SetActive(false);

        nowWeapon = newWeapon;
        totalDamage = CalculateTotalDamage();
        maxAmmo = CalCuateMaxAmmo(); //수정 필요
        currentAmmo = maxAmmo;       //수정 필요
        range = CalculateRange();
        
        nowWeapon.gameObject.SetActive(true);
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
        primaryWeapon = Instantiate(gunPrefabs[0], meshHolder).GetComponent<Gun>();
        //secondaryWeapon = Instantiate(gunPrefabs[1], meshHolder).GetComponent<Gun>();
        grenade = Instantiate(gunPrefabs[1], meshHolder).GetComponent<Gun>();
        
        primaryWeapon.gameObject.SetActive(false);
        //secondaryWeapon.gameObject.SetActive(false);
        grenade.gameObject.SetActive(false);

        nowWeapon = primaryWeapon;
    }

    #endregion


    public void ReloadingAmmo()
    {
        currentAmmo = maxAmmo;
    }

    public bool IsAmmoLoaded()
    {
        return currentAmmo > 0;
    }
}
