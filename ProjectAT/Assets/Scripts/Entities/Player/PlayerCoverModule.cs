using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PlayerCoverModule : MonoBehaviour
{
    private InputReader inputReader;
    private PlayerAnimator animator;
    private PlayerMovementModule movement;
    private BuffModule buffModule;

    [SerializeField]
    private BuffData buffData;

    private CoverObject currCoverObject;
    private CoverPoint currCoverPoint;
    private CoverPoint reservedCoverPoint;
    private Coroutine moveCoroutine;

    private bool isCover = false;
    public bool IsCover => isCover;

    private void Awake()
    {
        animator = GetComponent<PlayerAnimator>();
        movement = GetComponent<PlayerMovementModule>();
        buffModule = GetComponent<BuffModule>();
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
            //if (currCoverPoint is not null && currCoverPoint != coverPoint) currCoverPoint.HidePulse();

            currCoverPoint = coverPoint;
            //currCoverPoint.ShowPulse();
        }
    }

    public void StartMoveToCover(CoverPoint coverPoint)
    {
        CancelCurrentCoverAction();
        moveCoroutine = StartCoroutine(MoveToCoverCoroutine(coverPoint));
    }

    public void CancelCurrentCoverAction()
    {
        if (moveCoroutine is not null)
        {
            StopCoroutine(moveCoroutine);

            if (reservedCoverPoint is not null)
            {
                reservedCoverPoint.SetMoveTarget(false);
                reservedCoverPoint.HideIndicator();
                //reservedCoverPoint.HidePulse();
            }

            moveCoroutine = null;
        }

        reservedCoverPoint?.Release();
        reservedCoverPoint = null;
        animator.SetCrouch(false);
        isCover = false;
    }

    private IEnumerator MoveToCoverCoroutine(CoverPoint coverPoint)
    {
        reservedCoverPoint = coverPoint;
        coverPoint.SetMoveTarget(true);
        coverPoint.ShowIndicator();
        //coverPoint.ShowPulse();

        reservedCoverPoint.Reserve(gameObject);

        movement.PlayerWalk(coverPoint.transform.position);

        yield return new WaitUntil(() => movement.IsAgentArrived());

        animator.SetCrouch(true);
        isCover = true;
        buffModule.AddBuff(buffData);

        coverPoint.SetMoveTarget(false);
        coverPoint.HideIndicator();
        //coverPoint.HidePulse();

        moveCoroutine = null;
    }

    private void ClearHoverState()
    {
        if (currCoverObject is not null) currCoverObject.HideCoverPoint();
        //if (currCoverPoint is not null) currCoverPoint.HidePulse();

        currCoverObject = null;
        currCoverPoint = null;
    }
}
