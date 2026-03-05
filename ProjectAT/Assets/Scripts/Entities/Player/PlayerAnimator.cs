using MomDra.Weapon;
using UnityEngine;

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

    private static readonly int IsRunHash = Animator.StringToHash("IsRun");
    private static readonly int IsCrouchHash = Animator.StringToHash("Crouch_b");
    private static readonly int AttackkHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int SpeedHash = Animator.StringToHash("Speed_f");
    private static readonly int WeaponTypeHash = Animator.StringToHash("WeaponType_int");
    private static readonly int HeadHorizontalHash = Animator.StringToHash("Head_Horizontal_f");
    private static readonly int HeadVerticalHash = Animator.StringToHash("Head_Vertical_f");
    private static readonly int BodyHorizontalHash = Animator.StringToHash("Body_Horizontal_f");
    private static readonly int BodyVerticalHash = Animator.StringToHash("Body_Vertical_f");
    private static readonly int ShootHash = Animator.StringToHash("Shoot_b");
    private static readonly int IsDeadHash = Animator.StringToHash("Death_b");
    private static readonly int CancelTriggerHash = Animator.StringToHash("CancelTrigger");

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        entityStatus = GetComponent<EntityStatus>();
        //weaponHolder = GetComponentInChildren<WeaponHolder>();
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

    private void EntityDead()
    {
        SetIsDead(true);
    }

    private void EntityRevived()
    {
        SetIsDead(false);
    }

    public void SetIsDead(bool isDead)
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

    public void SetUpperBodyOffset(float headHorizontalOffset = 0.0f, float headVerticalOffset = 0.0f,  float bodyHorizontalOffset = 0.0f, float bodyVerticalOffset = 0.0f)
    {
        // animator.SetFloat(HeadHorizontalHash, headHorizontalOffset, 0.1f, Time.deltaTime);
        // animator.SetFloat(HeadVerticalHash, headVerticalOffset, 0.1f, Time.deltaTime);
        // animator.SetFloat(BodyHorizontalHash, bodyHorizontalOffset, 0.1f, Time.deltaTime);
        // animator.SetFloat(BodyVerticalHash, bodyVerticalOffset, 0.1f, Time.deltaTime);

        animator.SetFloat(HeadHorizontalHash, headHorizontalOffset);
        animator.SetFloat(HeadVerticalHash, headVerticalOffset);
        animator.SetFloat(BodyHorizontalHash, bodyHorizontalOffset);
        animator.SetFloat(BodyVerticalHash, bodyVerticalOffset);
    }

    public void ResetUpperBody()
    {
        SetUpperBodyOffset(0f, 0f);
    }

    public void SetRunState(bool isRunning)
    {
        animator.SetBool(IsRunHash, isRunning);
    }

    public void PlayAttack()
    {
        animator.SetTrigger(AttackkHash);
    }

    public void PlayHit()
    {
        animator.SetTrigger(HitHash);
    }

    public void PlayIdle()
    {
        weaponHolder.ChangeWeapon(WeaponHolder.WeaponSlot.Primary);
        animator.SetInteger(WeaponTypeHash, 2);
        SetUpperBodyOffset();
    }

    public void PlayGrenadeThrow()
    {
        animator.SetInteger(WeaponTypeHash, 10);
        weaponHolder.ChangeWeapon(WeaponHolder.WeaponSlot.Grenade);
    }

    public void PlayAiming()
    {
        SetUpperBodyOffset(-0.8f, 0.0f, 0.5f,0.0f); //MEMO : 애니메이션 로테이션 떄문에 하드코딩됨. 따로 정면으로 조준사격 하는 애니메이션 필요함.
    }

    public void OnAttackHitFrame()
    {
        SendMessageUpwards("ApplyDamageToTarget", SendMessageOptions.DontRequireReceiver);
    }

    public void CancelAnimation()
    {   
        animator.SetTrigger(CancelTriggerHash);
        PlayIdle();
    }
    public float GetSpeedValue()
    {
        return animator.GetFloat(SpeedHash);
    }

    public void PlayUseItem()
    {
        //붕대 사용하는 애니메이션 재생
        animator.SetInteger(WeaponTypeHash, 10);
    }
}
