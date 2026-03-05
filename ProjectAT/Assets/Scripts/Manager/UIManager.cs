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
        if(playerStatus is null)
        {
            Debug.LogError("PlayerInitialStatusData is null!");
            return;
        }

        playerHUD.SetPlayerPortrait(playerStatus.InitStatusRef.PortatitSprite);
        SetPlayerHealthUI(playerStatus.CurrentHp, playerStatus.MaxHp);
    }

    public void SetPlayerHealthUI(float currentHealth, float maxHealth)
    {
        playerHUD.SetPlayerHealthUI(currentHealth, maxHealth);
    }


    public void InitPlayerGunInfo(Gun gunData)
    {
        if(gunData is null)
        {
            Debug.LogError("Player GunData is null!");
            return;
        }
        playerHUD.SetPlayerWeaponInfo(gunData);
    }

    public void SetPlayerAmmoUI(int ammo, int maxAmmo)
    {
        playerHUD.SetPlayerAmmoText(ammo, maxAmmo);
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