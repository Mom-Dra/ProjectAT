
using Interactable;
using UnityEngine;

public class PlayerInteractionModule : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovementModule myMovementModule;

    [Header("Settings")]
    [SerializeField] private float interactLocationCheckInterval = 0.2f;
    private float currentInteractLocationCheckTime = 0f;

    private InteractableObject currInteractObject;
    private Vector3 currentInteractPosition;
    private Vector3 currentInteractLookDir;

    [SerializeField] private Transform holdPoint;
    public Transform HoldPoint { get => holdPoint; set => holdPoint = value; }
    public InteractableObject CurrentInteractTarget { get => currInteractObject; set => currInteractObject = value; }
    public Vector3 CurrentInteractPosition => currentInteractPosition;
    public Vector3 CurrentInteractLookDir => currentInteractLookDir;

    private void Awake()
    {
        myMovementModule = GetComponent<PlayerMovementModule>();
        currentInteractLocationCheckTime = Time.time;
    }

    private void OnEnable()
    {
        GetComponent<EntityStatus>().onDeath += DropHoldedObject;
    }

    private void OnDisable()
    {
        GetComponent<EntityStatus>().onDeath -= DropHoldedObject;
    }

    public void DropHoldedObject()
    {
        if (currInteractObject != null)
        {
            if (CurrentInteractTarget is ICarriable carriable)
            {
                carriable.StopCarrying();
            }
            currInteractObject.UnLock();
            currInteractObject = null;
        }
    }

    public void ClearInteractTarget(bool withUnLock = false)
    {
        if (currInteractObject == null) return;

        if (withUnLock && currInteractObject.CurrentInteractor == gameObject)
        {
            currInteractObject.UnLock();
        }

        UnSelectInteractTarget();
        currInteractObject = null;
        currentInteractPosition = Vector3.zero;
        currentInteractLookDir = Vector3.zero;
    }

    public void SelectInteractTarget(InteractableObject target)
    {
        InGameManager.Instance.InteractionManager.SelectInteractableTarget(target);
    }

    public void UnSelectInteractTarget()
    {
        InGameManager.Instance.InteractionManager.ClearSelectedTarget();
    }

    public bool TrySetInteractTarget(RaycastHit castedObject)
    {
        InteractableObject interactable = castedObject.collider.GetComponentInParent<InteractableObject>();
        if (!(interactable != null && !interactable.IsInUse))
        {
            return false;
        }

        if (TryUpdateInteractLocation(interactable))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SetInteractTarget(InteractableObject target)
    {
        if (currInteractObject == target) return;

        ClearInteractTarget(false);
        currInteractObject = target;

        if (currInteractObject != null) SelectInteractTarget(currInteractObject);
    }

    private void SetInteractLocation(Vector3 position, Vector3 lookDir)
    {
        currentInteractPosition = position;
        currentInteractLookDir = lookDir;
    }

    /// <summary>
    /// 현재 상호작용 대상이 유효하거나 도달 가능한 위치에 있는지 검사. 유효하지만 도달 불가능한 경우, NavMesh에서 샘플링된 위치로 상호작용 위치를 업데이트 시도. 그래도 불가능하면 false 반환.
    /// </summary>
    /// <returns></returns>
    public bool CheckCurrentInteractTargetReachable()
    {
        if (currInteractObject == null) return false;
        if (Time.time - currentInteractLocationCheckTime < interactLocationCheckInterval) return true;
        currentInteractLocationCheckTime = Time.time;

        if (myMovementModule.CanReachPosition(currentInteractPosition))
        {
            return true;
        }

        return TryUpdateInteractLocation(currInteractObject);
    }

    public bool CheckCurrentInteractObjectAvailable()
    {
        return currInteractObject != null
        && (!(currInteractObject.IsInUse && currInteractObject.CurrentInteractor != gameObject));
    }

    private bool TryUpdateInteractLocation(InteractableObject interactable)
    {
        if (interactable.TryGetInteractLocation(transform, out Vector3 sampledPosition, out Vector3 sampledLookDir, myMovementModule.Agent))
        {
            SetInteractTarget(interactable);
            SetInteractLocation(sampledPosition, sampledLookDir);
            return true;
        }
        return false;
    }
}
