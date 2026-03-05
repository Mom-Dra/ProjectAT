using System.Collections;
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
    private VisualElement[] skillInfos = new VisualElement[5];
    private VisualElement[] skillIcons = new VisualElement[5];
    private CooldownOverlay[] skillCooldownOverlays = new CooldownOverlay[5]; // 스킬 쿨타임 오버레이 UI 요소 배열
    private Label[] skillItemLabels = new Label[5]; // 스킬 아이템 개수 UI 요소 배열

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


        for(int i = 0 ; i < skillInfos.Length; i++)
        {
            skillInfos[i] = root.Q<VisualElement>($"SkillInfo_{i}");
            skillIcons[i] = skillInfos[i].Q<VisualElement>("Icon");
            skillItemLabels[i] = skillInfos[i].Q<Label>("ItemCount");
            skillCooldownOverlays[i] = skillInfos[i].Q<CooldownOverlay>("SkillCoolDown");
        }
        // skillInfos[(int)SkillNumber.DesignatedFire] = root.Q<VisualElement>("DesignateFire");
        // skillInfos[(int)SkillNumber.UseBandage] = root.Q<VisualElement>("UsingBanadge");
        // skillInfos[(int)SkillNumber.Grenade] = root.Q<VisualElement>("GrenadeThrow");
        // skillInfos[(int)SkillNumber.MainSkillOne] = root.Q<VisualElement>("MainSkillOne");
        // skillInfos[(int)SkillNumber.MainSkillTwo] = root.Q<VisualElement>("MainSkillTwo");

        // skillIcons[(int)SkillNumber.DesignatedFire] = skillInfos[(int)SkillNumber.DesignatedFire].Q<VisualElement>("Icon");
        // skillIcons[(int)SkillNumber.UseBandage] = skillInfos[(int)SkillNumber.UseBandage].Q<VisualElement>("Icon");
        // skillIcons[(int)SkillNumber.Grenade] = skillInfos[(int)SkillNumber.Grenade].Q<VisualElement>("Icon");
        // skillIcons[(int)SkillNumber.MainSkillOne] = skillInfos[(int)SkillNumber.MainSkillOne].Q<VisualElement>("Icon");
        // skillIcons[(int)SkillNumber.MainSkillTwo] = skillInfos[(int)SkillNumber.MainSkillTwo].Q<VisualElement>("Icon");

        // skillItemLabels[(int)SkillNumber.DesignatedFire] = skillInfos[(int)SkillNumber.DesignatedFire].Q<Label>("ItemCount");
        // skillItemLabels[(int)SkillNumber.UseBandage] = skillInfos[(int)SkillNumber.UseBandage].Q<Label>("ItemCount");
        // skillItemLabels[(int)SkillNumber.Grenade] = skillInfos[(int)SkillNumber.Grenade].Q<Label>("ItemCount");
        // skillItemLabels[(int)SkillNumber.MainSkillOne] = skillInfos[(int)SkillNumber.MainSkillOne].Q<Label>("ItemCount");
        // skillItemLabels[(int)SkillNumber.MainSkillTwo] = skillInfos[(int)SkillNumber.MainSkillTwo].Q<Label>("ItemCount");


        // skillCooldownOverlays[(int)SkillNumber.DesignatedFire] = skillInfos[(int)SkillNumber.DesignatedFire].Q<CooldownOverlay>("SkillCoolDown");
        // skillCooldownOverlays[(int)SkillNumber.UseBandage] = skillInfos[(int)SkillNumber.UseBandage].Q<CooldownOverlay>("SkillCoolDown");
        // skillCooldownOverlays[(int)SkillNumber.Grenade] = skillInfos[(int)SkillNumber.Grenade].Q<CooldownOverlay>("SkillCoolDown");
        // skillCooldownOverlays[(int)SkillNumber.MainSkillOne] = skillInfos[(int)SkillNumber.MainSkillOne].Q<CooldownOverlay>("SkillCoolDown");
        // skillCooldownOverlays[(int)SkillNumber.MainSkillTwo] = skillInfos[(int)SkillNumber.MainSkillTwo].Q<CooldownOverlay>("SkillCoolDown");
    }

    public void SetPlayerPortrait(Sprite portrait)
    {
        // 플레이어 초상화 설정 로직 (예: Image 컴포넌트에 Sprite 할당)
        if (_playerPortrait != null)
        {
            _playerPortrait.style.backgroundImage = new StyleBackground(portrait);
        }
    }

    public void SetPlayerHealthUI(float healthRatio)
    {
        if (_healthBar != null)
        {
            // ProgressBar의 값 설정
            _healthBar.Progress = healthRatio * 100f;
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
        SetPlayerAmmoText(gun.MagAmmo, gun.RemainAmmo);
    }

    public void SetPlayerAmmoText(int currentAmmo, int maxAmmo)
    {
        _playerAmmoText.text = $"{currentAmmo} / {maxAmmo}";
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
            if(skillDatas[i] is not ConsumableSkillData)
            {
                skillItemLabels[i].style.display = DisplayStyle.None;
            }
            skillIcons[i].style.backgroundImage = new StyleBackground(skillDatas[i].SkillIcon);
        }
    }

    public void BindPlayerSkillEvent(PlayerSkillModule playerSkillModule)
    {
        for(int i = 0; i < skillIcons.Length; i++)
        {
            BindSkillClickEvent(skillIcons[i], playerSkillModule, (SkillNumber)i);
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

    public void StartSkillCooldown(SkillNumber index, float cooldownDuration)
    {
        StartCoroutine(CooldownCoroutine(index, cooldownDuration));
    }

    private IEnumerator CooldownCoroutine(SkillNumber index, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetSkillCooldown(index, elapsed / duration);
            yield return null;
        }
        SetSkillCooldown(index, 1f);
    }

    private void SetSkillCooldown(SkillNumber index, float cooldownProgress)
    {
        if (skillCooldownOverlays[(int)index] != null)
        {
            skillCooldownOverlays[(int)index].FillAmount = 1f - cooldownProgress; // 투명도를 조절
        }
    }

    public void SetSkillItemText(SkillNumber index, int itemCount)
    {
        if(skillItemLabels[(int)index] != null)
        {
            if(itemCount < 0)
            {
                skillInfos[(int)index].SetEnabled(false);
            }
            else
            {
                skillInfos[(int)index].SetEnabled(true);
            }
            skillItemLabels[(int)index].text = itemCount.ToString();
        }
    }
}