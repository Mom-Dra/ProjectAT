using UnityEngine;

public class PlayerAnimationModule : MonoBehaviour
{
    [SerializeField] private Animator myAnim;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int FiringTrigger = Animator.StringToHash("FiringTrigger");

    private bool isRunning = false;

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
    public void PlayFiringAnimation()
    {
        myAnim.SetTrigger(FiringTrigger);
    }

    public void PlayThrowingAnimation()
    {
        myAnim.SetTrigger("ThrowingTrigger");
    }
}
