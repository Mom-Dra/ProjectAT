using UnityEngine;

public class SoundManager
{
    private readonly AudioSource[] oneShotSources;
    private int nextIndex;

    public SoundManager(int channelCount = 16)
    {
        GameObject root = new GameObject("SoundManager_OnShotPool");
        Object.DontDestroyOnLoad(root);
        oneShotSources = new AudioSource[channelCount];

        GenerateAudioSources(root, channelCount);
    }

    private void GenerateAudioSources(GameObject root, int channelCount)
    {
        

        for(int i = 0; i < channelCount; i++)
        {
            GameObject channel = new GameObject($"OneShot_{i}");
            channel.transform.SetParent(root.transform);

            AudioSource source = channel.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;

            oneShotSources[i] = source;
        }
    }

    public void PlayOneShotAt(AudioClip requestedSoundClip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if(requestedSoundClip == null) return;

        AudioSource source = oneShotSources[nextIndex];
        nextIndex = (nextIndex + 1) % oneShotSources.Length;

        source.transform.position = position;
        source.pitch = pitch;
        source.PlayOneShot(requestedSoundClip, volume);
    }

    public void PlayOneShotAt(RandomSoundCue randomCue, Vector3 position)
    {
        if(randomCue == null) return;

        PlayOneShotAt(randomCue.GetRandomClip(), position, randomCue.Volume, randomCue.Pitch);
    }

}
