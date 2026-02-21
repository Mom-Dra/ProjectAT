using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class UIManager
{
    private GameObject healthUIPrefab;
    private PlayerHUD playerHUD;

    public UIManager(GameObject healthUIPrefab, PlayerHUD playerHUD)
    {
        if (healthUIPrefab is null)
            Debug.LogError("healthUIPrefab is null");

        this.healthUIPrefab = healthUIPrefab;
        this.playerHUD = playerHUD;
    }

    public void ShowHealthUI(EntityStatus entityStatus)
    {
        GameObject healthUIObject = Managers.Instance.PoolManager.GetObject(healthUIPrefab);
        HealthUI healthUI = healthUIObject.GetComponentInChildren<HealthUI>();
        healthUI.Bind(entityStatus.transform, entityStatus);
    }

    public void HideHealthUI(HealthUI healthUI)
    {
        healthUI.UnBind();
        Managers.Instance.PoolManager.ReturnObject(healthUI.transform.parent.gameObject, healthUIPrefab);
    }

#region  플레이어 HUD 관련
    public void SetPlayerHealthUI(float currentHealth, float maxHealth)
    {
        playerHUD.SetHealthUI(currentHealth, maxHealth);
    }

    public void SetPlayerAmmoUI(int ammo, int maxAmmo)
    {
        playerHUD.SetPlayerAmmoText(ammo, maxAmmo);
    }

    public void InitPlayerStatusInfo(EntityStatus playerStatus)
    {
        if(playerStatus is null)
        {
            Debug.LogError("PlayerInitialStatusData is null!");
            return;
        }

        playerHUD.SetPlayerPortrait(playerStatus.InitStatusRef.PortatitSprite);
        playerHUD.SetHealthUI(playerStatus.CurrentHp, playerStatus.MaxHp);
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

    public void InitPlayerSkillInfo(SkillData[] skillDatas)
    {
        playerHUD.SetPlayerSkillInfo(skillDatas);
    }
# endregion
}
