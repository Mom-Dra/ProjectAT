using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using SkillOptionInterfaces;

public class PlayerHUD : MonoBehaviour
{
    [Header("Bound Entity References")]
    [SerializeField] private InGameManager boundInGameManager;
    [SerializeField] private PlayerController selectedPlayer;
    [SerializeField] private EntityStatus playerStatus;
    [SerializeField] private PlayerSkillModule playerSkillModule;
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private Inventory entityInventory;

    [Header("Player Status UI Elements")]
    [SerializeField] private UIDocument _uiDocument;
    private RadialProgressBar _healthBar;
    private VisualElement _playerPortrait;

    [Header("Weapon Info UI Elements")]
    private VisualElement _playerWeaponIcon;
    private Label _playerAmmoText;

    [Header("Skill Info UI Elements")]
    private readonly VisualElement[] skillInfos = new VisualElement[5];
    private readonly VisualElement[] skillIcons = new VisualElement[5];
    private readonly VisualElement[] skillDisableOverlays = new VisualElement[5];
    private readonly CooldownOverlay[] skillCooldownOverlays = new CooldownOverlay[5];
    private readonly Label[] skillItemLabels = new Label[5];

    private readonly Coroutine[] skillCooldownCoroutines = new Coroutine[5];
    private readonly EventCallback<ClickEvent>[] skillClickCallbacks = new EventCallback<ClickEvent>[5];

    [Header("Buff Info UI Elements")]
    private VisualElement buffList;
    private BuffModule boundBuffModule;
    private readonly Dictionary<BuffInstance, BuffSlotView> buffSlotViews = new Dictionary<BuffInstance, BuffSlotView>();

    private void Awake()
    {
        InitUiElements();
    }

    private void OnEnable()
    {
        RegisterSkillClickCallbacks();

        boundInGameManager = InGameManager.Instance;

        if (boundInGameManager != null)
        {
            boundInGameManager.SelectedPlayerChanged += BindEntityInfo;
        }
    }

    private void OnDisable()
    {
        if (boundInGameManager != null)
        {
            boundInGameManager.SelectedPlayerChanged -= BindEntityInfo;
            boundInGameManager = null;
        }

        UnregisterSkillClickCallbacks();
        UnBindEntityInfo();
    }

    private void Start()
    {
        BindEntityInfo((boundInGameManager != null) ? boundInGameManager.SelectedPlayer: null);
    }

    private void Update()
    {
        UpdateBuffSlots();
    }

    private void InitUiElements()
    {
        if (_uiDocument == null)
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        if (_uiDocument == null)
        {
            Debug.LogError($"{name}: UIDocument가 없습니다.", this);
            return;
        }

        VisualElement root = _uiDocument.rootVisualElement;

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
            skillInfos[i] = root.Q<VisualElement>($"SkillInfo_{i}");
            CheckUIElement(skillInfos[i], $"SkillInfo_{i}");

            if (skillInfos[i] == null)
            {
                continue;
            }

            skillIcons[i] = skillInfos[i].Q<VisualElement>("Icon"); CheckUIElement(skillIcons[i], $"SkillInfo_{i} Icon");
            skillItemLabels[i] = skillInfos[i].Q<Label>("ItemCount"); CheckUIElement(skillItemLabels[i], $"SkillInfo_{i} ItemCount");
            skillDisableOverlays[i] = skillInfos[i].Q<VisualElement>("DisableOverlays"); CheckUIElement(skillDisableOverlays[i], $"SkillInfo_{i} DisableOverlays");
            skillCooldownOverlays[i] = skillInfos[i].Q<CooldownOverlay>("SkillCoolDown");
            CheckUIElement(skillCooldownOverlays[i], $"SkillInfo_{i} SkillCoolDown");
        }

        ResetHud();
        RebuildBuffSlots();
    }

    private void CheckUIElement(VisualElement element, string elementName)
    {
        if (element == null)
        {
            Debug.LogError($"{elementName} UI element not found!", this);
        }
    }

    public void BindEntityInfo(PlayerController player)
    {
        // MissionTrackerUI와 동일하게 항상 기존 구독부터 제거합니다.
        UnBindEntityInfo();

        selectedPlayer = player;

        // null은 다른 타입의 오브젝트를 선택했을 때 발생할 수 있는 정상 상태입니다.
        if (selectedPlayer == null)
        {
            return;
        }

        if (selectedPlayer.TryGetComponent(out EntityStatus status))
        {
            playerStatus = status;
            playerStatus.onHealthChanged += SetPlayerHealthUI;
            SetPlayerStatusInfo(playerStatus);
        }

        weaponHolder = selectedPlayer.GetComponentInChildren<WeaponHolder>(true);

        if (weaponHolder != null)
        {
            weaponHolder.OnWeaponFired += UpdateWeaponInfo;
            weaponHolder.OnWeaponReloadStart += UpdateWeaponInfo;
            weaponHolder.OnWeaponReloaded += UpdateWeaponInfo;
            weaponHolder.OnWeaponChanged += UpdateWeaponInfo;

            SetPlayerWeaponInfo(weaponHolder);
        }

        if (selectedPlayer.TryGetComponent(out Inventory inventory))
        {
            entityInventory = inventory;
            entityInventory.OnItemCountChanged += HandleItemCountChanged;
        }

        if (selectedPlayer.TryGetComponent(out PlayerSkillModule skillModule))
        {
            playerSkillModule = skillModule;
            playerSkillModule.OnSkillExecuted += StartSkillCooldownEffect;
            playerSkillModule.SkillsInitialized += HandleSkillsInitialized;

            if (playerSkillModule.AreSkillsInitialized)
            {
                SynchronizeSkillInfo();
            }
        }

        if (selectedPlayer.TryGetComponent(out BuffModule buffModule))
        {
            BindBuffModule(buffModule);
        }
    }

    public void UnBindEntityInfo()
    {
        if (playerStatus != null)
        {
            playerStatus.onHealthChanged -= SetPlayerHealthUI;
        }

        if (weaponHolder != null)
        {
            weaponHolder.OnWeaponFired -= UpdateWeaponInfo;
            weaponHolder.OnWeaponReloadStart -= UpdateWeaponInfo;
            weaponHolder.OnWeaponReloaded -= UpdateWeaponInfo;
            weaponHolder.OnWeaponChanged -= UpdateWeaponInfo;
        }

        if (playerSkillModule != null)
        {
            playerSkillModule.OnSkillExecuted -= StartSkillCooldownEffect;
            playerSkillModule.SkillsInitialized -= HandleSkillsInitialized;
        }

        if (entityInventory != null)
        {
            entityInventory.OnItemCountChanged -= HandleItemCountChanged;
        }

        BindBuffModule(null);
        StopAllSkillCooldowns();

        selectedPlayer = null;
        playerStatus = null;
        playerSkillModule = null;
        weaponHolder = null;
        entityInventory = null;

        ResetHud();
    }

    private void ResetHud()
    {
        if (_playerPortrait != null)
        {
            _playerPortrait.style.backgroundImage = StyleKeyword.None;
        }

        if (_healthBar != null)
        {
            _healthBar.Progress = 0f;
        }

        if (_playerWeaponIcon != null)
        {
            _playerWeaponIcon.style.backgroundImage = StyleKeyword.None;
        }

        if (_playerAmmoText != null)
        {
            _playerAmmoText.text = "0 / 0";
        }

        for (int i = 0; i < skillInfos.Length; i++)
        {
            ResetSkillSlot(i);
        }
    }

    private void ResetSkillSlot(int index)
    {
        StopSkillCooldown(index);

        if (skillIcons[index] != null)
        {
            skillIcons[index].style.backgroundImage = StyleKeyword.None;
        }

        if (skillItemLabels[index] != null)
        {
            skillItemLabels[index].text = "0";
            skillItemLabels[index].visible = true;
            skillItemLabels[index].style.display = DisplayStyle.None;
            skillItemLabels[index].style.opacity = 1f;

            skillItemLabels[index].SetEnabled(true);
        }

        if (skillDisableOverlays[index] != null)
        {
            // 섹션은 숨기지 않고 비활성 상태만 표시합니다.
            skillDisableOverlays[index].visible = true;
        }

        if (skillCooldownOverlays[index] != null)
        {
            skillCooldownOverlays[index].FillAmount = 0f;
        }
    }

    private void SetPlayerStatusInfo(EntityStatus status)
    {
        if (status == null)
        {
            return;
        }

        SetPlayerPortrait(status.InitStatusRef.PortatitSprite);
        SetPlayerHealthUI(status.Ratio);
    }

    private void SetPlayerPortrait(Sprite portrait)
    {
        if (_playerPortrait != null)
        {
            _playerPortrait.style.backgroundImage = portrait != null ? new StyleBackground(portrait) : StyleKeyword.None;
        }
    }

    private void SetPlayerHealthUI(float healthRatio)
    {
        if (_healthBar != null)
        {
            _healthBar.Progress = Mathf.Clamp01(healthRatio) * 100f;
        }
    }

    private void SetPlayerWeaponInfo(WeaponHolder holder)
    {
        UpdateWeaponInfo(holder != null ? holder.NowWeapon : null);
    }

    private void UpdateWeaponInfo(Gun gun)
    {
        if (gun == null)
        {
            if (_playerWeaponIcon != null)
            {
                _playerWeaponIcon.style.backgroundImage = StyleKeyword.None;
            }

            SetAmmoText(0, 0);
            return;
        }

        if (_playerWeaponIcon != null)
        {
            _playerWeaponIcon.style.backgroundImage = new StyleBackground(gun.GunData.GunIcon);
        }

        SetAmmoText(gun.MagAmmo, gun.RemainAmmo);
    }

    private void SetAmmoText(int currentAmmo, int remainAmmo)
    {
        if (_playerAmmoText != null)
        {
            _playerAmmoText.text = $"{currentAmmo} / {remainAmmo}";
        }
    }

    private void HandleSkillsInitialized()
    {
        SynchronizeSkillInfo();
    }

    private void SynchronizeSkillInfo()
    {
        if (playerSkillModule == null ||
            !playerSkillModule.AreSkillsInitialized)
        {
            return;
        }

        IReadOnlyList<Skill> skills = playerSkillModule.SkillInstances;

        for (int i = 0; i < skillInfos.Length; i++)
        {
            ResetSkillSlot(i);

            if (skills == null || i >= skills.Count)
            {
                continue;
            }

            Skill skill = skills[i];

            if (skill == null || skill.SkillData == null)
            {
                continue;
            }

            if (skillIcons[i] != null)
            {
                skillIcons[i].style.backgroundImage =
                    new StyleBackground(skill.SkillData.SkillIcon);
            }

            if (skill is IInventoryCostSkill inventoryCostSkill)
            {
                int itemCount = entityInventory != null? entityInventory.GetItemCount(inventoryCostSkill.NeededItemData) : 0;

                bool hasEnoughItem = UpdateInventorySkillInfo(i, inventoryCostSkill, itemCount);

                if (!hasEnoughItem)
                {
                    continue;
                }
            }
            else if (skillDisableOverlays[i] != null)
            {
                skillDisableOverlays[i].visible = false;
            }

            StartSkillCooldownEffect(skill, (SkillNumber)i);
        }
    }

    private void HandleItemCountChanged(ItemData changedItem, int itemCount)
    {
        if (playerSkillModule == null || !playerSkillModule.AreSkillsInitialized)
        {
            return;
        }

        IReadOnlyList<Skill> skills = playerSkillModule.SkillInstances;

        for (int i = 0; i < skills.Count && i < skillInfos.Length; i++)
        {
            Skill skill = skills[i];

            if (skill is not IInventoryCostSkill inventoryCostSkill) continue;
            if (inventoryCostSkill.NeededItemData != changedItem) continue;
            bool hasEnoughItem = UpdateInventorySkillInfo(i, inventoryCostSkill, itemCount);

            if (hasEnoughItem)
            {
                StartSkillCooldownEffect(skill, (SkillNumber)i);
            }
        }
    }
    private bool UpdateInventorySkillInfo(int index, IInventoryCostSkill inventoryCostSkill, int itemCount)
    {
        bool hasEnoughItem = inventoryCostSkill.NeededItemData != null && itemCount >= inventoryCostSkill.NeededItemAmount;

        if (skillItemLabels[index] != null)
        {
            skillItemLabels[index].style.display = DisplayStyle.Flex;
            skillItemLabels[index].visible = true;
            skillItemLabels[index].text = Mathf.Max(0, itemCount).ToString();

            skillItemLabels[index].style.opacity = hasEnoughItem ? 1f : 0f;
            skillItemLabels[index].SetEnabled(true);
        }

        if (skillDisableOverlays[index] != null)
        {
            skillDisableOverlays[index].visible = !hasEnoughItem;
        }

        if (!hasEnoughItem)
        {
            StopSkillCooldownEffect(index); // 실행 중인 쿨다운을 멈추고 FillAmount도 제거합니다.
        }

        return hasEnoughItem;
    }

    private void RegisterSkillClickCallbacks()
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i] == null) continue;

            int capturedIndex = i;

            skillClickCallbacks[i] ??= evt => //callback함수를 따로 뺄까? (26.08.03)
            {
                if (playerSkillModule == null || !playerSkillModule.AreSkillsInitialized) return;
                playerSkillModule.ActivateTargettingMode((SkillNumber)capturedIndex);
            };

            skillIcons[i].RegisterCallback(skillClickCallbacks[i]);
        }
    }

    private void UnregisterSkillClickCallbacks()
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i] == null || skillClickCallbacks[i] == null)
            {
                continue;
            }

            skillIcons[i].UnregisterCallback(skillClickCallbacks[i]);
        }
    }

    private void StartSkillCooldownEffect(Skill skill, SkillNumber skillNumber)
    {
        int index = (int)skillNumber;

        if (!IsValidSkillIndex(index))
        {
            return;
        }

        if (skill == null ||
            skill.SkillData == null ||
            skill.SkillMaxCoolTime <= 0f)
        {
            StopSkillCooldownEffect(index);
            return;
        }

        if (skill is IInventoryCostSkill inventoryCostSkill)
        {
            int itemCount = entityInventory != null? entityInventory.GetItemCount(inventoryCostSkill.NeededItemData) : 0;

            bool hasEnoughItem = UpdateInventorySkillInfo(index, inventoryCostSkill, itemCount);

            if (!hasEnoughItem)
            {
                return;
            }
        }

        StopSkillCooldown(index);

        float elapsed = Mathf.Max(0f, Time.time - skill.CurrentSkillUseTime);

        if (elapsed >= skill.SkillMaxCoolTime)
        {
            SetSkillClockWipe(skillNumber, 1f);
            return;
        }

        SetSkillClockWipe(skillNumber, elapsed / skill.SkillMaxCoolTime);

        skillCooldownCoroutines[index] = StartCoroutine(CooldownCoroutine(skill, skillNumber));
    }

    private void StopSkillCooldownEffect(SkillNumber index)
    {
        StopSkillCooldownEffect((int)index);
    }

    private void StopSkillCooldownEffect(int skillNumber)
    {
        StopSkillCooldown(skillNumber);
        SetSkillClockWipe((SkillNumber)skillNumber, 1f);
    }

    private IEnumerator CooldownCoroutine(Skill skill, SkillNumber skillNumber)
    {
        int index = (int)skillNumber;

        while (true) //while(true)는 위험해보임(26.08.03)
        {
            float elapsed = Mathf.Max(0f, Time.time - skill.CurrentSkillUseTime);

            float progress = Mathf.Clamp01(elapsed / skill.SkillMaxCoolTime);

            SetSkillClockWipe(skillNumber, progress);

            if (progress >= 1f)
            {
                break;
            }

            yield return null;
        }

        skillCooldownCoroutines[index] = null;
    }

    private void SetSkillClockWipe(SkillNumber skillNumber, float cooldownProgress)
    {
        int index = (int)skillNumber;

        if (!IsValidSkillIndex(index) || skillCooldownOverlays[index] == null)
        {
            return;
        }

        skillCooldownOverlays[index].FillAmount = 1f - Mathf.Clamp01(cooldownProgress);
    }

    private bool IsValidSkillIndex(int index)
    {
        return index >= 0 && index < skillInfos.Length;
    }

    private void StopSkillCooldown(int index)
    {
        if (!IsValidSkillIndex(index) || skillCooldownCoroutines[index] == null)
        {
            return;
        }

        StopCoroutine(skillCooldownCoroutines[index]);
        skillCooldownCoroutines[index] = null;
    }

    private void StopAllSkillCooldowns()
    {
        for (int i = 0; i < skillCooldownCoroutines.Length; i++)
        {
            StopSkillCooldown(i);
        }
    }

    #region Buff Infos
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
    #endregion ================
    #region Buff Slot View Class
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
    #endregion ================
}
