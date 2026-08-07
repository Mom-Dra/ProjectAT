using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    #region AudioVolumeSettings
    /// <summary>
    /// 각 오디오 볼륨 설정을 나타내는 구조체입니다. BGM, SFX, UI 볼륨을 퍼센트(0~100)로 저장합니다.
    /// </summary>
    public struct AudioVolumeSettings
    {
        public float Bgm;
        public float Sfx;
        public float Ui;

        public AudioVolumeSettings(float bgm, float sfx, float ui)
        {
            Bgm = bgm;
            Sfx = sfx;
            Ui = ui;
        }

        public static AudioVolumeSettings Default => new AudioVolumeSettings(50f, 50f, 50f);
    }
    #endregion =========================

    #region Parameter Names
    private const string BgmVolumeParameter = "BGMVolume";
    private const string SfxVolumeParameter = "SFXVolume";
    private const string UiVolumeParameter = "UIVolume";

    private const string BgmVolumePreference = "Option.Audio.BGM";
    private const string SfxVolumePreference = "Option.Audio.SFX";
    private const string UiVolumePreference = "Option.Audio.UI";
    #endregion =========================

    private const float MinimumDecibels = -80f;

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup uiMixerGroup;

    [Header("Audio Sources Setting")]
    [SerializeField] private int sfxSourceCount = 8;
    [SerializeField] private int bgmSourceCount = 1;

    [Header("Temp Setting")]
    [SerializeField] private AudioClip bgmClip;

    private AudioSource[] oneShotSources;
    private int channelCount;
    private int nextSfxSourceIndex;
    private int bgmIndex;
    private AudioVolumeSettings currentAudioSettings = AudioVolumeSettings.Default;

    public AudioVolumeSettings CurrentAudioSettings => currentAudioSettings;

    private void Awake()
    {
        channelCount = sfxSourceCount + bgmSourceCount;
        oneShotSources = new AudioSource[channelCount];

        GenerateSfxSources(gameObject, 0, sfxSourceCount);
        GenerateBgmSources(gameObject, sfxSourceCount, channelCount);
        LoadSavedAudioSettings();
    }

    private void Start()
    {
        // PlayBgm(bgmClip);
    }

    /// <summary>
    /// 현재 오디오 설정을 적용하고, 필요에 따라 PlayerPrefs에 저장합니다.
    /// </summary>
    /// <param name="settings"> 적용할 오디오 설정 </param>
    /// <param name="saveToPreferences"> PlayerPrefs에 저장할지 여부 </param>
    public void ApplyAudioSettings(AudioVolumeSettings settings, bool saveToPreferences)
    {
        currentAudioSettings = new AudioVolumeSettings(
            NormalizePercentage(settings.Bgm), 
            NormalizePercentage(settings.Sfx),
            NormalizePercentage(settings.Ui)
            );

        SetMixerVolume(bgmMixerGroup, BgmVolumeParameter, currentAudioSettings.Bgm);
        SetMixerVolume(sfxMixerGroup, SfxVolumeParameter, currentAudioSettings.Sfx);
        SetMixerVolume(uiMixerGroup, UiVolumeParameter, currentAudioSettings.Ui);

        if (saveToPreferences)
        {
            PlayerPrefs.SetFloat(BgmVolumePreference, currentAudioSettings.Bgm);
            PlayerPrefs.SetFloat(SfxVolumePreference, currentAudioSettings.Sfx);
            PlayerPrefs.SetFloat(UiVolumePreference, currentAudioSettings.Ui);
            PlayerPrefs.Save();
        }
    }

    private void LoadSavedAudioSettings()
    {
        AudioVolumeSettings savedSettings = new AudioVolumeSettings(
            PlayerPrefs.GetFloat(BgmVolumePreference, AudioVolumeSettings.Default.Bgm),
            PlayerPrefs.GetFloat(SfxVolumePreference, AudioVolumeSettings.Default.Sfx),
            PlayerPrefs.GetFloat(UiVolumePreference, AudioVolumeSettings.Default.Ui)
            );

        ApplyAudioSettings(savedSettings, false);
    }

    private static void SetMixerVolume(AudioMixerGroup mixerGroup, string parameterName, float percentage)
    {
        if (mixerGroup == null)
        {
            Debug.LogWarning($"[SoundManager] Mixer group for '{parameterName}' is not assigned.");
            return;
        }

        float decibels = PercentageToDecibels(percentage);
        
        if (!mixerGroup.audioMixer.SetFloat(parameterName, decibels))
        {
            Debug.LogWarning($"[SoundManager] Exposed AudioMixer parameter '{parameterName}' was not found on mixer '{mixerGroup.audioMixer.name}'.");
        }
    }

    private static float PercentageToDecibels(float percentage)
    {
        float normalized = NormalizePercentage(percentage) / 100f;
        return normalized <= 0f ? MinimumDecibels : Mathf.Log10(normalized) * 20f;
    }

    private static float NormalizePercentage(float percentage)
    {
        return Mathf.Clamp(percentage, 0f, 100f);
    }

    private void GenerateSfxSources(GameObject root, int startChannel, int endChannel)
    {
        GenerateAudioSources(root, startChannel, endChannel, sfxMixerGroup, "SFX_");
    }

    private void GenerateBgmSources(GameObject root, int startChannel, int endChannel)
    {
        GenerateAudioSources(root, startChannel, endChannel, bgmMixerGroup, "BGM_");
        bgmIndex = startChannel;
    }

    private void GenerateAudioSources(GameObject root, int startChannel, int endChannel, AudioMixerGroup mixerGroup, string mixerName = "OneShot_")
    {
        for (int i = startChannel; i < endChannel; i++)
        {
            GameObject channel = new GameObject($"{mixerName}{i}");
            channel.transform.SetParent(root.transform);

            AudioSource source = channel.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.outputAudioMixerGroup = mixerGroup;

            oneShotSources[i] = source;
        }
    }

    public void PlaySfxOneShotAt(AudioClip requestedSoundClip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (requestedSoundClip == null)
        {
            return;
        }

        AudioSource source = oneShotSources[nextSfxSourceIndex];
        nextSfxSourceIndex = (nextSfxSourceIndex + 1) % sfxSourceCount;

        source.transform.position = position;
        source.pitch = pitch;
        source.PlayOneShot(requestedSoundClip, volume);
    }

    public void PlaySfxOneShotAt(RandomSoundCue randomCue, Vector3 position)
    {
        if (randomCue == null)
        {
            return;
        }

        PlaySfxOneShotAt(randomCue.GetRandomClip(), position, randomCue.Volume, randomCue.Pitch);
    }

    public void PlayBgm(AudioClip requestedSoundClip, float volume = 1f, float pitch = 1f)
    {
        if (requestedSoundClip == null)
        {
            return;
        }

        AudioSource source = oneShotSources[bgmIndex];
        source.clip = requestedSoundClip;
        source.loop = true;
        source.pitch = pitch;
        source.volume = volume;
        source.Play();
    }

    public void StopBgm()
    {
        AudioSource source = oneShotSources[bgmIndex];
        source.Stop();
    }
}
