using PlayerStateMachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Interactable;

public class CoverPoint : InteractableObject
{
    private DecalProjector decalProjector;
    private CoverPulse coverPulse;
    private bool isPlayerMovingTarget;
    
    //버프 제공용 변수도 필요할듯.(IBuffProvider 인터페이스 구현?)

    protected override void Awake()
    {
        base.Awake();
        decalProjector = GetComponent<DecalProjector>();
        decalProjector.enabled = false;

        coverPulse = GetComponent<CoverPulse>();
    }

    public bool Reserve(GameObject npc)
    {
        if (CurrentInteractor != null) return false;

        CurrentInteractor = npc;

        return true;
    }

    public void Release()
    {
        CurrentInteractor = null;
    }

    public void ShowIndicator()
    {
        if (CurrentInteractor == null)
            decalProjector.enabled = true;
    }

    public void HideIndicator()
    {
        if (isPlayerMovingTarget) return;

        decalProjector.enabled = false;
    }

    private void ShowPulse()
    {
        coverPulse.enabled = true;
    }

    private void HidePulse()
    {
        if (isPlayerMovingTarget) return;

        coverPulse.enabled = false;
    }

    public void SetMoveTarget(bool active)
    {
        isPlayerMovingTarget = active;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = CurrentInteractor != null ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    public override void OnInteractStart(PlayerController player) { }
    public override void OnExecute(PlayerController player)
    {
        HidePulse();
    }

    public override void OnHoverEnter()
    {
        ShowIndicator();
    }

    public override void OnHoverExit()
    {
        HideIndicator();
    }

    public override void OnTargeted()
    {
        isSelected = true;
        ShowPulse();
    }

    public override void OnUntargeted()
    {
        isSelected = false;
        HidePulse();
    }
}
