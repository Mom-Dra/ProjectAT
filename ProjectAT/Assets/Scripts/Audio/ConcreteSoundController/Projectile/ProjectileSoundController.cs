using UnityEngine;

public class ProjectileSoundController : SoundControllerBase
{
    private const int ImpactChannel = 0;
    [Header("Random Sound Cues")]
    [SerializeField] private RandomSoundCue impactCue;
    [SerializeField] private RandomSoundCue explosionCue;

    [Header("Options")]
    [SerializeField] private bool playImpactOnlyOnce = true;
    
    private bool impactPlayed;

    private void OnEnable()
    {
        impactPlayed = false;
    }

    public void PlayImpact()
    {
        if(playImpactOnlyOnce && impactPlayed) return;

        impactPlayed = true;
        PlayOneShot(ImpactChannel, impactCue);
    }

    public void PlayExplosionAt(Vector3 position)
    {
        if(explosionCue == null) return;

        Managers.Instance.SoundManager.PlayOneShotAt(explosionCue, position);
    }
}
