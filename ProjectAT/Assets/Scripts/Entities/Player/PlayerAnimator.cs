using MomDra.Weapon;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private EntityStatus entityStatus;
    [SerializeField] private WeaponHolder weaponHolder;
    
    #region  Animation Hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed_f");
    private static readonly int IsCrouchHash = Animator.StringToHash("Crouch_b");
    private static readonly int WeaponTypeHash = Animator.StringToHash("WeaponType_int");
    private static readonly int ShootHash = Animator.StringToHash("Shoot_b");
    private static readonly int RealoadHash= Animator.StringToHash("Reload_b");
    private static readonly int FullAutoHash = Animator.StringToHash("FullAuto_b");

    private static readonly int HeadHorizontalHash = Animator.StringToHash("Head_Horizontal_f");
    private static readonly int HeadVerticalHash = Animator.StringToHash("Head_Vertical_f");
    private static readonly int IsDeadHash = Animator.StringToHash("Death_b");
    
    private static readonly int CancelTriggerHash = Animator.StringToHash("Cancel_t");
    private static readonly int ThrowTriggerHash = Animator.StringToHash("Throw_t");
    #endregion

    [SerializeField] private float animationFPS = 30f;

    private Coroutine headLookCoroutine;
    [SerializeField] private Transform AimMarkerTransform;
    [SerializeField] private Transform AimedTargetLocation;
    [SerializeField] private RigBuilder aimRigBuilder;


    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        entityStatus = GetComponent<EntityStatus>();
        aimRigBuilder = GetComponentInChildren<RigBuilder>();
        weaponHolder = GetComponentInChildren<WeaponHolder>();
    }

    private void OnEnable()
    {
        entityStatus.onDeath += EntityDead;
        entityStatus.onRevive += EntityRevived;
        weaponHolder.OnWeaponReloaded += StopReloadAnimation;
        weaponHolder.OnWeaponReloadStart += PlayReloadAnimation;

    }

    private void OnDisable()
    {
        entityStatus.onDeath -= EntityDead;
        entityStatus.onRevive -= EntityRevived;
        weaponHolder.OnWeaponReloadStart -= PlayReloadAnimation;
        weaponHolder.OnWeaponReloaded -= StopReloadAnimation;
    }

    private void Start()
    {
        SetAiming(false, null);
    }

    private void LateUpdate()
    {
        UpdateAimMarkerPosition();
    }

    private void UpdateAimMarkerPosition()
    {
        AimMarkerTransform.position =  Vector3.up + (AimedTargetLocation != null ? AimedTargetLocation.position : transform.position + transform.forward * 10f);
    }

    private void EntityDead()
    {
        SetIsDead(true);
    }

    private void EntityRevived()
    {
        SetIsDead(false);
    }

    private void SetIsDead(bool isDead)
    {
        animator.SetBool(IsDeadHash, isDead);
    }

    public void SetSpeed(float speed)
    {
        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
    }

    public void SetCrouch(bool isCrouch)
    {
        animator.SetBool(IsCrouchHash, isCrouch);
    }

    public void PlayThrowAnimation()
    {
        animator.SetTrigger(ThrowTriggerHash);
    }

    public void SetAiming(bool isAiming = true, Transform targetTf = default)
    {
        animator.SetBool(ShootHash, isAiming);
        aimRigBuilder.layers[0].active = isAiming;
        AimedTargetLocation = targetTf;
    }

    public void CancelAnimation()
    {   
        animator.SetTrigger(CancelTriggerHash);
    }

    public void WeaponMeshVisible(bool isVisible)
    {
        weaponHolder.NowWeaponVisible(isVisible);
    }

    public void PlayReloadAnimation(Gun gun)
    {
        animator.SetBool(RealoadHash, true);
    }

    public void StopReloadAnimation(Gun gun)
    {
        animator.SetBool(RealoadHash, false);
    }
}