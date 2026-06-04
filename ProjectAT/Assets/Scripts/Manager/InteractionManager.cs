
using EPOOutline;
using Interactable;
using UnityEngine;

public class InteractionUIManager
{
    private Outlinable currOutlinable;
    private EntityStatus currEntityStatus;
    private HealthUI currHealthUI;
    private Camera mainCamera;

    private IHoverableFeedback currHoverTarget;
    private ITargetableFeedback currSelectedTarget;

    private LayerMask interactionLayerMask;

    public InteractionUIManager(LayerMask interactionLayerMask)
    {
        this.interactionLayerMask = interactionLayerMask;
    }
    public void Start()
    {
        mainCamera = Camera.main;
    }

    public void Update()
    {
        HandleInteractionRaycast(Managers.Instance.InputManager.MousePosition);
    }

    private void HandleInteractionRaycast(Vector2 mousePos)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactionLayerMask))
        {
            UpdateHoverTarget(hit);
            //UpdateHealthUI(hit);
        }
        else
        {
            ClearHoverTarget();
            //ClearEntityStatus();
        }
    }

    private void UpdateHoverTarget(RaycastHit hit)
    {
        IHoverableFeedback nextTarget = hit.transform.GetComponentInParent<IHoverableFeedback>();

        if (currHoverTarget == nextTarget) return;

        ClearHoverTarget();

        currHoverTarget = nextTarget;

        if (currHoverTarget != null) currHoverTarget.OnHoverEnter();
    }

    private void ClearHoverTarget()
    {
        if (currHoverTarget == null) return;
        currHoverTarget.OnHoverExit();
        currHoverTarget = null;
    }

    public void SelectInteractableTarget(InteractableObject target)
    {
        if(target.TryGetComponent(out ITargetableFeedback selectable))
        {
            SelectTarget(selectable);
        }
    }

    public void ClearSelectedTarget()
    {
        if (currSelectedTarget == null)
            return;

        currSelectedTarget.OnUntargeted();
        currSelectedTarget = null;
    }

    private void SelectTarget(ITargetableFeedback target)
    {
        if (currSelectedTarget == target) return;
        if (currSelectedTarget != null) currSelectedTarget.OnUntargeted();

        currSelectedTarget = target;

        if (currSelectedTarget != null) currSelectedTarget.OnTargeted();
    }

    // //TODO : HP관련 UI 호출은 나중에 구현.
    // private void ClearEntityStatus()
    // {
    //     if (currEntityStatus is null) return;
    //     else Debug.Log($"current EntityStatus is {currEntityStatus.gameObject.name}");
    //     Managers.Instance.UIManager.HideHealthUI(currHealthUI);

    //     currEntityStatus = null;
    //     currHealthUI = null;
    // }

    // //oldCodes
    // private void HandleInteractionRaycast(Vector2 mousePos)
    // {
    //     Ray ray = mainCamera.ScreenPointToRay(mousePos);

    //     Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

    //     if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactionLayerMask))
    //     {
    //         Outlinable outlinable = hit.transform.GetComponentInParent<Outlinable>();

    //         if(outlinable is not null)
    //         {
    //             if(currOutlinable != outlinable)
    //             {
    //                 ClearOutline();

    //                 currOutlinable = outlinable;
    //                 currOutlinable.OutlineParameters.Enabled = true;
    //             }
    //         }

    //         IInteractionFeedback interactable = hit.transform.GetComponentInParent<IInteractionFeedback>();

    //         if (interactable is not null)
    //         {
    //             if (currInteractable != interactable)
    //             {
    //                 ClearTarget();

    //                 currInteractable = interactable;
    //                 interactable.OnHoverEnter();
    //             }
    //         }

    //         EntityStatus entityStatus = hit.transform.GetComponentInParent<EntityStatus>();
    //         UIAnchor uIAnchor = hit.transform.GetComponentInParent<UIAnchor>();

    //         if(entityStatus is not null && uIAnchor is not null)
    //         {   
    //             if(currEntityStatus != entityStatus)
    //             {
    //                 ClearEntityStatus();
    //                 currEntityStatus = entityStatus;

    //                 currHealthUI = Managers.Instance.UIManager.ShowHealthUI(uIAnchor.TargetAnchor, entityStatus);
    //             }
    //         }
    //     }
    //     else
    //     {
    //         ClearOutline();
    //         ClearTarget();
    //         ClearEntityStatus();
    //     }
    // }

    // private void ClearOutline()
    // {
    //     if(currOutlinable is not null)
    //     {
    //         currOutlinable.OutlineParameters.Enabled = false;
    //         currOutlinable = null;
    //     }
    // }

    // public void HandleRightClick()
    // {
    //     if (currInteractable is null) return;

    //     //currInteractable.OnInteract();
    // }

    // private void ClearTarget()
    // {
    //     if (currInteractable is null) return;

    //     currInteractable.OnHoverExit();
    //     currInteractable = null;
    // }

}
