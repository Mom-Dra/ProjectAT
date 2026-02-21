using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]private UIDocument _uiDocument;
    private RadialProgressBar _healthBar; // ProgressBar 타입 사용
    private VisualElement _playerPortrait; // 플레이어 초상화 UI 요소
    
    [Header("Weapon Info UI Elements")]
    private VisualElement _playerWeaponIcon; // 플레이어 무기 정보 UI 요소
    private Label _playerAmmoText; // 플레이어 탄약 정보 UI 요소

    [Header("Skill Info UI Elements")]
    private VisualElement[] skillInfos = new VisualElement[3];
    private CooldownOverlay[] skillCooldownOverlays = new CooldownOverlay[3]; // 스킬 쿨타임 오버레이 UI 요소 배열

    public int SkillInfoCount => skillInfos.Length;

    //private VisualElement _designatedFireSkill; // 플레이어 초상화 UI 요소
    //private VisualElement _bandageSkill; // 플레이어 초상화 UI 요소


    // [Header("Player Stats")]
    // [SerializeField]private int maxAmmo = 30;
    // [SerializeField]private float maxHealth = 100f;

    // [Header("Player Instance Reference")]
    // [SerializeField] private PlayerController controller;

    // [Header("Test Variables")]
    // // 테스트용 변수 (인스펙터에서 조절해보세요)
    // public float currentHealth = 100f;
    // public int currentAmmo = 30;
    // public Sprite playerPortraitSprite;

    void OnEnable()
    {
        InitUIElements();
    }

    void Update()
    {
        // 실제 게임에선 맞았을 때만 호출하겠지만, 테스트를 위해 Update에 둡니다.
        //UpdateCurrentHealthUI();
        //SetPlayerAmmoText(currentAmmo);
    }

    private void InitUIElements()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        // UI Builder에서 지은 이름 "HealthBar"로 찾기
        _healthBar = root.Q<RadialProgressBar>("HealthBar");
        _playerPortrait = root.Q<VisualElement>("Portrait");

        _playerWeaponIcon = root.Q<VisualElement>("WeaponIcon");
        _playerAmmoText = root.Q<Label>("AmmoText");

        skillInfos[(int)SkillNumber.DesignatedFire] = root.Q<VisualElement>("DesignateFire").Q<VisualElement>("Icon");
        skillInfos[(int)SkillNumber.UseBandage] = root.Q<VisualElement>("UsingBanadge").Q<VisualElement>("Icon");
        skillInfos[(int)SkillNumber.Grenade] = root.Q<VisualElement>("GrenadeThrow").Q<VisualElement>("Icon");

        skillCooldownOverlays[(int)SkillNumber.DesignatedFire] = skillInfos[(int)SkillNumber.DesignatedFire].Q<CooldownOverlay>("SkillCoolDown");
        skillCooldownOverlays[(int)SkillNumber.UseBandage] = skillInfos[(int)SkillNumber.UseBandage].Q<CooldownOverlay>("SkillCoolDown");
        skillCooldownOverlays[(int)SkillNumber.Grenade] = skillInfos[(int)SkillNumber.Grenade].Q<CooldownOverlay>("SkillCoolDown");
    }

    public void SetPlayerPortrait(Sprite portrait)
    {
        // 플레이어 초상화 설정 로직 (예: Image 컴포넌트에 Sprite 할당)
        if (_playerPortrait != null)
        {
            _playerPortrait.style.backgroundImage = new StyleBackground(portrait);
        }
    }

    public void SetPlayerHealthUI(float currentHealth, float maxHealth)
    {
        if (_healthBar != null)
        {
            // ProgressBar의 값 설정
            _healthBar.Progress = (currentHealth / maxHealth) * 100f;
        }
        else
        {
            Debug.LogWarning("HealthBar UI element not found!");
        }
    }

    public void SetPlayerWeaponInfo(Gun gun)
    {
        if (gun is null)
        {
            Debug.LogError("GunData is null!");
            return;
        }
        _playerWeaponIcon.style.backgroundImage = new StyleBackground(gun.GunData.GunIcon);
        SetPlayerAmmoText(gun.RemainAmmo, gun.MagAmmo);
    }

    public void SetPlayerAmmoText(int ammo, int maxAmmo)
    {
        // 플레이어 탄약 텍스트 설정 로직 (예: Label 컴포넌트에 텍스트 할당)
        if (_playerAmmoText != null)
        {
            _playerAmmoText.text = $"{ammo} / {maxAmmo}";
        }
    }

    public void SetPlayerSkillInfo(SkillData[] skillDatas)
    {
        // 플레이어 스킬 정보 설정 로직 (예: 각 스킬 아이콘 업데이트)
        if (skillDatas == null)
        {
            Debug.LogError("Skill data is null or insufficient!");
            return;
        }

        for(int i = 0 ; i< skillDatas.Length; i++)
        {
            if(skillDatas[i] == null)
            {
                Debug.LogError($"Skill data for skill index {i} is null!");
                continue;
            }
            skillInfos[i].style.backgroundImage = new StyleBackground(skillDatas[i].SkillIcon);
        }
        //skillInfos[(int)SkillNumber.DesignatedFire].style.backgroundImage = new StyleBackground(skillDatas[0].SkillIcon);
        //skillInfos[(int)SkillNumber.UseBandage].style.backgroundImage = new StyleBackground(skillDatas[1].SkillIcon);
        //skillInfos[(int)SkillNumber.Grenade].style.backgroundImage = new StyleBackground(skillDatas[2].SkillIcon);
    }

    public void BindPlayerSkillEvent(PlayerSkillModule playerSkillModule)
    {
        for(int i = 0; i < skillInfos.Length; i++)
        {
            BindSkillClickEvent(skillInfos[i], playerSkillModule, (SkillNumber)i);

        }
        // BindSkillClickEvent(skillInfos[(int)SkillNumber.DesignatedFire], playerSkillModule, SkillNumber.DesignatedFire);
        // BindSkillClickEvent(skillInfos[(int)SkillNumber.UseBandage], playerSkillModule, SkillNumber.UseBandage);
        // BindSkillClickEvent(skillInfos[(int)SkillNumber.Grenade], playerSkillModule, SkillNumber.Grenade);
    }

    private void BindSkillClickEvent(VisualElement iconElement, PlayerSkillModule playerSkillModule, SkillNumber index)
    {
        if(iconElement is not null && playerSkillModule is not null)
        {
            iconElement.RegisterCallback<ClickEvent>(evt=>
            {
                playerSkillModule.ActivateTargettingMode(index);
            }
            );
        }
    }

    public void SetSkillCooldown(SkillNumber index, float cooldownProgress)
    {
        if (skillCooldownOverlays[(int)index] != null)
        {
            Debug.Log($"Setting cooldown for skill {index}: {cooldownProgress}");
            skillCooldownOverlays[(int)index].FillAmount = 1f - cooldownProgress; // 예시로 투명도를 조절
        }
        else
        {
            Debug.LogWarning($"Cooldown overlay for skill {index} not found!");
        }
    }
}