using UnityEngine;
using UnityEngine.Audio;
using ProjectAT.Option;

public class SoundManager : MonoBehaviour
{

    #region Parameter Names
    private const string BgmVolumeParameter = "BGMVolume";
    private const string SfxVolumeParameter = "SFXVolume";
    private const string UiVolumeParameter = "UIVolume";

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

    private AudioOptionSetting currentAudioSettings = AudioOptionSetting.Default;
    public AudioOptionSetting CurrentAudioSettings => currentAudioSettings;

    private void Awake()
    {
        channelCount = sfxSourceCount + bgmSourceCount;
        oneShotSources = new AudioSource[channelCount];

        GenerateSfxSources(gameObject, 0, sfxSourceCount);
        GenerateBgmSources(gameObject, sfxSourceCount, channelCount);
    }

    private void Start()
    {
        // PlayBgm(bgmClip);
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

    public void ApplyAudioSettings(AudioOptionSetting settings)
    {
        currentAudioSettings = new AudioOptionSetting(
            NormalizePercentage(settings.Bgm),
            NormalizePercentage(settings.Sfx),
            NormalizePercentage(settings.Ui)
        );

        SetMixerVolume(bgmMixerGroup, BgmVolumeParameter, currentAudioSettings.Bgm);
        SetMixerVolume(sfxMixerGroup, SfxVolumeParameter, currentAudioSettings.Sfx);
        SetMixerVolume(uiMixerGroup, UiVolumeParameter, currentAudioSettings.Ui);
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
