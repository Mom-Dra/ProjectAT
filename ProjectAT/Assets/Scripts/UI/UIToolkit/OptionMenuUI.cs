using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

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

    #region UI Elements
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement optionView;
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
    private readonly KeyCode[] baselineSkillKeys = new KeyCode[5];
    private readonly KeyCode[] draftSkillKeys = new KeyCode[5];
    #endregion =========================

    private SoundManager.AudioVolumeSettings baselineAudioSettings;
    private SoundManager.AudioVolumeSettings draftAudioSettings;
    private GraphicOptionSettings baselineGraphicSettings = GraphicOptionSettings.Default;
    private GraphicOptionSettings draftGraphicSettings = GraphicOptionSettings.Default;
    private int capturingSkillIndex = -1;
    private bool uiReady;
    private bool callbacksRegistered;

    public event Action SaveCompleted;

    public bool IsOpen { get; private set; }
    public bool IsCapturingKey => capturingSkillIndex >= 0;

    private struct GraphicOptionSettings
    {
        public string Resolution;
        public bool VSync;
        public string FrameLimit;
        public string AntiAliasing;

        public GraphicOptionSettings(string resolution, bool vSync, string frameLimit, string antiAliasing)
        {
            Resolution = resolution;
            VSync = vSync;
            FrameLimit = frameLimit;
            AntiAliasing = antiAliasing;
        }

        public static GraphicOptionSettings Default => new GraphicOptionSettings(
            "1920 x 1080",
            false,
            "Off",
            "FXAA"
        );
    }

    private static readonly List<string> ResolutionChoices = new List<string>
    {
        "1280 x 720",
        "1920 x 1080",
        "2560 x 1440"
    };

    private static readonly List<string> FrameLimitChoices = new List<string>
    {
        "60 FPS",
        "90 FPS",
        "144 FPS",
        "Off"
    };

    private static readonly List<string> AntiAliasingChoices = new List<string>
    {
        "No AA",
        "FXAA",
        "TAA"
    };

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

        SoundManager soundManager = Managers.Instance != null ? Managers.Instance.SoundManager : null;

        baselineAudioSettings = soundManager != null ? soundManager.CurrentAudioSettings : SoundManager.AudioVolumeSettings.Default;
        draftAudioSettings = baselineAudioSettings;
        draftGraphicSettings = baselineGraphicSettings;

        for (int i = 0; i < DefaultSkillKeys.Length; i++)
        {
            baselineSkillKeys[i] = DefaultSkillKeys[i];
            draftSkillKeys[i] = DefaultSkillKeys[i]; //TODO : 나중에 저장된 키 객체를 정의하면 이를 이용해 초기화 할 것
        }

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
        optionView = root.Q<VisualElement>("OptionView");
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

        for (int i = 0; i < skillKeyButtons.Length; i++)
        {
            skillKeyButtons[i] = root.Q<Button>($"Skill{i + 1}KeyButton");
        }

        bool foundAll = optionView != null && optionScrollView != null
            && graphicTabButton != null && soundTabButton != null && controlTabButton != null
            && graphicTabIndicator != null && soundTabIndicator != null && controlTabIndicator != null
            && graphicContent != null && soundContent != null && controlContent != null
            && resetButton != null && saveButton != null
            && bgmSlider != null && sfxSlider != null && uiSlider != null
            && bgmFill != null && sfxFill != null && uiFill != null
            && bgmValueLabel != null && sfxValueLabel != null && uiValueLabel != null
            && resolutionDropdown != null && vSyncToggle != null
            && vSyncCheckmark != null
            && frameLimitDropdown != null && antiAliasingDropdown != null;

        for (int i = 0; i < skillKeyButtons.Length; i++)
        {
            foundAll &= skillKeyButtons[i] != null;
        }

        if (!foundAll)
        {
            Debug.LogError($"{name}: Required Option UI elements were not found in PauseMenu.uxml.", this);
        }
        else
        {
            ConfigureGraphicControls();
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
        saveButton.clicked += SaveChanges;

        bgmSlider.RegisterValueChangedCallback(OnBgmVolumeChanged);
        sfxSlider.RegisterValueChangedCallback(OnSfxVolumeChanged);
        uiSlider.RegisterValueChangedCallback(OnUiVolumeChanged);

        resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);
        vSyncToggle.RegisterValueChangedCallback(OnVSyncChanged);
        frameLimitDropdown.RegisterValueChangedCallback(OnFrameLimitChanged);
        antiAliasingDropdown.RegisterValueChangedCallback(OnAntiAliasingChanged);

        skillKeyButtons[0].clicked += BeginSkill1Capture;
        skillKeyButtons[1].clicked += BeginSkill2Capture;
        skillKeyButtons[2].clicked += BeginSkill3Capture;
        skillKeyButtons[3].clicked += BeginSkill4Capture;
        skillKeyButtons[4].clicked += BeginSkill5Capture;

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
        saveButton.clicked -= SaveChanges;

        bgmSlider.UnregisterValueChangedCallback(OnBgmVolumeChanged);
        sfxSlider.UnregisterValueChangedCallback(OnSfxVolumeChanged);
        uiSlider.UnregisterValueChangedCallback(OnUiVolumeChanged);

        resolutionDropdown.UnregisterValueChangedCallback(OnResolutionChanged);
        vSyncToggle.UnregisterValueChangedCallback(OnVSyncChanged);
        frameLimitDropdown.UnregisterValueChangedCallback(OnFrameLimitChanged);
        antiAliasingDropdown.UnregisterValueChangedCallback(OnAntiAliasingChanged);

        skillKeyButtons[0].clicked -= BeginSkill1Capture;
        skillKeyButtons[1].clicked -= BeginSkill2Capture;
        skillKeyButtons[2].clicked -= BeginSkill3Capture;
        skillKeyButtons[3].clicked -= BeginSkill4Capture;
        skillKeyButtons[4].clicked -= BeginSkill5Capture;

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
        indicator.text = selected ? "|" : string.Empty;
    }

    private void OnBgmVolumeChanged(ChangeEvent<float> evt)
    {
        float value = NormalizePercentage(evt.newValue);
        draftAudioSettings.Bgm = value;
        SetVolumeControlWithoutNotify(bgmSlider, bgmFill, bgmValueLabel, value);
    }

    private void OnSfxVolumeChanged(ChangeEvent<float> evt)
    {
        float value = NormalizePercentage(evt.newValue);
        draftAudioSettings.Sfx = value;
        SetVolumeControlWithoutNotify(sfxSlider, sfxFill, sfxValueLabel, value);
    }

    private void OnUiVolumeChanged(ChangeEvent<float> evt)
    {
        float value = NormalizePercentage(evt.newValue);
        draftAudioSettings.Ui = value;
        SetVolumeControlWithoutNotify(uiSlider, uiFill, uiValueLabel, value);
    }

    private void OnResolutionChanged(ChangeEvent<string> evt)
    {
        draftGraphicSettings.Resolution = evt.newValue;
    }

    private void OnVSyncChanged(ChangeEvent<bool> evt)
    {
        draftGraphicSettings.VSync = evt.newValue;
        SetVSyncCheckmark(evt.newValue);
    }

    private void OnFrameLimitChanged(ChangeEvent<string> evt)
    {
        draftGraphicSettings.FrameLimit = evt.newValue;
    }

    private void OnAntiAliasingChanged(ChangeEvent<string> evt)
    {
        draftGraphicSettings.AntiAliasing = evt.newValue;
    }

    private void ResetChanges()
    {
        CancelKeyCapture();
        
        draftAudioSettings = baselineAudioSettings;
        draftGraphicSettings = baselineGraphicSettings;
        Array.Copy(baselineSkillKeys, draftSkillKeys, baselineSkillKeys.Length);

        RefreshAllControls();
    }

    private void SaveChanges()
    {
        CancelKeyCapture();

        // 그래픽 설정값은 현재 실행 세션의 UI 상태로만 유지합니다.
        // 실제 그래픽 API와 PlayerPrefs 적용은 별도 구현에서 담당합니다.
        baselineGraphicSettings = draftGraphicSettings;

        SoundManager soundManager = Managers.Instance != null ? Managers.Instance.SoundManager : null;

        if (soundManager == null)
        {
            Debug.LogError("[Option] SoundManager was not found. Audio settings could not be applied.", this);
        }
        else
        {
            soundManager.ApplyAudioSettings(draftAudioSettings, true);
        }

        if (HaveControlBindingsChanged())
        {
            Debug.LogWarning("[Option] Control key saving is not implemented yet. The displayed key changes were not applied.", this);
        }

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

    private void BeginSkill1Capture()
    {
        BeginKeyCapture(0);
    }

    private void BeginSkill2Capture()
    {
        BeginKeyCapture(1);
    }

    private void BeginSkill3Capture()
    {
        BeginKeyCapture(2);
    }

    private void BeginSkill4Capture()
    {
        BeginKeyCapture(3);
    }

    private void BeginSkill5Capture()
    {
        BeginKeyCapture(4);
    }

    private void BeginKeyCapture(int skillIndex)
    {
        if (IsCapturingKey)
        {
            skillKeyButtons[capturingSkillIndex].text = GetKeyDisplayName(draftSkillKeys[capturingSkillIndex]);
        }

        capturingSkillIndex = skillIndex;
        skillKeyButtons[skillIndex].text = "Press Key...";
        skillKeyButtons[skillIndex].Focus();
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (!IsOpen || !IsCapturingKey || evt.keyCode == KeyCode.None)
        {
            return;
        }

        evt.StopImmediatePropagation(); // 해당 이벤트가 다른 UI 요소에 전달되지 않도록 막음

        if (evt.keyCode == KeyCode.Escape)
        {
            CancelKeyCapture();
            return;
        }

        draftSkillKeys[capturingSkillIndex] = evt.keyCode;
        skillKeyButtons[capturingSkillIndex].text = GetKeyDisplayName(evt.keyCode);
        capturingSkillIndex = -1;
    }

    private void RefreshAllControls()
    {
        SetVolumeControlWithoutNotify(bgmSlider, bgmFill, bgmValueLabel, draftAudioSettings.Bgm);
        SetVolumeControlWithoutNotify(sfxSlider, sfxFill, sfxValueLabel, draftAudioSettings.Sfx);
        SetVolumeControlWithoutNotify(uiSlider, uiFill, uiValueLabel, draftAudioSettings.Ui);
        SetGraphicControlsWithoutNotify(draftGraphicSettings);

        for (int i = 0; i < skillKeyButtons.Length; i++)
        {
            skillKeyButtons[i].text = GetKeyDisplayName(draftSkillKeys[i]);
        }
    }

    private void ConfigureGraphicControls()
    {
        resolutionDropdown.choices = new List<string>(ResolutionChoices);
        frameLimitDropdown.choices = new List<string>(FrameLimitChoices);
        antiAliasingDropdown.choices = new List<string>(AntiAliasingChoices);

        SetGraphicControlsWithoutNotify(baselineGraphicSettings);
    }

    private void SetGraphicControlsWithoutNotify(GraphicOptionSettings settings)
    {
        resolutionDropdown.SetValueWithoutNotify(settings.Resolution);
        vSyncToggle.SetValueWithoutNotify(settings.VSync);
        SetVSyncCheckmark(settings.VSync);
        frameLimitDropdown.SetValueWithoutNotify(settings.FrameLimit);
        antiAliasingDropdown.SetValueWithoutNotify(settings.AntiAliasing);
    }

    private void SetVSyncCheckmark(bool enabled)
    {
        vSyncCheckmark.text = enabled ? "✓" : string.Empty;
    }

    /// <summary>
    /// 볼륨 슬라이더, 채우기 요소, 값 레이블을 주어진 백분율 값으로 업데이트합니다. 슬라이더의 값은 알림 없이 설정됩니다.
    /// </summary>
    /// <param name="slider"> 볼륨 슬라이더 </param>
    /// <param name="fill"> 채우기 요소 </param>
    /// <param name="valueLabel"> 값 레이블 </param>
    /// <param name="percentage"> 백분율 값 </param>
    private static void SetVolumeControlWithoutNotify(Slider slider, VisualElement fill, Label valueLabel, float percentage)
    {
        float normalized = NormalizePercentage(percentage);
        slider.SetValueWithoutNotify(normalized);

        fill.style.width = Length.Percent(normalized);
        valueLabel.text = $"{Mathf.RoundToInt(normalized)}%";
    }

    private static float NormalizePercentage(float value)
    {
        return Mathf.Round(Mathf.Clamp(value, 0f, 100f));
    }

    /// <summary>
    /// 해당 키코드의 표시 이름을 string으로 반환함.
    /// </summary>
    /// <param name="keyCode"> 원하는 키코드 </param>
    /// <returns> 표시 이름 문자열</returns>
    private static string GetKeyDisplayName(KeyCode keyCode)
    {
        return keyCode.ToString();
    }

    private void SetOptionVisible(bool visible)
    {
        if (optionView == null)
        {
            return;
        }

        optionView.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
