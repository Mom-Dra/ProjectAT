using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform gunMeshHolder;
    [SerializeField] private GameObject gunModeling;

    [SerializeField] private GunData nowWeaponData;

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
    public float FireRate => nowWeaponData.TimeBetFire;
    #endregion

    private void Awake()
    {
        SetWeapon(nowWeaponData);
    }

    public void SetWeapon(GunData newWeapon)
    {
        if(newWeapon == null) return;

        nowWeaponData = newWeapon;
        totalDamage = CalculateTotalDamage();
        maxAmmo = CalCuateMaxAmmo();
        currentAmmo = maxAmmo;
        range = CalculateRange();

        ChangeWeaponMesh();
    }

    #region 초기화 함수
    private int CalculateTotalDamage()
    {
        // Example calculation, modify as needed
        return nowWeaponData.Damage;
    }

    private int CalCuateMaxAmmo()
    {
        // Example calculation, modify as needed
        return nowWeaponData.MagCapacity;
    }

    private float CalculateRange()
    {
        // Example calculation, modify as needed
        return nowWeaponData.MaxDistance;
    }

    private void ChangeWeaponMesh()
    {
        /*if(gunModeling != null)
        {
            Destroy(gunModeling);
            gunModeling = null;
        }
        gunModeling = Instantiate(nowWeaponData.ModelingPrefab, gunMeshHolder);*/
    }
    #endregion


    public void ReloadingAmmo()
    {
        currentAmmo = maxAmmo;
    }
}
