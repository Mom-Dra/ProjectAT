using Biostart.Enemy;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class UIManager
{
    private GameObject healthUIPrefab;
    private PlayerHUD playerHUD;
    private PlayerSkillModule playerSkillModule;

    public UIManager(GameObject healthUIPrefab, PlayerHUD playerHUD)
    {
        if (healthUIPrefab is null)
            Debug.LogError("healthUIPrefab is null");

        this.healthUIPrefab = healthUIPrefab;
        this.playerHUD = playerHUD;
    }

    public void SetPlayerHUD(PlayerHUD playerHUD)
    {
        this.playerHUD = playerHUD;
    }

    public HealthUI ShowHealthUI(Transform targetAnchor, EntityStatus entityStatus)
    {
        GameObject healthUIObject = Managers.Instance.PoolManager.GetObject(healthUIPrefab);
        HealthUI healthUI = healthUIObject.GetComponentInChildren<HealthUI>();
        healthUI.Bind(targetAnchor, entityStatus);

        return healthUI;
    }

    public void HideHealthUI(HealthUI healthUI)
    {
        healthUI.UnBind();
        Managers.Instance.PoolManager.ReturnObject(healthUI.transform.gameObject, healthUIPrefab);
    }

    #region  플레이어 HUD - Status
    public void InitPlayerStatusInfo(EntityStatus playerStatus)
    {
        if (playerStatus is null)
        {
            Debug.LogError("PlayerInitialStatusData is null!");
            return;
        }

        playerHUD.SetPlayerPortrait(playerStatus.InitStatusRef.PortatitSprite);
        SetPlayerHealthUI((float)playerStatus.CurrentHp / playerStatus.MaxHp);
        playerStatus.onHealthChanged += SetPlayerHealthUI;
    }

    public void SetPlayerHealthUI(float healthRatio)
    {
        playerHUD.SetPlayerHealthUI(healthRatio);
    }

    public void InitPlayerGunInfo(WeaponHolder myGunHolder)
    {
        if (myGunHolder.NowWeapon is null)
        {
            Debug.LogError("Player GunData is null!");
            return;
        }
        playerHUD.SetPlayerWeaponInfo(myGunHolder.NowWeapon);
        myGunHolder.OnWeaponFired += UpdatePlayerAmmoUI;
    }

    public void UpdatePlayerAmmoUI(Gun gun)
    {
        if (gun is null)
        {
            Debug.LogError("GunData is null!");
            return;
        }
        playerHUD.SetPlayerAmmoText(gun.MagAmmo, gun.RemainAmmo);
    }

    public void SetPlayerAmmoUI(int currentAmmo, int maxAmmo)
    {
        playerHUD.SetPlayerAmmoText(currentAmmo, maxAmmo);
    }
    #endregion

    #region  플레이어 HUD - Skills
    public void InitPlayerSkillInfo(PlayerSkillModule playerSkillModule, SkillData[] skillDatas)
    {
        this.playerSkillModule = playerSkillModule;
        playerHUD.SetPlayerSkillInfo(skillDatas);
        playerHUD.BindPlayerSkillEvent(playerSkillModule);
        playerSkillModule.OnSkillCooldownStart += StartSkillCooldown;
        playerSkillModule.OnSkillItemCountChange += SetSkillItemCount;
    }

    private void StartSkillCooldown(SkillNumber skillNumber, float cooldownPercent)
    {
        playerHUD.StartSkillCooldown(skillNumber, cooldownPercent);
    }

    public void SetSkillItemCount(SkillNumber skillNumber, int itemCount)
    {
        playerHUD.SetSkillItemText(skillNumber, itemCount);
    }


    # endregion
}