using System.Collections;
using UnityEngine;

public class PlayerCoverModule : MonoBehaviour
{
    private InputReader inputReader;
    private PlayerAnimator animator;
    private PlayerMovementModule movement;

    private CoverObject currCoverObject;
    private CoverPoint currCoverPoint;
    private CoverPoint targetCoverPoint;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        animator = GetComponent<PlayerAnimator>();
        movement = GetComponent<PlayerMovementModule>();
    }

    public void HandleCoverRaycast(Vector2 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("CoverPoint")))
        {
            UpdateHoverState(hit);
        }
        else
        {
            ClearHoverState();
        }
    }

    private void UpdateHoverState(RaycastHit hit)
    {
        CoverObject coverObject = hit.transform.GetComponentInParent<CoverObject>();

        if (coverObject is not null && currCoverObject != coverObject)
        {
            if (currCoverObject) currCoverObject.HideCoverPoint();
            currCoverObject = coverObject;
            currCoverObject.ShowCoverPoint();
        }

        if (hit.transform.TryGetComponent(out CoverPoint coverPoint))
        {
            if (currCoverPoint is not null && currCoverPoint != coverPoint)
                currCoverPoint.HidePulse();

            currCoverPoint = coverPoint;
            currCoverPoint.ShowPulse();
        }
    }

    public void StartMoveToCover(CoverPoint coverPoint)
    {
        CancelCurrentCoverAction();
        moveCoroutine = StartCoroutine(MoveToCoverCoroutine(coverPoint));
    }

    public void CancelCurrentCoverAction()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);

            if (targetCoverPoint is not null)
            {
                targetCoverPoint.SetMoveTarget(false);
                targetCoverPoint.HideIndicator();
                targetCoverPoint.HidePulse();
            }

            moveCoroutine = null;
            targetCoverPoint = null;
        }

        animator.SetCrouch(false); // 이동 시작 시 엄폐 애니메이션 해제
    }

    private IEnumerator MoveToCoverCoroutine(CoverPoint coverPoint)
    {
        targetCoverPoint = coverPoint;
        coverPoint.SetMoveTarget(true);
        coverPoint.ShowIndicator();
        coverPoint.ShowPulse();

        movement.PlayerWalk(coverPoint.transform.position);

        yield return new WaitUntil(() => movement.IsAgentArrived());

        animator.SetCrouch(true);

        coverPoint.SetMoveTarget(false);
        coverPoint.HideIndicator();
        coverPoint.HidePulse();

        targetCoverPoint = null;
        moveCoroutine = null;
    }

    private void ClearHoverState()
    {
        if (currCoverObject is not null) currCoverObject.HideCoverPoint();
        if (currCoverPoint is not null) currCoverPoint.HidePulse();

        currCoverObject = null;
        currCoverPoint = null;
    }
}
