using UnityEngine;

public class PlayerSoundController : SoundControllerBase
{
    private const int FootstepChannel = 0;
    private const int VoiceChannel = 1;
    private const int ActionChannel = 2;
    private const int exertionChannel = 3;

    [Header("References")]
    [SerializeField] private PlayerMovementModule movementModule;
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private EntityStatus entityStatus;

    [Header("Random Sound Cues")]
    [SerializeField] private RandomSoundCue footstepCue;
    [SerializeField] private RandomSoundCue hitCue;
    [SerializeField] private AudioClip exertionCue;

    [Header("Foot Step Settings")]
    [SerializeField] private bool autoPlayFootsteps = true;
    [SerializeField] private bool playOnlyWhenRunning = true;
    [SerializeField] private float minFootstepVelocity = 0.1f;
    [SerializeField] private float walkStepInterval = 0.48f;
    [SerializeField] private float runStepInterval = 0.32f;

    [SerializeField] private float fallbackWalkSpeed = 3.5f; //NOTE : 이거 두개는 뭐임?
    [SerializeField] private float fallbackRunSpeed = 6f;   

    [Header("Durations")]
    [SerializeField] private float hitSoundInterval = 0.5f;

    [Header("Low Health")]
    [SerializeField] private bool playExertionOnLowHealth = true;

    private float footstepTimer;
    private float currentHitTime;

    private float ownerEntityWalkSpeed => entityStatus != null ? entityStatus.WalkSpeed : fallbackWalkSpeed;
    private float ownerEntityRunSpeed => entityStatus != null ? entityStatus.RunSpeed : fallbackRunSpeed;

    private void Awake()
    {
        if(movementModule == null) movementModule = GetComponentInParent<PlayerMovementModule>();
        if(entityStatus == null) entityStatus = GetComponentInParent<EntityStatus>();
        if (weaponHolder == null) weaponHolder = GetComponentInChildren<WeaponHolder>();
    }

    private void OnEnable()
    {
        if(entityStatus != null) 
        {
            entityStatus.onHealthChanged += HandleHealthChanged;
            entityStatus.onLowHealthWarning += HandleLowHealthWarning;
            entityStatus.onLowHealthWarningEnd += HandleLowHealthWarningEnd;
        }

        if (weaponHolder != null)
        {
            weaponHolder.OnWeaponFired += HandleWeaponFired;
            weaponHolder.OnWeaponReloadStart += HandleWeaponReloadedStart;
            weaponHolder.OnWeaponReloaded += HandleWeaponReloadedEnd;
        }   
    }

    private void OnDisable()
    {
        if(entityStatus != null) 
        {
            entityStatus.onHealthChanged -= HandleHealthChanged;
            entityStatus.onLowHealthWarning -= HandleLowHealthWarning;
            entityStatus.onLowHealthWarningEnd -= HandleLowHealthWarningEnd;
        }
        if (weaponHolder != null)
        {
            weaponHolder.OnWeaponFired -= HandleWeaponFired;
            weaponHolder.OnWeaponReloadStart -= HandleWeaponReloadedStart;
            weaponHolder.OnWeaponReloaded -= HandleWeaponReloadedEnd;
        }
    }

    private void Update()
    {
        if (autoPlayFootsteps)
        {
            UpdateFootsteps();
        }
    }

    #region SoundRequests API
    public bool PlayOneShot(AudioClip clip)
    {
        SoundManager soundManager = SoundManager.Instance;
        if(soundManager == null || clip == null) return false;

        soundManager.PlaySfxOneShotAt(clip, transform.position);
        return true;
    }
    #endregion

    #region Common Situation Sounds

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
    public void PlayHitSound()
    {
        if(Time.time - currentHitTime < hitSoundInterval) return;
        PlayOneShot(VoiceChannel, hitCue);
        currentHitTime = Time.time;
    }
    
    private void HandleHealthChanged(float healthRatio)
    {
        PlayHitSound();
    }

    public void PlayWeaponFireSound(Gun gun)
    {
        Debug.Log("Enter PlayerWeaponFire");
        if (gun == null || gun.GunData == null) return;
        Debug.Log("Fire Sound Play");
        PlayOneShot(ActionChannel, gun.GunData.ShotClip);
    }

    private void HandleWeaponFired(Gun gun)
    {
        PlayWeaponFireSound(gun);
    }

    private void HandleWeaponReloadedStart(Gun gun)
    {
        if (gun == null || gun.GunData == null) return;

        PlayOneShot(ActionChannel, gun.GunData.ReloadStartClip);
    }
    private void HandleWeaponReloadedEnd(Gun gun)
    {
        if (gun == null || gun.GunData == null) return;

        PlayOneShot(ActionChannel, gun.GunData.ReloadEndClip);
    }

    private void HandleLowHealthWarning()
    {
        if(playExertionOnLowHealth)
        {
            Play(exertionChannel, exertionCue);
        }
    }
    private void HandleLowHealthWarningEnd()
    {
        Stop(exertionChannel);
    }
    #endregion

}
