using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ProjectAT.Option;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
public class OptionMenuUI : MonoBehaviour
{
    private enum OptionTab
    {
        Graphic,
        Sound,
        Control
    }

    private static readonly KeyCode[] DefaultSkillKeys =
    {
        KeyCode.Q,
        KeyCode.W,
        KeyCode.E,
        KeyCode.D,
        KeyCode.A
    };

    private static readonly List<string> FrameLimitChoices = 
        new List<string>
        {
            "60 FPS",
            "90 FPS",
            "144 FPS",
            "Off"
        };

    private static readonly List<string> AntiAliasingChoices =
        new List<string>
        {
            "No AA",
            "FXAA",
            "TAA"
        };

    private UIDocument uiDocument;
    private OptionManager optionManager;

    private VisualElement root;
    private VisualElement optionOverlay;
    private ScrollView optionScrollView;

    private Button graphicTabButton;
    private Button soundTabButton;
    private Button controlTabButton;

    private Label graphicTabIndicator;
    private Label soundTabIndicator;
    private Label controlTabIndicator;

    private VisualElement graphicContent;
    private VisualElement soundContent;
    private VisualElement controlContent;

    private Button resetButton;
    private Button cancelButton;
    private Button saveButton;

    private Slider bgmSlider;
    private Slider sfxSlider;
    private Slider uiSlider;

    private VisualElement bgmFill;
    private VisualElement sfxFill;
    private VisualElement uiFill;

    private Label bgmValueLabel;
    private Label sfxValueLabel;
    private Label uiValueLabel;

    private DropdownField resolutionDropdown;
    private Toggle vSyncToggle;
    private Label vSyncCheckmark;
    private DropdownField frameLimitDropdown;
    private DropdownField antiAliasingDropdown;

    private readonly Button[] skillKeyButtons = new Button[5];

    private readonly Action[] skillKeyButtonActions = new Action[5];

    private readonly KeyCode[] baselineSkillKeys = new KeyCode[5];

    private readonly KeyCode[] draftSkillKeys = new KeyCode[5]; //키 세팅 임시저장용

    private readonly List<Vector2Int> resolutionOptions = new List<Vector2Int>();

    private OptionSetting baselineSettings;
    private OptionSetting draftSettings;    //옵션 세팅 임시저장용

    private int capturingSkillIndex = -1;

    private bool uiReady;
    private bool callbacksRegistered;

    public event Action SaveCompleted;
    public event Action CancelCompleted;

    public bool IsOpen { get; private set; }

    public bool IsCapturingKey => capturingSkillIndex >= 0;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        uiReady = CacheUIElements();

        if (uiReady)
        {
            SetOptionVisible(false);
        }
    }

    private void OnEnable()
    {
        if (!uiReady)
        {
            uiReady = CacheUIElements();
        }

        if (!uiReady)
        {
            Debug.LogWarning($"{name}: Option UI elements are not ready.", this);

            return;
        }

        RegisterCallbacks();
        SetOptionVisible(false);
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
        CancelKeyCapture();

        IsOpen = false;
    }

    public void Open()
    {
        if (!uiReady || IsOpen)
        {
            return;
        }

        Managers managers = Managers.Instance;

        if (managers == null || managers.OptionManager == null || !managers.OptionManager.IsInitialized)
        {
            Debug.LogError($"{name}: OptionManager is not ready.", this);

            return;
        }

        optionManager = managers.OptionManager;

        baselineSettings = optionManager.GetSettingsSnapshot();

        draftSettings = baselineSettings;

        Array.Copy(DefaultSkillKeys, baselineSkillKeys, DefaultSkillKeys.Length);
        Array.Copy(DefaultSkillKeys, draftSkillKeys, DefaultSkillKeys.Length);

        ConfigureResolutionChoices();
        RefreshAllControls();
        SelectTab(OptionTab.Graphic);

        SetOptionVisible(true);
        IsOpen = true;

        graphicTabButton.Focus();
    }

    public void CloseWithoutSaving()
    {
        if (!IsOpen)
        {
            return;
        }

        CancelKeyCapture();

        IsOpen = false;
        SetOptionVisible(false);
    }

    public void Cancel()
    {
        if (!IsOpen)
        {
            return;
        }

        CloseWithoutSaving();
        CancelCompleted?.Invoke();
    }

    public void CancelKeyCapture()
    {
        if (!IsCapturingKey)
        {
            return;
        }

        skillKeyButtons[capturingSkillIndex].text = GetKeyDisplayName(draftSkillKeys[capturingSkillIndex]);

        capturingSkillIndex = -1;
    }

    private bool CacheUIElements()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError($"{name}: UIDocument is missing.", this);
            return false;
        }

        root = uiDocument.rootVisualElement;
        GetUIElements();

        for (int i = 0; i < skillKeyButtons.Length; i++)
        {
            int capturedIndex = i;

            skillKeyButtons[i] = root.Q<Button>($"Skill{i + 1}KeyButton");
            skillKeyButtonActions[i] = () => BeginKeyCapture(capturedIndex);
        }

        bool foundAll = CheckUIElementReferences();

        if (!foundAll)
        {
            Debug.LogError($"{name}: Required elements were not found in OptionMenu.uxml.", this);

            return false;
        }

        frameLimitDropdown.choices = new List<string>(FrameLimitChoices);
        antiAliasingDropdown.choices = new List<string>(AntiAliasingChoices);

        return true;
    }

    private void GetUIElements()
    {
        optionOverlay = root.Q<VisualElement>("OptionOverlay");
        optionScrollView = root.Q<ScrollView>("OptionScrollView");
        graphicTabButton = root.Q<Button>("GraphicTabButton");
        soundTabButton = root.Q<Button>("SoundTabButton");
        controlTabButton = root.Q<Button>("ControlTabButton");
        graphicTabIndicator = root.Q<Label>("GraphicTabIndicator");
        soundTabIndicator = root.Q<Label>("SoundTabIndicator");
        controlTabIndicator = root.Q<Label>("ControlTabIndicator");
        graphicContent = root.Q<VisualElement>("GraphicContent");
        soundContent = root.Q<VisualElement>("SoundContent");
        controlContent = root.Q<VisualElement>("ControlContent");
        resetButton = root.Q<Button>("OptionResetButton");
        cancelButton = root.Q<Button>("OptionCancelButton");
        saveButton = root.Q<Button>("OptionSaveButton");
        bgmSlider = root.Q<Slider>("BgmVolumeSlider");
        sfxSlider = root.Q<Slider>("SfxVolumeSlider");
        uiSlider = root.Q<Slider>("UiVolumeSlider");
        bgmFill = root.Q<VisualElement>("BgmVolumeFill");
        sfxFill = root.Q<VisualElement>("SfxVolumeFill");
        uiFill = root.Q<VisualElement>("UiVolumeFill");
        bgmValueLabel = root.Q<Label>("BgmVolumeLabel");
        sfxValueLabel = root.Q<Label>("SfxVolumeLabel");
        uiValueLabel = root.Q<Label>("UiVolumeLabel");
        resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
        vSyncToggle = root.Q<Toggle>("VSyncToggle");
        vSyncCheckmark = root.Q<Label>("VSyncCheckmark");
        frameLimitDropdown = root.Q<DropdownField>("FrameLimitDropdown");
        antiAliasingDropdown = root.Q<DropdownField>("AntiAliasingDropdown");
    }

    private bool CheckUIElementReferences()
    {
        bool foundAll 
            =  optionOverlay != null
            && optionScrollView != null
            && graphicTabButton != null
            && soundTabButton != null
            && controlTabButton != null
            && graphicTabIndicator != null
            && soundTabIndicator != null
            && controlTabIndicator != null
            && graphicContent != null
            && soundContent != null
            && controlContent != null
            && resetButton != null
            && cancelButton != null
            && saveButton != null
            && bgmSlider != null
            && sfxSlider != null
            && uiSlider != null
            && bgmFill != null
            && sfxFill != null
            && uiFill != null
            && bgmValueLabel != null
            && sfxValueLabel != null
            && uiValueLabel != null
            && resolutionDropdown != null
            && vSyncToggle != null
            && vSyncCheckmark != null
            && frameLimitDropdown != null
            && antiAliasingDropdown != null;

        for (int i = 0; i < skillKeyButtons.Length; i++)
        {
            foundAll &= skillKeyButtons[i] != null;
        }

        return foundAll;
    }

    private void RegisterCallbacks()
    {
        if (callbacksRegistered)
        {
            return;
        }

        graphicTabButton.clicked += SelectGraphicTab;
        soundTabButton.clicked += SelectSoundTab;
        controlTabButton.clicked += SelectControlTab;

        resetButton.clicked += ResetChanges;
        cancelButton.clicked += Cancel;
        saveButton.clicked += SaveChanges;

        bgmSlider.RegisterValueChangedCallback(OnBgmVolumeChanged);
        sfxSlider.RegisterValueChangedCallback(OnSfxVolumeChanged);
        uiSlider.RegisterValueChangedCallback(OnUiVolumeChanged);
        resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);
        vSyncToggle.RegisterValueChangedCallback(OnVSyncChanged);
        frameLimitDropdown.RegisterValueChangedCallback(OnFrameLimitChanged);
        antiAliasingDropdown.RegisterValueChangedCallback(OnAntiAliasingChanged);

        for (int i = 0; i < skillKeyButtons.Length; i++)
        {
            skillKeyButtons[i].clicked += skillKeyButtonActions[i];
        }

        root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        callbacksRegistered = true;
    }

    private void UnregisterCallbacks()
    {
        if (!callbacksRegistered)
        {
            return;
        }

        graphicTabButton.clicked -= SelectGraphicTab;
        soundTabButton.clicked -= SelectSoundTab;
        controlTabButton.clicked -= SelectControlTab;

        resetButton.clicked -= ResetChanges;
        cancelButton.clicked -= Cancel;
        saveButton.clicked -= SaveChanges;

        bgmSlider.UnregisterValueChangedCallback(OnBgmVolumeChanged);
        sfxSlider.UnregisterValueChangedCallback(OnSfxVolumeChanged);
        uiSlider.UnregisterValueChangedCallback(OnUiVolumeChanged);
        resolutionDropdown.UnregisterValueChangedCallback(OnResolutionChanged);
        vSyncToggle.UnregisterValueChangedCallback(OnVSyncChanged);
        frameLimitDropdown.UnregisterValueChangedCallback(OnFrameLimitChanged);
        antiAliasingDropdown.UnregisterValueChangedCallback(OnAntiAliasingChanged);

        for (int i = 0; i < skillKeyButtons.Length; i++)
        {
            skillKeyButtons[i].clicked -= skillKeyButtonActions[i];
        }

        root.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        callbacksRegistered = false;
    }

    private void SelectGraphicTab()
    {
        SelectTab(OptionTab.Graphic);
    }

    private void SelectSoundTab()
    {
        SelectTab(OptionTab.Sound);
    }

    private void SelectControlTab()
    {
        SelectTab(OptionTab.Control);
    }

    private void SelectTab(OptionTab tab)
    {
        CancelKeyCapture();

        bool graphicSelected = tab == OptionTab.Graphic;
        bool soundSelected = tab == OptionTab.Sound;
        bool controlSelected = tab == OptionTab.Control;

        SetTabSelected(graphicTabButton, graphicTabIndicator, graphicSelected);
        SetTabSelected(soundTabButton, soundTabIndicator, soundSelected);
        SetTabSelected(controlTabButton, controlTabIndicator, controlSelected);

        graphicContent.style.display = graphicSelected ? DisplayStyle.Flex : DisplayStyle.None;
        soundContent.style.display = soundSelected ? DisplayStyle.Flex : DisplayStyle.None;
        controlContent.style.display = controlSelected ? DisplayStyle.Flex : DisplayStyle.None;
        optionScrollView.scrollOffset = Vector2.zero;
    }

    private static void SetTabSelected(Button button, Label indicator, bool selected)
    {
        button.EnableInClassList("option-tab--selected", selected);
        indicator.text = selected ? "|" : string.Empty; // "|" = tab indicator, displayed when the tab is selected
    }

    private void OnBgmVolumeChanged(ChangeEvent<float> evt)
    {
        AudioOptionSetting audio = draftSettings.Audio;

        audio.Bgm = NormalizePercentage(evt.newValue);
        draftSettings.Audio = audio;
        SetVolumeControlWithoutNotify(bgmSlider, bgmFill, bgmValueLabel, audio.Bgm);
    }

    private void OnSfxVolumeChanged(ChangeEvent<float> evt)
    {
        AudioOptionSetting audio = draftSettings.Audio;

        audio.Sfx = NormalizePercentage(evt.newValue);
        draftSettings.Audio = audio;
        SetVolumeControlWithoutNotify(sfxSlider, sfxFill, sfxValueLabel, audio.Sfx);
    }

    private void OnUiVolumeChanged(ChangeEvent<float> evt)
    {
        AudioOptionSetting audio = draftSettings.Audio;

        audio.Ui = NormalizePercentage(evt.newValue);
        draftSettings.Audio = audio;
        SetVolumeControlWithoutNotify(uiSlider, uiFill, uiValueLabel, audio.Ui);
    }

    private void OnResolutionChanged(ChangeEvent<string> evt)
    {
        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            Vector2Int resolution = resolutionOptions[i];

            if (GetResolutionText(resolution) != evt.newValue)
            {
                continue;
            }

            GraphicOptionSetting graphic = draftSettings.Graphic;

            graphic.Width = resolution.x;
            graphic.Height = resolution.y;

            draftSettings.Graphic = graphic;
            return;
        }
    }

    private void OnVSyncChanged(ChangeEvent<bool> evt)
    {
        GraphicOptionSetting graphic = draftSettings.Graphic;

        graphic.VSync = evt.newValue;
        draftSettings.Graphic = graphic;

        SetVSyncCheckmark(evt.newValue);
    }

    private void OnFrameLimitChanged(ChangeEvent<string> evt)
    {
        GraphicOptionSetting graphic = draftSettings.Graphic;

        graphic.FrameLimit = ParseFrameLimit(evt.newValue);
        draftSettings.Graphic = graphic;
    }

    private void OnAntiAliasingChanged(ChangeEvent<string> evt)
    {
        GraphicOptionSetting graphic = draftSettings.Graphic;

        graphic.AntiAliasing = ParseAntiAliasing(evt.newValue);
        draftSettings.Graphic = graphic;
    }

    private void ResetChanges()
    {
        CancelKeyCapture();

        draftSettings = baselineSettings;
        Array.Copy(baselineSkillKeys, draftSkillKeys, baselineSkillKeys.Length);
        RefreshAllControls();
    }

    private void SaveChanges()
    {
        CancelKeyCapture();

        if (optionManager == null)
        {
            Debug.LogError($"{name}: OptionManager is missing.", this);

            return;
        }

        if (HaveControlBindingsChanged())
        {
            Debug.LogWarning("[Option] Control key saving is not implemented yet. Displayed changes were not applied.", this);
        }

        optionManager.ApplyAndSave(draftSettings);

        baselineSettings = optionManager.GetSettingsSnapshot();

        IsOpen = false;
        SetOptionVisible(false);

        SaveCompleted?.Invoke();
    }

    private bool HaveControlBindingsChanged()
    {
        for (int i = 0; i < draftSkillKeys.Length; i++)
        {
            if (draftSkillKeys[i] != baselineSkillKeys[i])
            {
                return true;
            }
        }

        return false;
    }

    private void BeginKeyCapture(int index)
    {
        if (IsCapturingKey)
        {
            skillKeyButtons[capturingSkillIndex].text = GetKeyDisplayName(draftSkillKeys[capturingSkillIndex]);
        }

        capturingSkillIndex = index;

        skillKeyButtons[index].text = "Press Key...";
        skillKeyButtons[index].Focus();
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (!IsOpen || !IsCapturingKey || evt.keyCode == KeyCode.None)
        {
            return;
        }

        evt.StopImmediatePropagation(); // 이벤트전파를 중단함으로써 로직을 여기서 처리.

        if (evt.keyCode == KeyCode.Escape)
        {
            CancelKeyCapture();
            return;
        }

        draftSkillKeys[capturingSkillIndex] = evt.keyCode;
        skillKeyButtons[capturingSkillIndex].text = GetKeyDisplayName(evt.keyCode);

        capturingSkillIndex = -1;
    }

    /// <summary>
    /// dropdown 메뉴에 표시할 해상도 선택지를 구성합니다.
    /// </summary>
    private void ConfigureResolutionChoices()
    {
        resolutionOptions.Clear();

        IReadOnlyList<Vector2Int> available = optionManager.GetAvailableResolutions();
        List<string> choices = new List<string>();

        for (int i = 0; i < available.Count; i++)
        {
            Vector2Int resolution = available[i];

            resolutionOptions.Add(resolution);
            choices.Add(GetResolutionText(resolution));
        }

        resolutionDropdown.choices = choices;
    }

    private void RefreshAllControls()
    {
        GraphicOptionSetting graphic = draftSettings.Graphic;
        AudioOptionSetting audio = draftSettings.Audio;

        resolutionDropdown.SetValueWithoutNotify($"{graphic.Width} x {graphic.Height}");

        vSyncToggle.SetValueWithoutNotify(graphic.VSync);
        SetVSyncCheckmark(graphic.VSync);

        frameLimitDropdown.SetValueWithoutNotify(GetFrameLimitText(graphic.FrameLimit));
        antiAliasingDropdown.SetValueWithoutNotify(GetAntiAliasingText(graphic.AntiAliasing));

        SetVolumeControlWithoutNotify(bgmSlider, bgmFill, bgmValueLabel, audio.Bgm);
        SetVolumeControlWithoutNotify(sfxSlider, sfxFill, sfxValueLabel, audio.Sfx);
        SetVolumeControlWithoutNotify(uiSlider, uiFill, uiValueLabel, audio.Ui);

        for (int i = 0; i < skillKeyButtons.Length; i++)
        {
            skillKeyButtons[i].text = GetKeyDisplayName(draftSkillKeys[i]);
        }
    }

    private static string GetResolutionText(Vector2Int resolution)
    {
        return $"{resolution.x} x {resolution.y}";
    }

    private static string GetFrameLimitText(int value)
    {
        return value > 0? $"{value} FPS" : "Off";
    }

    private static int ParseFrameLimit(string value)
    {
        if (value == "Off")
        {
            return -1;
        }

        string numberText = value.Replace(" FPS", string.Empty);

        return int.TryParse(numberText, out int result)? result : -1;
    }

    private static string GetAntiAliasingText(AntiAliasingOption value)
    {
        return value switch
        {
            AntiAliasingOption.Off => "No AA",
            AntiAliasingOption.FXAA => "FXAA",
            AntiAliasingOption.TAA => "TAA",
            _ => "FXAA"
        };
    }

    private static AntiAliasingOption ParseAntiAliasing(string value)
    {
        return value switch
        {
            "No AA" => AntiAliasingOption.Off,
            "TAA" => AntiAliasingOption.TAA,
            _ => AntiAliasingOption.FXAA
        };
    }

    private void SetVSyncCheckmark(bool enabled)
    {
        vSyncCheckmark.text = enabled ? "\u2713" : string.Empty; //\u2713 = unicode checkmark icon
    }

    private static void SetVolumeControlWithoutNotify(Slider slider, VisualElement fill, Label valueLabel, float percentage)
    {
        float normalized = NormalizePercentage(percentage);

        slider.SetValueWithoutNotify(normalized);
        fill.style.width = Length.Percent(normalized);
        valueLabel.text = $"{Mathf.RoundToInt(normalized)}%";
    }

    private static float NormalizePercentage( float value)
    {
        return Mathf.Round(Mathf.Clamp(value, 0f, 100f));
    }

    private static string GetKeyDisplayName(KeyCode keyCode)
    {
        return keyCode.ToString();
    }

    private void SetOptionVisible(bool visible)
    {
        if (optionOverlay == null)
        {
            return;
        }

        optionOverlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}