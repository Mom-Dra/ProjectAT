using EPOOutline;
using UnityEngine;

public class InteractionUIManager
{
    private Outlinable currOutlinable;
    private EntityStatus currEntityStatus;
    private HealthUI currHealthUI;

    private IUIHoverable currInteractable;
    private LayerMask interactionLayerMask;

    [SerializeField]
    private float interactTime = 3f;

    public InteractionUIManager(LayerMask interactionLayerMask)
    {
        this.interactionLayerMask = interactionLayerMask;
    }

    public void Update()
    {
        HandleInteractionRaycast(Managers.Instance.InputManager.MousePosition);
    }

    private void HandleInteractionRaycast(Vector2 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos); //캐싱 필요. Camera.main은 내부적으로 FindObjectWithTag()를 호출함.

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactionLayerMask))
        {
            Outlinable outlinable = hit.transform.GetComponentInParent<Outlinable>();

            if(outlinable is not null)
            {
                if(currOutlinable != outlinable)
                {
                    ClearOutline();

                    currOutlinable = outlinable;
                    currOutlinable.OutlineParameters.Enabled = true;
                }
            }

            IUIHoverable interactable = hit.transform.GetComponentInParent<IUIHoverable>();

            if (interactable is not null)
            {
                if (currInteractable != interactable)
                {
                    ClearTarget();

                    currInteractable = interactable;
                    interactable.OnHoverEnter();
                }
            }

            EntityStatus entityStatus = hit.transform.GetComponentInParent<EntityStatus>();
            UIAnchor uIAnchor = hit.transform.GetComponentInParent<UIAnchor>();

            if(entityStatus is not null)
            {   
                if(currEntityStatus != entityStatus)
                {
                    ClearEntityStatus();
                    currEntityStatus = entityStatus;

                    currHealthUI = Managers.Instance.UIManager.ShowHealthUI(uIAnchor.TargetAnchor, entityStatus);
                }
            }
        }
        else
        {
            ClearOutline();
            ClearTarget();
            ClearEntityStatus();
        }
    }

    private void ClearOutline()
    {
        if(currOutlinable is not null)
        {
            currOutlinable.OutlineParameters.Enabled = false;
            currOutlinable = null;
        }
    }

    public void HandleRightClick()
    {
        if (currInteractable is null) return;

        //currInteractable.OnInteract();
    }

    private void ClearTarget()
    {
        if (currInteractable is null) return;

        currInteractable.OnHoverExit();
        currInteractable = null;
    }

    private void ClearEntityStatus()
    {
        if (currEntityStatus is null) return;
        else Debug.Log($"current EntityStatus is {currEntityStatus.gameObject.name}");
        Managers.Instance.UIManager.HideHealthUI(currHealthUI);

        currEntityStatus = null;
        currHealthUI = null;
    }
}
