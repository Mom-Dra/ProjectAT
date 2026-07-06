using UnityEngine;

public class DoorSoundController : SoundControllerBase
{
    private const int OneShotChannel = 0;

    [Header("Door One Shot Sounds")]
    [SerializeField] private RandomSoundCue openStartCue;
    [SerializeField] private RandomSoundCue closeStartCue;
    
    public void PlayDoorMotionStartSound(bool targetOpen)
    {
        PlayOneShot(OneShotChannel, targetOpen ? openStartCue : closeStartCue);
    }
}
