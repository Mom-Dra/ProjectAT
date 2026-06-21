using UnityEngine;
using PlayerStateMachine;
using EPOOutline;
using NUnit.Framework;

namespace Interactable
{
    public class DownedBody : InteractableObject, ICarriable
    {
        private LayerMask groundLayerMask;        
        protected Animator animator;
        private readonly int downedAnimationHash = Animator.StringToHash("DeadType");

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
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

        public override void OnHoverEnter()
        {
            if (!isSelected) outlinable.OutlineParameters.Enabled = true;
        }

        public override void OnHoverExit()
        {
            if (!isSelected) outlinable.OutlineParameters.Enabled = false;
        }

        public override void OnTargeted()
        {
            isSelected = true;
            outlinable.OutlineParameters.Enabled = true;
        }

        public override void OnUntargeted()
        {
            isSelected = false;
            outlinable.OutlineParameters.Enabled = false;
        }

        public override void OnInteractStart(PlayerController player)
        {
            //원래였으면 시체가 들어올려지는 애니메이션이 있어야하지만 없으므로 아무것도 하지 않음.
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