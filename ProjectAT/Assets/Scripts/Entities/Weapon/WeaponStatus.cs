using Unity.Netcode;
using UnityEngine;

public class WeaponStatus : MonoBehaviour
{
    //It's Deprecated Script. Don't use it.

    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer myMesh;
    [SerializeField] private GunData nowWeapon;

    [Header("Stats")]
    [SerializeField] private int damage;
    [SerializeField] private int currAmmo;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float range;
    
    
    #region Stat_Getters
    public int Damage { get { return damage; } }
    public int CurrAmmo { get { return currAmmo; } }
    public int MaxAmmo { get { return maxAmmo; } }
    public float Range { get { return range; } }
    #endregion

    
    public void ReloadingAmmo()
    {
        currAmmo = maxAmmo;
    }
}
