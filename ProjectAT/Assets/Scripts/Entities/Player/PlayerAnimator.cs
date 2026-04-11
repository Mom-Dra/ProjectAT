using System.Collections;
using MomDra.Weapon;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum SkillAnimationType : ushort
{
    ThrowGrenade,
    TargetAndFire
}

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
    #endregion

    [SerializeField] private float animationFPS = 30f;

    private Coroutine headLookCoroutine;
    [SerializeField] private Transform TargetTransform;
    [SerializeField] private Transform AimedTargetLocation;
    [SerializeField] private RigBuilder aimRigBuilder;


    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        entityStatus = GetComponent<EntityStatus>();
        weaponHolder = GetComponentInChildren<WeaponHolder>();
        aimRigBuilder = GetComponentInChildren<RigBuilder>();
    }

    private void OnEnable()
    {
        entityStatus.onDeath += EntityDead;
        entityStatus.onRevive += EntityRevived;
        //weaponHolder.OnWeaponFired += PlayWeaponFireOnce;
    }

    private void OnDisable()
    {
        entityStatus.onDeath -= EntityDead;
        entityStatus.onRevive -= EntityRevived;
        //weaponHolder.OnWeaponFired -= PlayWeaponFireOnce;
    }

    private void Start()
    {
        SetAiming(false);
    }

    private void LateUpdate()
    {
        TargetTransform.position = transform.position + Vector3.up + (AimedTargetLocation? AimedTargetLocation.position : transform.forward * 10f);
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

    public void SetWeaponType(WeaponType weaponType)
    {
        animator.SetInteger(WeaponTypeHash, (int)weaponType);
    }

    public void SetShoot(bool isShoot)
    {
        if(animator.GetBool(ShootHash) != isShoot)
            animator.SetBool(ShootHash, isShoot);
    }

    public void PlayIdle()
    {
        // weaponHolder.ChangeWeapon(WeaponHolder.WeaponSlot.Primary);
        // animator.SetInteger(WeaponTypeHash, 2);
        weaponHolder.ChangeWeapon(weaponHolder.NowWeaponSlot);
        animator.SetInteger(WeaponTypeHash, 2);
        animator.SetBool(ShootHash, false);
        SetHeadLookDirection(0f, 0f);
    }

    public void PlayGrenadeThrow(float t = 1.0f)
    {
        float targetSpeed = 48.0f/ (animationFPS * t);

        animator.SetInteger(WeaponTypeHash, 10);
        weaponHolder.ChangeProjectileWeapon(WeaponHolder.WeaponSlot.Grenade);
        //animator.SetFloat("ThrowSpeed", targetSpeed);
        //PlayAiming(false);
    }

    public void SetAiming(bool isAiming = true, Transform targetTf = default)
    {
        if (isAiming)
        {
            //SetHeadLookDirection(-0.8f, 0f);
            animator.SetBool(ShootHash, true);
            aimRigBuilder.layers[0].active = true;
            AimedTargetLocation = targetTf;
        }
        else
        {
            animator.SetBool(ShootHash, false);
            aimRigBuilder.layers[0].active = false;
            AimedTargetLocation = null;
           // SetHeadLookDirection(0f, 0f);
        }
    }

    public void OnAttackHitFrame()
    {
        SendMessageUpwards("ApplyDamageToTarget", SendMessageOptions.DontRequireReceiver);
    }

    public void CancelAnimation()
    {   
        animator.SetTrigger(CancelTriggerHash);
        //PlayIdle();
    }

    public void SetHeadLookDirection(float horizontal, float vertical)
    {
        if (headLookCoroutine != null)
        {
            StopCoroutine(headLookCoroutine);
        }

        headLookCoroutine = StartCoroutine(SmoothHeadLook(horizontal, vertical));
    }

    private IEnumerator SmoothHeadLook(float horizontal, float vertical, float duration = 0.5f)
    {
        float elapsed = 0f;

        float initialHeadHorizontal = animator.GetFloat(HeadHorizontalHash);
        float initialHeadVertical = animator.GetFloat(HeadVerticalHash);

        float targetHeadHorizontal = Mathf.Clamp(horizontal, -1f, 1f);
        float targetHeadVertical = Mathf.Clamp(vertical, -1f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float newHeadHorizontal = Mathf.Lerp(initialHeadHorizontal, targetHeadHorizontal, t);
            float newHeadVertical = Mathf.Lerp(initialHeadVertical, targetHeadVertical, t);

            animator.SetFloat(HeadHorizontalHash, newHeadHorizontal);
            animator.SetFloat(HeadVerticalHash, newHeadVertical);

            yield return null;
        }

        animator.SetFloat(HeadHorizontalHash, targetHeadHorizontal);
        animator.SetFloat(HeadVerticalHash, targetHeadVertical);
    }
}