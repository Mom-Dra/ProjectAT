using Unity.Netcode;
using UnityEngine;

public class WeaponStatus : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer myMesh;

    [Header("Stats")]
    [SerializeField] private int damage;
    [SerializeField] private int currAmmo;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float radius;

    #region Stat_Getters
    public int Damage { get { return damage; } }
    public int CurrAmmo { get { return currAmmo; } }
    public int MaxAmmo { get { return maxAmmo; } }
    public float Radius { get { return radius; } }
    #endregion

    public void ReloadingAmmo()
    {
        currAmmo = maxAmmo;
    }
}
