using MomDra.Weapon;
using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private EntityStatus entityStatus;

    private static readonly int IsRunHash = Animator.StringToHash("IsRun");
    private static readonly int IsCrouchHash = Animator.StringToHash("Crouch_b");
    private static readonly int AttackkHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int SpeedHash = Animator.StringToHash("Speed_f");
    private static readonly int WeaponTypeHash = Animator.StringToHash("WeaponType_int");
    private static readonly int HeadHorizontalHash = Animator.StringToHash("Head_Horizontal_f");
    private static readonly int BodyHorizontalHash = Animator.StringToHash("Body_Horizontal_f");
    private static readonly int ShootHash = Animator.StringToHash("Shoot_b");
    private static readonly int IsDeadHash = Animator.StringToHash("Death_b");

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        entityStatus = GetComponent<EntityStatus>();
    }

    private void OnEnable()
    {
        entityStatus.onDeath += EnemyDied;
    }

    private void OnDisable()
    {
        entityStatus.onDeath -= EnemyDied;
    }

    private void EnemyDied()
    {
        SetIsDead(true);
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
        animator.SetBool(ShootHash, isShoot);
    }

    public void SetUpperBodyOffset(float headOffset, float bodyOffset)
    {
        animator.SetFloat(HeadHorizontalHash, headOffset, 0.1f, Time.deltaTime);
        animator.SetFloat(BodyHorizontalHash, bodyOffset, 0.1f, Time.deltaTime);
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

    public void OnAttackHitFrame()
    {
        SendMessageUpwards("ApplyDamageToTarget", SendMessageOptions.DontRequireReceiver);
    }

    public void PlayCoverAnimation()
    {
        
    }

    //private IEnumerator aaCoroutine()
    //{
        
    //}
}
