using UnityEngine;
using PlayerStateMachine;
using EPOOutline;
using NUnit.Framework;

namespace Interactable
{
    public class DownedBody : InteractableObject, IUIHoverable, ICarriable
    {
        private LayerMask groundLayerMask;        
        protected Animator animator;
        private readonly int downedAnimationHash = Animator.StringToHash("DeadType");
        protected Outlinable outlinable;

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            outlinable = GetComponent<Outlinable>();
            PlayDownedAnimation(2);

            groundLayerMask = LayerMask.GetMask("Ground");
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
            //Do Nothing
            outlinable.OutlineParameters.Enabled = true;
        }

        public override void OnExecute(PlayerController player)
        {
            if(transform.parent == null)
            {
                StartCarrying(player.MyInteractionModule.HoldPoint);
            }
            else
            {
                StopCarrying();
            }
        }

        public override void OnInteractEnd(PlayerController player)
        {
            //nextState = (nextState == PlayerStateType.Carry) ? PlayerStateType.Normal : PlayerStateType.Carry;
            outlinable.OutlineParameters.Enabled = false;
        }

        public void StartCarrying(Transform holdPoint)
        {

            if (holdPoint != null)
            {
                transform.SetParent(holdPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0, -60, 0); 
                nextState = PlayerStateType.Normal;
            }
        }

        public void StopCarrying()
        {
            transform.SetParent(null);
            nextState = PlayerStateType.Carry;

            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, groundLayerMask))
            {
                transform.position = hit.point;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
        }
    }
}