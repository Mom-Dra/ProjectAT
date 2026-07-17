using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;

    [Header("Audio Sources Setting")]
    private AudioSource[] oneShotSources;
    [SerializeField] private int sfxSourceCount = 8;
    [SerializeField] private int bgmSourceCount = 1;
    private int channelCount = 0;
    private int nextSfxSourceIndex;
    private int bgmIndex;

    [Header("Temp Setting")]
    [SerializeField] private AudioClip bgmClip; //NOTE : 이거는 임시로 넣은거라 맵 정보를 나타내는 오브젝트에서 가져와야함.

    private void Awake()
    {
        channelCount = sfxSourceCount + bgmSourceCount;
        oneShotSources = new AudioSource[channelCount];

        GenerateSfxSources(gameObject, 0, sfxSourceCount);
        GenerateBgmSources(gameObject, sfxSourceCount, channelCount);
    }

    private void Start()
    {
        PlayBgm(bgmClip);
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
        if (requestedSoundClip == null) return;

        AudioSource source = oneShotSources[nextSfxSourceIndex];
        nextSfxSourceIndex = (nextSfxSourceIndex + 1) % sfxSourceCount;

        source.transform.position = position;
        source.pitch = pitch;
        source.PlayOneShot(requestedSoundClip, volume);
    }

    public void PlaySfxOneShotAt(RandomSoundCue randomCue, Vector3 position)
    {
        if (randomCue == null) return;

        PlaySfxOneShotAt(randomCue.GetRandomClip(), position, randomCue.Volume, randomCue.Pitch);
    }

    public void PlayBgm(AudioClip requestedSoundClip, float volume = 1f, float pitch = 1f)
    {
        if (requestedSoundClip == null) return;

        AudioSource source = oneShotSources[bgmIndex];
        source.clip = requestedSoundClip;
        source.loop = true;
        source.pitch = pitch;

        source.Play();
    }

    public void StopBgm()
    {
        AudioSource source = oneShotSources[bgmIndex];
        source.Stop();
    }
}
