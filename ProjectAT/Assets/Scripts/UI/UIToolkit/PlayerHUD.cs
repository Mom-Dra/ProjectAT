using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using SkillDataOptionInterfaces;
using Unity.AppUI.UI;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private UIDocument _uiDocument;
    private RadialProgressBar _healthBar; // ProgressBar 타입 사용
    private VisualElement _playerPortrait; // 플레이어 초상화 UI 요소

    [Header("Weapon Info UI Elements")]
    private VisualElement _playerWeaponIcon; // 플레이어 무기 정보 UI 요소
    private Label _playerAmmoText; // 플레이어 탄약 정보 UI 요소

    [Header("Skill Info UI Elements")]
    private readonly VisualElement[] skillInfos = new VisualElement[5];
    private readonly VisualElement[] skillIcons = new VisualElement[5];
    private readonly CooldownOverlay[] skillCooldownOverlays = new CooldownOverlay[5];
    private readonly Label[] skillItemLabels = new Label[5];

    [Header("Buff Info UI Elements")]
    private VisualElement buffList;
    private BuffModule boundBuffModule;
    private readonly Dictionary<BuffInstance, BuffSlotView> buffSlotViews = new Dictionary<BuffInstance, BuffSlotView>();

    public int SkillInfoCount => skillInfos.Length;

    private void Awake()
    {
        Managers.Instance.UIManager.SetPlayerHUD(this);
    }

    private void OnEnable()
    {
        InitUIElements();
    }

    private void OnDisable()
    {
        BindBuffModule(null);
    }

    private void Update()
    {
        UpdateBuffSlots();
    }

    private void InitUIElements()
    {
        _uiDocument = GetComponent<UIDocument>();
        VisualElement root = _uiDocument.rootVisualElement;

        // UI Builder에서 지은 이름 "HealthBar"로 찾기
        _healthBar = root.Q<RadialProgressBar>("HealthBar"); CheckUIElement(_healthBar, "HealthBar");
        _playerPortrait = root.Q<VisualElement>("Portrait"); CheckUIElement(_playerPortrait, "Portrait");

        _playerWeaponIcon = root.Q<VisualElement>("WeaponIcon"); CheckUIElement(_playerWeaponIcon, "WeaponIcon");
        _playerAmmoText = root.Q<Label>("AmmoText"); CheckUIElement(_playerAmmoText, "AmmoText");

        buffList = root.Q<VisualElement>("BuffList");
        if (buffList == null)
        {
            buffList = new VisualElement { name = "BuffList" };
            root.Add(buffList);
            ConfigureBuffListStyle();
        }

        for (int i = 0; i < skillInfos.Length; i++)
        {
            skillInfos[i] = root.Q<VisualElement>($"SkillInfo_{i}"); CheckUIElement(skillInfos[i], $"SkillInfo_{i}");
            skillIcons[i] = skillInfos[i].Q<VisualElement>("Icon"); CheckUIElement(skillIcons[i], $"SkillInfo_{i} Icon");
            skillItemLabels[i] = skillInfos[i].Q<Label>("ItemCount"); CheckUIElement(skillItemLabels[i], $"SkillInfo_{i} ItemCount");
            skillCooldownOverlays[i] = skillInfos[i].Q<CooldownOverlay>("SkillCoolDown"); CheckUIElement(skillCooldownOverlays[i], $"SkillInfo_{i} SkillCoolDown");
        }

        RebuildBuffSlots();
    }

    private void CheckUIElement(VisualElement element, string elementName)
    {
        if (element == null)
        {
            Debug.LogError($"{elementName} UI element not found!");
        }
    }

    public void SetPlayerPortrait(Sprite portrait)
    {
        // 플레이어 초상화 설정 로직 (예: Image 컴포넌트에 Sprite 할당)
        if (_playerPortrait != null)
        {
            Debug.Log("SetPlayerPortrait");
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

        for (int i = 0; i < skillDatas.Length; i++)
        {
            if (skillDatas[i] == null)
            {
                Debug.LogError($"Skill data for skill index {i} is null!");
                continue;
            }

            if (skillDatas[i] is not IConsumableSkillData)
            {
                skillItemLabels[i].style.display = DisplayStyle.None;
            }

            skillIcons[i].style.backgroundImage = new StyleBackground(skillDatas[i].SkillIcon);
        }
    }

    public void BindPlayerSkillEvent(PlayerSkillModule playerSkillModule)
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            BindSkillClickEvent(skillIcons[i], playerSkillModule, (SkillNumber)i);
        }
    }

    private void BindSkillClickEvent(VisualElement iconElement, PlayerSkillModule playerSkillModule, SkillNumber index)
    {
        if (iconElement is not null && playerSkillModule is not null)
        {
            iconElement.RegisterCallback<ClickEvent>(evt =>
            {
                playerSkillModule.ActivateTargettingMode(index);
            });
        }
    }

    public void StartSkillCooldown(SkillNumber index, float cooldownDuration)
    {
        if (cooldownDuration < 0f)
        {
            DisableSkillInfo(index);
            return;
        }
        StartCoroutine(CooldownCoroutine(index, cooldownDuration));
    }

    private IEnumerator CooldownCoroutine(SkillNumber index, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetSkillClockWipe(index, elapsed / duration);

            yield return null;
        }

        SetSkillClockWipe(index, 1f);
    }

    private void SetSkillClockWipe(SkillNumber index, float cooldownProgress)
    {
        if (skillCooldownOverlays[(int)index] != null)
        {
            skillCooldownOverlays[(int)index].FillAmount = 1f - cooldownProgress; // 투명도를 조절
        }
    }

    public void SetSkillItemText(SkillNumber index, int itemCount)
    {
        if (skillItemLabels[(int)index] != null)
        {
            skillItemLabels[(int)index].text = itemCount.ToString();
        }
    }

    public void DisableSkillInfo(SkillNumber index)
    {
        if (skillInfos[(int)index] != null)
        {
            SetSkillClockWipe(index, 0f);
        }

        if (skillItemLabels[(int)index] != null)
        {
            skillItemLabels[(int)index].visible = false;
        }
    }

    public void BindBuffModule(BuffModule buffModule)
    {
        if (boundBuffModule == buffModule)
        {
            RebuildBuffSlots();
            return;
        }

        if (boundBuffModule != null)
        {
            boundBuffModule.OnBuffsChanged -= RebuildBuffSlots;
        }

        boundBuffModule = buffModule;

        if (boundBuffModule != null)
        {
            boundBuffModule.OnBuffsChanged += RebuildBuffSlots;
        }

        RebuildBuffSlots();
    }

    private void RebuildBuffSlots()
    {
        buffSlotViews.Clear();

        if (buffList == null)
        {
            return;
        }

        buffList.Clear();

        if (boundBuffModule == null)
        {
            return;
        }

        foreach (BuffInstance buffInstance in boundBuffModule.ActiveBuffs)
        {
            BuffSlotView slotView = CreateBuffSlot(buffInstance);
            buffSlotViews.Add(buffInstance, slotView);
            buffList.Add(slotView.Root);
        }

        UpdateBuffSlots();
    }

    private BuffSlotView CreateBuffSlot(BuffInstance buffInstance)
    {
        VisualElement root = new VisualElement
        {
            name = $"Buff_{buffInstance.BuffName}",
            pickingMode = PickingMode.Ignore
        };
        root.tooltip = buffInstance.BuffName;
        root.style.position = Position.Relative;
        root.style.width = 48;
        root.style.height = 48;
        root.style.marginRight = 6;
        root.style.flexShrink = 0;
        root.style.backgroundColor = new Color(0f, 0f, 0f, 0.6f);

        VisualElement icon = new VisualElement { pickingMode = PickingMode.Ignore };
        icon.style.position = Position.Absolute;
        icon.style.left = 0;
        icon.style.right = 0;
        icon.style.top = 0;
        icon.style.bottom = 0;
        icon.style.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);
        if (buffInstance.Icon != null)
        {
            icon.style.backgroundImage = new StyleBackground(buffInstance.Icon);
        }

        CooldownOverlay overlay = new CooldownOverlay
        {
            pickingMode = PickingMode.Ignore,
            OverlayColor = new Color(0f, 0f, 0f, 0.45f)
        };
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.right = 0;
        overlay.style.top = 0;
        overlay.style.bottom = 0;

        Label timeLabel = new Label { pickingMode = PickingMode.Ignore };
        timeLabel.style.position = Position.Absolute;
        timeLabel.style.left = 0;
        timeLabel.style.right = 0;
        timeLabel.style.bottom = 0;
        timeLabel.style.height = 16;
        timeLabel.style.fontSize = 12;
        timeLabel.style.color = Color.white;
        timeLabel.style.backgroundColor = new Color(0f, 0f, 0f, 0.65f);
        timeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

        root.Add(icon);
        root.Add(overlay);
        root.Add(timeLabel);

        return new BuffSlotView(root, overlay, timeLabel);
    }

    private void UpdateBuffSlots()
    {
        foreach (KeyValuePair<BuffInstance, BuffSlotView> pair in buffSlotViews)
        {
            BuffInstance buffInstance = pair.Key;
            BuffSlotView slotView = pair.Value;

            slotView.TimeLabel.text = buffInstance.IsPermanent
                ? "--"
                : Mathf.CeilToInt(Mathf.Max(0f, buffInstance.RemainingTime)).ToString();

            slotView.CooldownOverlay.FillAmount = buffInstance.IsPermanent
                ? 0f
                : 1f - buffInstance.RemainingRatio;
        }
    }

    private void ConfigureBuffListStyle()
    {
        buffList.style.position = Position.Absolute;
        buffList.style.left = Length.Percent(1);
        buffList.style.bottom = Length.Percent(32);
        buffList.style.height = 52;
        buffList.style.flexDirection = FlexDirection.Row;
        buffList.style.alignItems = Align.Center;
    }

    private class BuffSlotView
    {
        public readonly VisualElement Root;
        public readonly CooldownOverlay CooldownOverlay;
        public readonly Label TimeLabel;

        public BuffSlotView(VisualElement root, CooldownOverlay cooldownOverlay, Label timeLabel)
        {
            Root = root;
            CooldownOverlay = cooldownOverlay;
            TimeLabel = timeLabel;
        }
    }
}
