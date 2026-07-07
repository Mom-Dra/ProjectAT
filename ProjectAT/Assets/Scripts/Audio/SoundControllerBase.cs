using UnityEngine;

public class SoundControllerBase : MonoBehaviour
{
    [SerializeField] private AudioSource[] channels;

    protected AudioSource GetChannel(int index)
    {
        if(!CheckChannelIndex(index)) return null;
        return channels[index];
    }

    private bool CheckChannelIndex(int index)
    {
        if(channels == null || index < 0 || index >= channels.Length)
        {
            Debug.LogWarning($"SoundControllerBase : Invalid channel index {index}. Channel count is {channels?.Length ?? 0}");
            return false;
        }
        return true;
    }

    protected void PlayOneShot(int channelIndex, AudioClip requestedSoundClip, float volume = 1f)
    {
        AudioSource source = GetChannel(channelIndex);
        if(source == null || requestedSoundClip == null) return;

        source.PlayOneShot(requestedSoundClip, volume);
    }

    protected void PlayOneShot(int channelIndex, RandomSoundCue randomCue)
    {
        if(randomCue == null) return;

        AudioSource source = GetChannel(channelIndex);
        AudioClip clip = randomCue.GetRandomClip();

        if(source == null || clip == null) return;
        
        source.pitch = randomCue.Pitch;
        source.PlayOneShot(clip, randomCue.Volume);
    }

    protected void Play(int channelIndex, AudioClip requestedClip, float volume =1f, float pitch = 1f, bool loop = false)
    {
        AudioSource source = GetChannel(channelIndex);
        if(source == null || requestedClip == null) return;

        source.clip = requestedClip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.Play();
    }

    protected void Stop(int channelIndex)
    {
        AudioSource source = GetChannel(channelIndex);
        if(source == null) return;

        source.Stop();
    }
}
