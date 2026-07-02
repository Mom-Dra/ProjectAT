using UnityEngine;

public class PlayerSoundController : SoundControllerBase
{
    private const int FootstepChannel = 0;
    private const int VoiceChannel = 1;
    private const int ActionChannel = 2;

    [Header("References")]
    [SerializeField] private PlayerMovementModule movementModule;
    [SerializeField] private EntityStatus entityStatus;

    [Header("Random Sound Cues")]
    [SerializeField] private RandomSoundCue footstepCue;
    [SerializeField] private RandomSoundCue pullingGrenadePinCue;
    [SerializeField] private RandomSoundCue commandConfirmCue;
    [SerializeField] private RandomSoundCue exertionCue;

    [Header("Foot Step Settings")]
    [SerializeField] private bool autoPlayFootsteps = true;
    [SerializeField] private bool playOnlyWhenRunning = true;
    [SerializeField] private float minFootstepVelocity = 0.1f;
    [SerializeField] private float walkStepInterval = 0.48f;
    [SerializeField] private float runStepInterval = 0.32f;

    [SerializeField] private float fallbackWalkSpeed = 3.5f; //NOTE : 이거 두개는 뭐임?
    [SerializeField] private float fallbackRunSpeed = 6f;   

    [Header("Durations")]
    [SerializeField] private float commandConfirmDuration = 0.2f;
    [SerializeField] private float exertionDuration = 4f;

    [Header("Low Health")]
    [SerializeField] private bool playExertionOnLowHealth = true;
    [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.35f; //NOTE : 이거는 빈사를 구분짓는 기준이 될 수 있으므로 entityStatus쪽으로 가는게?

    private float footstepTimer;
    private float currentCommandConfirmTime;
    private float currentExertionTime;

    private float ownerEntityWalkSpeed => entityStatus != null ? entityStatus.WalkSpeed : fallbackWalkSpeed;
    private float ownerEntityRunSpeed => entityStatus != null ? entityStatus.RunSpeed : fallbackRunSpeed;

    private void Awake()
    {
        if(movementModule == null) movementModule = GetComponentInParent<PlayerMovementModule>();
        if(entityStatus == null) entityStatus = GetComponentInParent<EntityStatus>();
    }

    private void OnEnable()
    {
        if(entityStatus != null) entityStatus.onHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if(entityStatus != null) entityStatus.onHealthChanged -= HandleHealthChanged;
    }

    private void Update()
    {
        if (autoPlayFootsteps)
        {
            UpdateFootsteps();
        }
    }

    #region Common Situation Sounds
    public void PlayCommandConfirmSound()
    {
        if(Time.time - currentCommandConfirmTime < commandConfirmDuration) return;
        currentCommandConfirmTime = Time.time;

        PlayOneShot(VoiceChannel, commandConfirmCue);
    }

    public void PlayFootstepSound()
    {
        PlayOneShot(FootstepChannel, footstepCue);
    }

    private void UpdateFootsteps()
    {
        if(movementModule == null) return;
        if (!movementModule.IsAgentMoving())
        {
            footstepTimer = 0f;
            return;
        }

        float velocity = movementModule.GetVelocity();
        if(velocity < minFootstepVelocity || (playOnlyWhenRunning && !IsRunning(velocity)))
        {
            footstepTimer = 0f;
            return;
        }


        footstepTimer -= Time.deltaTime;
        if(footstepTimer > 0f) return;

        PlayFootstepSound();
        footstepTimer = GetFootstepInterval(velocity);
    }

    private float GetFootstepInterval(float currentVelocity)
    {
        float speedRatio = Mathf.InverseLerp(ownerEntityWalkSpeed, ownerEntityRunSpeed, currentVelocity);
        return Mathf.Lerp(walkStepInterval, runStepInterval, speedRatio);
    }

    private bool IsRunning(float currentVelocity)
    {
        float runThreshold = Mathf.Lerp(ownerEntityWalkSpeed, ownerEntityRunSpeed, 0.65f);
        return currentVelocity >= runThreshold;
    }
    #endregion
    
    #region Combat Situation Sounds
    public void PlayPullingGrenadePinSound()
    {
        PlayOneShot(ActionChannel, pullingGrenadePinCue);
    }

    public void PlayExertionSound()
    {
        if(Time.time - currentExertionTime < exertionDuration) return;
        currentExertionTime = Time.time;

        PlayOneShot(VoiceChannel, exertionCue);
    }

    private void HandleHealthChanged(float healthRatio)
    {
        if (!playExertionOnLowHealth) return;
        if (healthRatio > lowHealthThreshold) return;

        PlayExertionSound();
    }
    #endregion

}
