using EPOOutline;
using UnityEngine;

public class InteractionManager
{
    private Outlinable currOutlinable;

    private IInteractable currInteractable;
    private InteractionUI interactionUI;
    private RectTransform interactionUIRectTransform;
    private Coroutine interactionCoroutine;

    [SerializeField]
    private float interactTime = 3f;
    public float InteractTime { get => interactTime; set => interactTime = value; }

    public bool Hasinteractable => currInteractable != null;

    public bool IsInteracting => interactionCoroutine != null;

    public void Update()
    {
        // 마우스 입력 어디서 받아옴?
        HandleInteractionRaycast(Input.mousePosition);
    }

    private void HandleInteractionRaycast(Vector2 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Interactable")))
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

            IInteractable interactable = hit.transform.GetComponentInParent<IInteractable>();

            if (interactable is not null)
            {
                if (currInteractable != interactable)
                {
                    ClearTarget();

                    currInteractable = interactable;
                    currInteractable.OnHoverEnter();
                }
            }
        }
        else
        {
            ClearTarget();
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

        currInteractable.OnInteract();
    }

    private void ClearTarget()
    {
        if (currInteractable is null) return;

        currInteractable.OnHoverExit();
        currInteractable = null;
    }
}
