using UnityEngine;


public class PlayerAnimationModule : MonoBehaviour
{
    [SerializeField] private Animator myAnim;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsFiring = Animator.StringToHash("isFiring");
    private static readonly int SnipingTrigger = Animator.StringToHash("SnipingTrigger");
    private static readonly int ThrowingTrigger = Animator.StringToHash("ThrowingTrigger");

    private bool isRunning = false;
    private bool isFiring = false;

    private void Awake()
    {
        myAnim = GetComponentInChildren<Animator>();
    }

    public void SetRunningAnimation(bool shouldBeRunning)
    {
        if (myAnim.GetBool(IsWalking) != shouldBeRunning)
        {
            myAnim.SetBool(IsWalking, shouldBeRunning);
            isRunning = shouldBeRunning;
        }
    }

    public void SetFiringAnimation(bool shouldBeFiring)
    {
        if (myAnim.GetBool(IsFiring) != shouldBeFiring)
        {
            myAnim.SetBool(IsFiring, shouldBeFiring);
            isFiring = shouldBeFiring;
        }
    }

    public void PlaySkillAnimation(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Throwing:
                myAnim.SetTrigger(ThrowingTrigger);
                break;
            case SkillType.Sniping:
                myAnim.SetTrigger(SnipingTrigger);
                break;
        }
    }
}
