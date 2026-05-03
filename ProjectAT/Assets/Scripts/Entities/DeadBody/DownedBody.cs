using UnityEngine;
using PlayerStateMachine;
using EPOOutline;

namespace Interactable
{
    [RequireComponent(typeof(Rigidbody))]
    public class DownedBody : InteractableObject, IUIHoverable
    {
        protected Animator animator;
        private readonly int downedAnimationHash = Animator.StringToHash("DeadType");
        protected Rigidbody rb;
        protected Outlinable outlinable;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            outlinable = GetComponent<Outlinable>();
            PlayDownedAnimation(2);
        }

        public void PlayDownedAnimation(int type = 1)
        {
            if (animator != null)
            {
                animator.SetInteger(downedAnimationHash, type);
            }
        }

        public override Vector3 GetInteractLookDir(Transform playerTransform)
        {
            // 상호작용 시 플레이어가 시체를 바라보도록 방향 계산 (Y축 회전만 고려)
            Vector3 dir = transform.position - playerTransform.position;
            dir.y = 0; 
            return dir.normalized;
        }

        public override Vector3 GetInteractPosition(Transform playerTransform)
        {
            return transform.position;
        }

        public void OnHoverEnter()
        {
            outlinable.OutlineParameters.Enabled = true;
        }

        public void OnHoverExit()
        {
            outlinable.OutlineParameters.Enabled = false;
        }

        public override void OnTargetSelected()
        {
            outlinable.OutlineParameters.Enabled = true;
        }

        public override void OnTargetDeselected()
        {
            outlinable.OutlineParameters.Enabled = false;
        }

        public override void OnInteractStart(PlayerController player)
        {
            // TODO: "시체 드는 중..." 상호작용 프로그레스 바 UI 호출 (필요 시)
        }

        public override void OnExecute(PlayerController player)
        {
            // 1. 아무도 사용 중이지 않다면 -> 듭니다.
            if (!IsInUse)
            {
                StartCarrying(player);
            }
            // 2. 이미 사용 중인데, 그게 나 자신이라면 -> 내려놓습니다.
            else if (CurrentInteractor == player.gameObject)
            {
                StopCarrying();
            }
        }

        public virtual void StartCarrying(PlayerController carrier)
        {
            // 락을 걸어서 IsInUse를 true로 만듭니다. (실패 시 중단)
            if (!TryLock(carrier)) return; 
            
            rb.isKinematic = true;

            if (carrier.MyInteractionModule.HoldPoint != null)
            {
                transform.SetParent(carrier.MyInteractionModule.HoldPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(-90, 0, 0); 
            }
        }

        public virtual void StopCarrying()
        {
            transform.SetParent(null);
            rb.isKinematic = false;
            
            UnLock();             // 락을 해제하여 IsInUse를 false로 만듭니다.
        }
    }
}