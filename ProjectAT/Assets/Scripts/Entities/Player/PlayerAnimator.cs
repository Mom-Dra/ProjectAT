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
    }

    private void OnEnable()
    {
        entityStatus.onDeath += EntityDead;
        entityStatus.onRevive += EntityRevived;
    }

    private void OnDisable()
    {
        entityStatus.onDeath -= EntityDead;
        entityStatus.onRevive -= EntityRevived;
    }

    private void Start()
    {
        SetAiming(false, null);
    }

    private void LateUpdate()
    {
        AimMarkerTransform.position =  Vector3.up + (AimedTargetLocation? AimedTargetLocation.position : transform.position + transform.forward * 10f);
    }

    private void EntityDead()
    {
        SetIsDead(true);
    }

    private void EntityRevived()
    {
        SetIsDead(false);
    }

    public void SetAimMarker(Transform tf)
    {
        AimedTargetLocation = tf;
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

    public void PlayIdle()
    {
        SetWeaponAnimation(2); //하드코딩됨. SetWeaponType로 대체 가능
        animator.SetBool(ShootHash, false);
    }

    public void SetWeaponAnimation(int WeaponType)
    {
        animator.SetInteger(WeaponTypeHash, WeaponType);
    }

    public void PlayTriggerAnimation(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public void PlayThrowAnimation()
    {
        animator.SetTrigger(ThrowTriggerHash);
    }

    public void SetAiming(bool isAiming = true, Transform targetTf = default)
    {
        if (targetTf != null)
        {
            animator.SetBool(ShootHash, true);
            aimRigBuilder.layers[0].active = true;
            AimedTargetLocation = targetTf;
        }
        else
        {
            animator.SetBool(ShootHash, false);
            aimRigBuilder.layers[0].active = false;
            AimedTargetLocation = null;
        }
    }

    public void CancelAnimation()
    {   
        animator.SetTrigger(CancelTriggerHash);
    }
}