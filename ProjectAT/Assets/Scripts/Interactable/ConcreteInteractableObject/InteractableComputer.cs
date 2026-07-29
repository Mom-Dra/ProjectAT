using UnityEngine;
using Interactable;
using UnityEngine.AI;

public sealed class InteractableComputer : StaticInteractableObject
{
    [Header("Interactable Computer Settings")]
    [SerializeField] private Transform interactionPoint;

    private bool isCompleted;
    public bool IsCompleted => isCompleted;

    public override void OnInteractStart(PlayerController player) { }

    protected override void InitiateInteractPositions()
    {
        Vector3 position = interactionPoint != null ? interactionPoint.position : transform.position;
        interactPositionCandidates = new[] { position };
    }

    protected override bool TryExecuteInteraction(PlayerController player)
    {
        if (isCompleted)
        {
            return false;
        }

        isCompleted = true;
        isSelected = false;

        if (outlinable != null)
        {
            outlinable.OutlineParameters.Enabled = false;
        }

        return true;
    }

    public override bool TryGetInteractLocation(Transform playerTransform, out Vector3 sampledPosition, out Vector3 sampledLookDir, NavMeshAgent agent)
    {
        if (isCompleted)
        {
            sampledPosition = Vector3.zero;
            sampledLookDir = Vector3.zero;
            return false;
        }

        return base.TryGetInteractLocation(playerTransform, out sampledPosition, out sampledLookDir, agent);
    }

    public override bool TryLock(PlayerController interactor)
    {
        return !isCompleted && base.TryLock(interactor);
    }

    public override void OnHoverEnter()
    {
        if (isCompleted)
        {
            return;
        }

        if (!isSelected && outlinable != null)
        {
            outlinable.OutlineParameters.Enabled = true;
        }
    }

    public override void OnHoverExit()
    {
        if (!isSelected && outlinable != null)
        {
            outlinable.OutlineParameters.Enabled = false;
        }
    }

    public override void OnTargeted()
    {
        if (isCompleted)
        {
            return;
        }

        isSelected = true;

        if (outlinable != null)
        {
            outlinable.OutlineParameters.Enabled = true;
        }
    }

    public override void OnUntargeted()
    {
        isSelected = false;

        if (outlinable != null)
        {
            outlinable.OutlineParameters.Enabled = false;
        }
    }
}
