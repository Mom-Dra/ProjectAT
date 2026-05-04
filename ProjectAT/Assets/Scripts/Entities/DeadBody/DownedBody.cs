using UnityEngine;
using PlayerStateMachine;
using EPOOutline;
using NUnit.Framework;

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

        public override void OnInteractStart(PlayerController player){ }

        public override void OnExecute(PlayerController player)
        {
            if(transform.parent == null)
            {
                StartCarrying(player);
            }
            else
            {
                StopCarrying();
            }
        }

        public override void OnInteractEnd(PlayerController player)
        {
            nextState = (transform.parent == null) ? PlayerStateType.Carry : PlayerStateType.Normal;
        }

        public virtual void StartCarrying(PlayerController carrier)
        {
            rb.isKinematic = true;

            if (carrier.MyInteractionModule.HoldPoint != null)
            {
                transform.SetParent(carrier.MyInteractionModule.HoldPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(300, 0, 0); 
            }
        }

        public virtual void StopCarrying()
        {
            Debug.Log("StopCarrying 호출! 시체를 바닥에 내려놓습니다.");
            transform.SetParent(null);
            rb.isKinematic = false;
        }
    }
}