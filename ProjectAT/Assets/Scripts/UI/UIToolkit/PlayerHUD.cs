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
    //private VisualElement _designatedFireSkill; // 플레이어 초상화 UI 요소
    //private VisualElement _bandageSkill; // 플레이어 초상화 UI 요소


    [Header("Player Stats")]
    [SerializeField]private int maxAmmo = 30;
    [SerializeField]private float maxHealth = 100f;

    [Header("Test Variables")]
    // 테스트용 변수 (인스펙터에서 조절해보세요)
    public float currentHealth = 100f;
    public int currentAmmo = 30;
    public Sprite playerPortraitSprite;

    void OnEnable()
    {
        InitUIElements();
        InitPlayerInfo();
        InitPlayerSkill();
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
    }

    private void InitPlayerInfo()
    {
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;
        SetPlayerPortrait(playerPortraitSprite);
    }

    private void InitPlayerSkill()
    {
        
    }

    public void SetPlayerPortrait(Sprite portrait)
    {
        // 플레이어 초상화 설정 로직 (예: Image 컴포넌트에 Sprite 할당)
        if (_playerPortrait != null)
        {
            _playerPortrait.style.backgroundImage = new StyleBackground(portrait);
        }
    }

    public void SetHealthUI(float currentHealth, float maxHealth)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        UpdateCurrentHealthUI();
    }

    public void UpdateCurrentHealthUI()
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
            _playerAmmoText.text = $"{gun.RemainAmmo} / {gun.MagAmmo}";        
     
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

    public void SetPlayerAmmoText(int ammo, int maxAmmo)
    {
        // 플레이어 탄약 텍스트 설정 로직 (예: Label 컴포넌트에 텍스트 할당)
        if (_playerAmmoText != null)
        {
            this.maxAmmo = maxAmmo; 
            _playerAmmoText.text = $"{ammo} / {maxAmmo}";
        }
    }
}