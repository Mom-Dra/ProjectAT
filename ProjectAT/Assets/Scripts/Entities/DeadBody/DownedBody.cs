using UnityEngine;
using PlayerStateMachine;
using EPOOutline;
using System.Collections.Generic;

namespace Interactable
{
    public class DownedBody : InteractableObject, ICarriable, IBushHideable
    {
        private LayerMask groundLayerMask;
        private const float BushSyncRadius = 0.25f;

        [SerializeField] private LayerMask bushMask = ~0;
        [SerializeField] private DitherFadeController fadeController;

        protected Animator animator;
        private readonly int downedAnimationHash = Animator.StringToHash("DeadType");
        protected Outlinable outlinable;

        private readonly HashSet<Bush> hidingBushes = new HashSet<Bush>();
        private readonly Collider[] bushColliders = new Collider[8];
        private Collider corpseCollider;

        public Transform Transform => transform;
        public bool IsHidden => hidingBushes.Count > 0;

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            outlinable = GetComponent<Outlinable>();
            corpseCollider = GetComponent<Collider>();
            fadeController = GetComponent<DitherFadeController>();

            PlayDownedAnimation(2);

            groundLayerMask = LayerMask.GetMask("Ground");

            SyncHiddenVisual();
        }

        public void EnterBush(Bush bush)
        {
            if (bush is null) return;

            hidingBushes.Add(bush);
            SyncHiddenVisual();
        }

        public void ExitBush(Bush bush)
        {
            if (bush is null) return;

            hidingBushes.Remove(bush);
            SyncHiddenVisual();
        }

        public bool CanBeDetectedBy(Transform observer)
        {
            RefreshHidingBushes();

            if (!IsHidden) return true;
            if (observer is null) return false;

            foreach (Bush bush in hidingBushes)
            {
                if (bush is not null && bush.Contains(observer))
                    return true;
            }

            return false;
        }

        private void RefreshHidingBushes()
        {
            hidingBushes.Clear();

            Bounds bounds = GetBushSyncBounds();
            float radius = Mathf.Max(BushSyncRadius, bounds.extents.magnitude);

            int count = Physics.OverlapSphereNonAlloc(
                bounds.center,
                radius,
                bushColliders,
                bushMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; ++i)
            {
                Bush bush = bushColliders[i].GetComponentInParent<Bush>();
                if (bush == null) continue;
                if (!IsOverlappingCorpse(bushColliders[i])) continue;

                hidingBushes.Add(bush);
            }

            SyncHiddenVisual();
        }

        private Bounds GetBushSyncBounds()
        {
            if (corpseCollider != null)
                return corpseCollider.bounds;

            return new Bounds(transform.position, Vector3.one * BushSyncRadius * 2f);
        }

        private bool IsOverlappingCorpse(Collider candidate)
        {
            if (candidate == null) return false;
            if (corpseCollider == null) return true;

            return Physics.ComputePenetration(
                corpseCollider,
                corpseCollider.transform.position,
                corpseCollider.transform.rotation,
                candidate,
                candidate.transform.position,
                candidate.transform.rotation,
                out _,
                out _);
        }

        private void SyncHiddenVisual()
        {
            if (fadeController is null) return;

            fadeController.SetHidden(IsHidden);
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
            if (transform.parent == null)
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

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, groundLayerMask))
            {
                transform.position = hit.point;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }

            RefreshHidingBushes();
        }
    }
}
