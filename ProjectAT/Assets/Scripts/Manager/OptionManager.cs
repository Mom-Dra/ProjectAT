using UnityEngine;
using ProjectAT.Option;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class OptionManager : MonoBehaviour
{
    #region Const Keys
    private const int CurrentSchemaVersion = 1;
    private const string SchemaVersionPreference = "Option.SchemaVersion";

    private const string ResolutionWidthPreference  = "Option.Graphic.ResolutionWidth";
    private const string ResolutionHeightPreference = "Option.Graphic.ResolutionHeight";
    private const string VSyncPreference = "Option.Graphic.VSync";
    private const string FrameLimitPreference = "Option.Graphic.FrameLimit";
    private const string AntiAliasingPreference = "Option.Graphic.AntiAliasing";

    private const string BgmVolumePreference = "Option.Audio.BGM";
    private const string SfxVolumePreference = "Option.Audio.SFX";
    private const string UiVolumePreference = "Option.Audio.UI";

    private static readonly int[] SupportedFrameLimits = {-1, 60, 90, 144};
    #endregion =============================

    private SoundManager soundManager;
    private bool initialized = false;

    public OptionSetting CurrentSetting {get; private set;} = OptionSetting.Default;
    public bool IsInitialized => initialized;
    public event Action<OptionSetting> SettingsApplied;

    public void Initialize(SoundManager targetSoundManager)
    {
        if(initialized) return;
        if(targetSoundManager == null)
        {
            Debug.LogError("[OptionManager] SoundManager is null. SoundManager is required.", this);
            return;
        }

        soundManager = targetSoundManager;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        initialized = true;
        ReloadAndApply();

    }

    private void OnDestroy()
    {
        if(!initialized) return;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public OptionSetting GetSettingsSnapshot()
    {
        return CurrentSetting;
    }

    /// <summary>
    /// 사용 가능한 해상도 목록을 반환합니다.
    /// </summary>
    /// <returns> 읽기 전용인 사용 가능 해상도 목록</returns>
    public IReadOnlyList<Vector2Int> GetAvailableResolutions()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        HashSet<Vector2Int> uniqueChart = new HashSet<Vector2Int>();
        
        foreach(Resolution resolution in Screen.resolutions)
        {
            Vector2Int size = new Vector2Int(resolution.width, resolution.height);
            if (uniqueChart.Add(size))
            {
                result.Add(size);
            }
        }

        if(result.Count == 0)
        {
            result.Add(new Vector2Int(Screen.width, Screen.height));
        }

        result.Sort((left, right) => {
            int widthComparision = left.x.CompareTo(right.x);

            return (widthComparision != 0) ? widthComparision : left.y.CompareTo(right.y);
        });

        return result;
    }

    public void ReloadAndApply()
    {
        EnsureInitialized();

        OptionSetting loaded = LoadFromPreferences();
        ApplyAndSave(loaded);
    }

    public void ApplyAndSave(OptionSetting RequestedSetting)
    {
        EnsureInitialized();

        OptionSetting normalized = Normalize(RequestedSetting);
        CurrentSetting = normalized;

        Apply(normalized);
        SaveToPreferences(normalized);

        SettingsApplied?.Invoke(CurrentSetting);
    }

    private void EnsureInitialized()
    {
        if (!initialized)
        {
            throw new InvalidOperationException(
                "OptionManager.Initialize must be called first.");
        }
    }

    private OptionSetting LoadFromPreferences()
    {
        GraphicOptionSetting graphicDefaults = GetValidGraphicDefaults();
        GraphicOptionSetting graphic = new GraphicOptionSetting(

            PlayerPrefs.GetInt(ResolutionWidthPreference, graphicDefaults.Width),
            PlayerPrefs.GetInt(ResolutionHeightPreference, graphicDefaults.Height),
            PlayerPrefs.GetInt(VSyncPreference, graphicDefaults.VSync ? 1 : 0) != 0,
            PlayerPrefs.GetInt(FrameLimitPreference, graphicDefaults.FrameLimit),
            (AntiAliasingOption)PlayerPrefs.GetInt(AntiAliasingPreference, (int)graphicDefaults.AntiAliasing)
        );

        AudioOptionSetting audioOptionSetting = AudioOptionSetting.Default;
        AudioOptionSetting audio = new AudioOptionSetting(
            PlayerPrefs.GetFloat(BgmVolumePreference, audioOptionSetting.Bgm),
            PlayerPrefs.GetFloat(SfxVolumePreference, audioOptionSetting.Sfx),
            PlayerPrefs.GetFloat(UiVolumePreference, audioOptionSetting.Ui)
        );

        return new OptionSetting(graphic, audio, new ControlKeySetting());
    }

    private void SaveToPreferences(OptionSetting setting)
    {
        //Schema
        PlayerPrefs.SetInt(SchemaVersionPreference, CurrentSchemaVersion);

        SaveGraphicSetting(setting.Graphic);
        SaveSoundSetting(setting.Audio);
        //SaveControlSetting(setting.Control);

        PlayerPrefs.Save();
    }

    private void SaveGraphicSetting(GraphicOptionSetting graphic)
    {
        PlayerPrefs.SetInt(ResolutionWidthPreference, graphic.Width);
        PlayerPrefs.SetInt(ResolutionHeightPreference, graphic.Height);
        PlayerPrefs.SetInt(VSyncPreference, graphic.VSync ? 1 : 0);
        PlayerPrefs.SetInt(FrameLimitPreference, graphic.FrameLimit);
        PlayerPrefs.SetInt(AntiAliasingPreference, (int)graphic.AntiAliasing);
    }

    private void SaveSoundSetting(AudioOptionSetting audio)
    {
        PlayerPrefs.SetFloat(BgmVolumePreference, audio.Bgm);
        PlayerPrefs.SetFloat(SfxVolumePreference, audio.Sfx);
        PlayerPrefs.SetFloat(UiVolumePreference, audio.Ui);
    }

    private OptionSetting Normalize(OptionSetting settings)
    {
        GraphicOptionSetting graphic = settings.Graphic;
        AudioOptionSetting audio = settings.Audio;

        if(!IsSupportedResolution(graphic.Width, graphic.Height))
        {
            GraphicOptionSetting defaults = GetValidGraphicDefaults();

            graphic.Width = defaults.Width;
            graphic.Height = defaults.Height;
        }

        if(!IsSupportedFrameLimit(graphic.FrameLimit))
        {
            graphic.FrameLimit = GetValidGraphicDefaults().FrameLimit;
        }
        if(!Enum.IsDefined(typeof(AntiAliasingOption), graphic.AntiAliasing))
        {
            graphic.AntiAliasing = GraphicOptionSetting.Default.AntiAliasing;
        }

        audio.Bgm = NormalizePercentage(audio.Bgm);
        audio.Sfx = NormalizePercentage(audio.Sfx);
        audio.Ui = NormalizePercentage(audio.Ui);

        settings.Graphic = graphic;
        settings.Audio = audio;

        return settings;
    }

    private GraphicOptionSetting GetValidGraphicDefaults()
    {
        GraphicOptionSetting defaults = GraphicOptionSetting.Default;

        if(IsSupportedResolution(defaults.Width, defaults.Height))
        {
            return defaults;
        }

        int fallbackWidth = Screen.width;
        int fallbackHeight = Screen.height;

        if(!IsSupportedResolution(fallbackWidth, fallbackHeight))
        {
            fallbackWidth = Screen.currentResolution.width;
            fallbackHeight = Screen.currentResolution.height;
        }

        defaults.Width = fallbackWidth;
        defaults.Height = fallbackHeight;

        return defaults;
    }

    private bool IsSupportedResolution(int width, int height)
    {
        IReadOnlyList<Vector2Int> resolution = GetAvailableResolutions();

        for(int i = 0; i< resolution.Count; i++)
        {
            if(resolution[i].x == width && resolution[i].y == height)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSupportedFrameLimit(int frameLimit)
    {
        for(int i = 0 ; i < SupportedFrameLimits.Length; i++)
        {
            if(SupportedFrameLimits[i] == frameLimit) return true;
        }

        return false;
    }

    private void Apply(OptionSetting settings)
    {
        ApplyGraphic(settings.Graphic);
        ApplyAudio(settings.Audio);
    }

    private static void ApplyGraphic(GraphicOptionSetting settings)
    {
        Screen.SetResolution(settings.Width, settings.Height, Screen.fullScreenMode);
        QualitySettings.vSyncCount = settings.VSync? 1 : 0;

        Application.targetFrameRate = settings.FrameLimit > 0 ? settings.FrameLimit : -1;

        ApplyAntiAliasing(settings.AntiAliasing);
    }

    private void ApplyAudio(AudioOptionSetting audio)
    {
        soundManager.ApplyAudioSettings(audio);
    }

    private static void ApplyAntiAliasing(AntiAliasingOption requestedMode)
    {
        AntialiasingMode urpMode = requestedMode switch
        {
            AntiAliasingOption.Off => AntialiasingMode.None,
            AntiAliasingOption.FXAA => AntialiasingMode.FastApproximateAntialiasing,
            AntiAliasingOption.TAA => AntialiasingMode.TemporalAntiAliasing,
            _ => AntialiasingMode.FastApproximateAntialiasing
        };

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach(Camera camera in cameras)
        {
            if (camera == null) continue;

            UniversalAdditionalCameraData camData = camera.GetUniversalAdditionalCameraData();

            camData.antialiasing = urpMode;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyAntiAliasing(CurrentSetting.Graphic.AntiAliasing);
    }
    private static float NormalizePercentage(float value)
    {
        return Mathf.Round(Mathf.Clamp(value, 0f, 100f));
    }
}