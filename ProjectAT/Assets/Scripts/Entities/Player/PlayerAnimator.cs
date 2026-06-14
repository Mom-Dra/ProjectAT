using MomDra.Weapon;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
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

    [Header("Params")]
    [SerializeField] private float animationFPS = 30f;
    [SerializeField] private float suppressiveFireAimDistance = 10f;
    [SerializeField] private float suppressiveFireSweepSpeed = 8f;
    private Coroutine animationCoroutine;

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

        animationCoroutine = null;

    }

    private void OnDisable()
    {
        entityStatus.onDeath -= EntityDead;
        entityStatus.onRevive -= EntityRevived;
        weaponHolder.OnWeaponReloadStart -= PlayReloadAnimation;
        weaponHolder.OnWeaponReloaded -= StopReloadAnimation;

        DestroyAnimationCoroutine();
    }

    private void Start()
    {
        SetAiming(false, null);
    }

    private void LateUpdate()
    {
        if(animationCoroutine == null) UpdateAimMarkerPosition();
    }

    private void UpdateAimMarkerPosition()
    {
        AimMarkerTransform.position =  Vector3.up + (AimedTargetLocation != null ? AimedTargetLocation.position : transform.position + transform.forward * 10f);
    }

    private void DestroyAnimationCoroutine()
    {
        if(animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
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

    public void StartSuppressiveFireAnimation(float maxAngle)
    {
        SetAiming(true, null);

        if(animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(SuppreSiveFireAnimationCoroutine(maxAngle));
    }
    private IEnumerator SuppreSiveFireAnimationCoroutine(float maxAngle)
    {
        Vector3 baseDirection = transform.forward;
        baseDirection.y = 0f;

        if (baseDirection.sqrMagnitude < 0.001f)
            baseDirection = Vector3.forward;
        else
            baseDirection.Normalize();

        while (true)
        {
            float angle = Mathf.Sin(Time.time * suppressiveFireSweepSpeed) * maxAngle;
            Vector3 aimDirection = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;

            AimMarkerTransform.position =
                transform.position +
                aimDirection * suppressiveFireAimDistance +
                Vector3.up;

            yield return null;

            weaponHolder.FireWeaponOnlyVFX(AimMarkerTransform.position, Vector3.zero, true); // Suppressive Fire는 풀오토 발사 연출
        }
    }

    public void StopSuppressiveFireAnimation()
    {
        SetAiming(false, null);
        DestroyAnimationCoroutine();
    }
}