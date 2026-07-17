using UnityEngine;

public class InGameUIManager
{
    private readonly GameObject healthUIPrefab;
    private PlayerHUD playerHUD;

    public InGameUIManager(GameObject healthUIPrefab, PlayerHUD playerHUD)
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
        if (healthUI is null) return;

        healthUI.UnBind();
        Managers.Instance.PoolManager.ReturnObject(healthUI.transform.gameObject, healthUIPrefab);
    }

    public void InitPlayerStatusInfo(EntityStatus playerStatus)
    {
        if (playerStatus is null)
        {
            Debug.LogError("PlayerInitialStatusData is null!");
            return;
        }

        if (playerHUD is null)
        {
            Debug.LogError("PlayerHUD is null!");
            return;
        }

        playerHUD.SetPlayerPortrait(playerStatus.InitStatusRef.PortatitSprite);
        SetPlayerHealthUI((float)playerStatus.CurrentHp / playerStatus.MaxHp);
        playerStatus.onHealthChanged += SetPlayerHealthUI;

        if (playerStatus.TryGetComponent(out BuffModule buffModule))
        {
            playerHUD.BindBuffModule(buffModule);
        }
        else
        {
            playerHUD.BindBuffModule(null);
        }
    }

    public void SetPlayerHealthUI(float healthRatio)
    {
        if (playerHUD is null)
            return;

        playerHUD.SetPlayerHealthUI(healthRatio);
    }

    public void InitPlayerGunInfo(WeaponHolder myGunHolder)
    {
        if (playerHUD is null)
        {
            Debug.LogError("PlayerHUD is null!");
            return;
        }

        if (myGunHolder.NowWeapon is null)
        {
            Debug.LogError("Player GunData is null!");
            return;
        }

        playerHUD.SetPlayerWeaponInfo(myGunHolder.NowWeapon);
        myGunHolder.OnWeaponFired += UpdatePlayerAmmoUI;
        myGunHolder.OnWeaponReloadStart += UpdatePlayerAmmoUI;
        myGunHolder.OnWeaponReloaded += UpdatePlayerAmmoUI;
    }

    public void UpdatePlayerAmmoUI(Gun gun)
    {
        if (playerHUD is null)
            return;

        if (gun is null)
        {
            Debug.LogError("GunData is null!");
            return;
        }

        playerHUD.SetPlayerAmmoText(gun.MagAmmo, gun.RemainAmmo);
    }

    public void SetPlayerAmmoUI(int currentAmmo, int maxAmmo)
    {
        if (playerHUD is null)
            return;

        playerHUD.SetPlayerAmmoText(currentAmmo, maxAmmo);
    }

    public void InitPlayerSkillInfo(PlayerSkillModule playerSkillModule, SkillData[] skillDatas)
    {
        if (playerHUD is null)
        {
            Debug.LogError("PlayerHUD is null!");
            return;
        }

        playerHUD.SetPlayerSkillInfo(skillDatas);
        playerHUD.BindPlayerSkillEvent(playerSkillModule);
        playerSkillModule.OnSkillCooldownStart += StartSkillCooldown;
        playerSkillModule.OnSkillItemCountChange += SetSkillItemCount;
    }

    private void StartSkillCooldown(SkillNumber skillNumber, float cooldownDuraion)
    {
        if (playerHUD is null)
            return;

        playerHUD.StartSkillCooldown(skillNumber, cooldownDuraion);
    }

    public void SetSkillItemCount(SkillNumber skillNumber, int itemCount)
    {
        if (playerHUD is null)
            return;

        playerHUD.SetSkillItemText(skillNumber, itemCount);
    }
}
