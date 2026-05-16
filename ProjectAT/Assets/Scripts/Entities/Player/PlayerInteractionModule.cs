
using UnityEngine;

public class PlayerInteractionModule : MonoBehaviour
{
    private IInteractable currInteractObject;
    [SerializeField] private EntityStatus myStatus;
    [SerializeField] private Transform holdPoint;
    public Transform HoldPoint { get => holdPoint; set => holdPoint = value; }
    public IInteractable CurrentInteractTarget {get => currInteractObject; set => currInteractObject = value; } //참조횟수가 20임.

    private void Awake()
    {
        myStatus = GetComponent<EntityStatus>();
    }

    private void OnEnable()
    {
        myStatus.onDeath += DropHoldedObject;   
    }

    private void OnDisable()
    {
        myStatus.onDeath -= DropHoldedObject;
    }

    public void DropHoldedObject()
    {
        if(currInteractObject != null)
        {
            if(CurrentInteractTarget is ICarriable carriable)
            {
                carriable.StopCarrying();
            }
            currInteractObject.UnLock();
            currInteractObject = null;
        }
    }
    // public void HandleInteractionRaycast(Vector2 mousePos)
    // {
    //     Ray ray = Camera.main.ScreenPointToRay(mousePos);

    //     Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

    //     if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Interactable")))
    //     {
    //         IUIHoverable interactable = hit.transform.GetComponentInParent<IUIHoverable>();

    //         if(interactable is not null)
    //         {
    //             if (currInteractObject != interactable)
    //             {
    //                 ClearTarget();

    //                 //currInteractObject = interactable;
    //                 interactable.OnHoverEnter();
    //             }
    //         }
    //     }
    //     else
    //     {
    //         ClearTarget();
    //     }
    // }

    // public void HandleRightClick()
    // {
    //     if (currInteractObject is null) return;
    //     //아마 여기서 상태전이 필요할듯
    //     currInteractObject.OnInteract();
    // }

    // private void ClearTarget()
    // {
    //     if (currInteractObject is null) return;

    //     //currInteractObject.OnHoverExit();
    //     currInteractObject = null;
    // }

    #region old code
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

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.TryGetComponent(out IInteractable component))
    //     {
    //         //interactionUI.transform.position = other.transform.position;

    //         //interactionUIRectTransform.position = Camera.main.WorldToScreenPoint(other.transform.position);

    //         //interactionUI?.ShowFkeyImage();
    //         //interactable.OnTriggerEntered();
    //     }
    // }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.TryGetComponent(out IInteractable component) && currInteractable == component)
    //     {
    //         //if (IsInteracting)
    //         //{
    //         //    StopCoroutine(interactionCoroutine);
    //         //    interactionCoroutine = null;
    //         //    interactable?.OnInteractCanceled();
    //         //    MyPlayer.LookRockClear();
    //         //}

    //         //interactable?.OnTriggerExited();
    //         //interactable = null;
    //         //interactionUI?.CloseImage();
    //     }
    // }

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
    #endregion
}
