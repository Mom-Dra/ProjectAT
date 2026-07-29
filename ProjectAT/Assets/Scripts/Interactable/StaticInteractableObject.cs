using Interactable;
using UnityEngine;
using UnityEngine.AI;

namespace Interactable
{
    public abstract class StaticInteractableObject : InteractableObject
    {
        [Header("StaticInteractable Object")]
        [SerializeField] protected float navMeshSearchRadius = 1.0f;
        protected Vector3[] interactPositionCandidates;

        protected override void Awake()
        {
            base.Awake();
            InitiateInteractPositions();
        }

        protected virtual void InitiateInteractPositions()
        {
            interactPositionCandidates = new Vector3[] { transform.position };
        }

        public override bool TryGetInteractLocation(Transform playerTransform, out Vector3 sampledPosition, out Vector3 sampledLookDir, NavMeshAgent agent)
        {
            sampledLookDir = Vector3.zero;
            sampledPosition = GetInteractPosition(playerTransform.position, agent);

            if (sampledPosition == Vector3.zero)
            {
                return false;
            }

            sampledLookDir = GetInteractLookDir(sampledPosition);
            return true;
        }

        protected virtual Vector3 GetInteractPosition(Vector3 interactorPos, NavMeshAgent agent)
        {
            if(agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                return Vector3.zero;
            }
            
            Vector3 interactPosition = Vector3.zero;

            float bestSqrDistance = float.MaxValue;
            Vector3 interactorPosXZ = new Vector3(interactorPos.x, 0f, interactorPos.z);

            foreach (Vector3 candidate in interactPositionCandidates)
            {
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSearchRadius, agent.areaMask))
                {
                    continue;
                }

                NavMeshPath path = new NavMeshPath();

                if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                Vector3 hitPosXZ = new Vector3(hit.position.x, 0f, hit.position.z);
                float sqrDistance = Vector3.SqrMagnitude(interactorPosXZ - hitPosXZ);

                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    interactPosition = hit.position;
                }
            }

            return interactPosition;
        }
    }
}
