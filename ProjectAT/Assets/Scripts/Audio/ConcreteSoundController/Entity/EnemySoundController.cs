using UnityEngine;

public class EnemySoundController : SoundControllerBase
{
    private const int VoiceChannel = 0;

    [Header("[Enemy Sound Controller]")]
    [Header("References")]
    [SerializeField] private Enemy owner;
    [SerializeField] private EntityStatus ownerStatus;
    [SerializeField] private AwarenessModule  awarenessModule;
    [SerializeField] private PerceptionSystem perceptionSystem;

    [Header("Voice Cues")]
    [SerializeField] private RandomSoundCue deathCue;
    [SerializeField] private RandomSoundCue targetDetectedCue;
    [SerializeField] private RandomSoundCue suspiciousCue;
    [SerializeField] private RandomSoundCue combatWarCryCue;

    [Header("Voice Settings")]
    [SerializeField] private bool autoPlayCombatWarCry = true;
    [SerializeField, Min(0f)] private float suspiciousSoundInterval = 2f;
    [SerializeField, Min(0.1f)] private float combatWarCryInterval = 10f;
    [SerializeField] private Vector2 combatWarCryIntervalJitter = new Vector2(-2f, 5f);

    private float lastSuspiciousSoundTime = float.NegativeInfinity;
    private float nextCombatWarCryTime;

    private void Awake()
    {
        if (owner == null) owner = GetComponentInParent<Enemy>();
        if (ownerStatus == null) ownerStatus = GetComponentInParent<EntityStatus>();
        if (awarenessModule == null) awarenessModule = GetComponentInParent<AwarenessModule>();
        if (perceptionSystem  == null) perceptionSystem  = GetComponentInParent<PerceptionSystem>();

        ScheduleNextCombatWarCry();  
    }

    private void OnEnable()
    {
        if (ownerStatus != null)
        {
            ownerStatus.onDeath += HandleDeath;
        }

        if (owner != null)
        {
            owner.onTargetDetected += HandleTargetDetected;
        }

        if (awarenessModule != null)
        {
            awarenessModule.onScanCanceled += HandleScanStarted;
        }

        if (perceptionSystem != null)
        {
            perceptionSystem.onNoiseDetected += HandleNoiseDetected;
        }
    }

    private void OnDisable()
    {
        if (ownerStatus != null)
        {
            ownerStatus.onDeath -= HandleDeath;
        }

        if (owner != null)
        {
            owner.onTargetDetected -= HandleTargetDetected;
        }

        if (awarenessModule != null)
        {
            awarenessModule.onScanStarted -= HandleScanStarted;
        }

        if (perceptionSystem != null)
        {
            perceptionSystem.onNoiseDetected -= HandleNoiseDetected;
        }
    }

    private void Update()
    {
        if(!autoPlayCombatWarCry) return;
        if(owner == null || !owner.IsInCombat) return;
        if(Time.time < nextCombatWarCryTime) return;

        PlayCombatWarCrySound();
        ScheduleNextCombatWarCry();
    }

    public void PlayDeathSound()
    {
        PlayOneShot(VoiceChannel, deathCue);
    }

    public void PlayTargetDetectedSound()
    {
        PlayOneShot(VoiceChannel, targetDetectedCue);
    }

    public void PlaySuspiciousSound()
    {
        if(Time.time - lastSuspiciousSoundTime < suspiciousSoundInterval) return;
        lastSuspiciousSoundTime = Time.time;
        PlayOneShot(VoiceChannel, suspiciousCue);
    }

    public void PlayCombatWarCrySound()
    {
        PlayOneShot(VoiceChannel, combatWarCryCue);
    }

    private void HandleDeath()
    {
        PlayDeathSound();
    }

    private void HandleTargetDetected(ISquadMember reporter, IPerceivable target)
    {
        PlayTargetDetectedSound();
        ScheduleNextCombatWarCry();
    }

    private void HandleScanStarted(IPerceivable target)
    {
        PlaySuspiciousSound();
    }

    private void HandleNoiseDetected(Vector3 noisePosition)
    {
        PlaySuspiciousSound();
    }

    private void ScheduleNextCombatWarCry()
    {
        float jitter = Random.Range(combatWarCryIntervalJitter.x, combatWarCryIntervalJitter.y);
        nextCombatWarCryTime = Time.time + combatWarCryInterval + jitter;
    }
}
