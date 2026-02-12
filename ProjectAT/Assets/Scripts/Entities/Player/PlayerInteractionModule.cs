using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

public class PlayerInteractionModule : MonoBehaviour
{
    private IInteractable currInteractable;
    private HealthUI interactionUI;
    private RectTransform interactionUIRectTransform;
    private Coroutine interactionCoroutine;

    [SerializeField]
    private float interactTime = 3f;
    public float InteractTime { get => interactTime; set => interactTime = value; }

    public bool Hasinteractable => currInteractable != null;

    public bool IsInteracting => interactionCoroutine != null;

    //private Player MyPlayer = default;

    private void Awake()
    {
        //interactionUI = FindAnyObjectByType<InteractionUI>();
        //interactionUIRectTransform = interactionUI.GetComponent<RectTransform>();
    }

    public bool InteractionCheck()
    {
        Debug.LogError(Hasinteractable);
        Debug.LogError(currInteractable);

        if (Hasinteractable)
            return true;

        return false;
    }

    public void HandleInteractionRaycast(Vector2 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Interactable")))
        {
            IInteractable interactable = hit.transform.GetComponentInParent<IInteractable>();

            if(interactable is not null)
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

    //public void InteractEnter(Player player)
    //{
    //    MyPlayer = player;
    //    interactionCoroutine = StartCoroutine(InterationCoroutine());
    //    MyPlayer.IsInputRock = true;

    //    interactable?.OnInteractEnter();
    //}

    //public void InteractExit(Player player)
    //{
    //    if (IsInteracting)
    //        StopCoroutine(interactionCoroutine);

    //    MyPlayer = null;
    //    interactable?.OnInteractExit();
    //}

    //public void InteractUpdate(Player player)
    //{

    //}

    //public void CancelInteract(Player player)
    //{
    //    if (IsInteracting)
    //    {
    //        StopCoroutine(interactionCoroutine);
    //        interactionCoroutine = null;

    //        player.IsInputRock = false;

    //        interactionUI?.CloseImage();
    //        interactable?.OnInteractCanceled();

    //        player.Animator.SetBool("isInteraction", false);
    //        player.ChangeState(PlayerState.Idle);
    //    }
    //}

    //public void CloseUI()
    //{
    //    if (interactable != null && interactable is IUIClosable closable)
    //    {
    //        closable.CloseUI();
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable component))
        {
            //interactionUI.transform.position = other.transform.position;

            //interactionUIRectTransform.position = Camera.main.WorldToScreenPoint(other.transform.position);

            //interactionUI?.ShowFkeyImage();
            //interactable.OnTriggerEntered();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable component) && currInteractable == component)
        {
            //if (IsInteracting)
            //{
            //    StopCoroutine(interactionCoroutine);
            //    interactionCoroutine = null;
            //    interactable?.OnInteractCanceled();
            //    MyPlayer.LookRockClear();
            //}

            //interactable?.OnTriggerExited();
            //interactable = null;
            //interactionUI?.CloseImage();
        }
    }

    //private IEnumerator InterationCoroutine()
    //{
    //    float time = 0f;

    //    MyPlayer.IsCancel = false;

    //    interactionUI?.SetRatio(0f);
    //    interactionUI?.ShowProgressImage();

    //    while (time < interactTime)
    //    {
    //        time += Time.deltaTime;

    //        if (MyPlayer.IsCancel)
    //            break;

    //        yield return null;
    //    }

    //    interactionUI?.SetRatio(1f);
    //    interactionUI?.CloseImage();

    //    interactable?.OnInteractCompleted();
    //    interactionCoroutine = null;

    //    if (MyPlayer.IsCancel) MyPlayer.LookRockClear();
    //    else interactable?.OnInteractCompleted();

    //    MyPlayer.Animator.SetBool("isInteraction", false);
    //    interactionCoroutine = null;

    //    MyPlayer.ChangeState(PlayerState.Idle);
    //}
}
