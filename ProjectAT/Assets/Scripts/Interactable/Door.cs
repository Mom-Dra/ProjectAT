using System.Collections;
using UnityEngine;
using EPOOutline;
using Interactable;
using UnityEngine.AI;

public class Door : StaticInteractableObject
{
    [Header("[Door]")]
    [Header("References")]
    [SerializeField] private DoorSoundController soundController;

    [Header("Door Wings")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private NavMeshObstacle leftDoorMeshObsctalce;
    [SerializeField] private NavMeshObstacle rightDoorMeshObstacle;

    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openTime = 1f;
    [SerializeField] private float offsetDistance = 0.3f; 
    private bool isOpen = false;
    private Coroutine runningCoroutine;

    protected override void Awake()
    {
        base.Awake();
        if(soundController == null) soundController = GetComponentInChildren<DoorSoundController>();
        if(leftDoorMeshObsctalce == null) 
        {
            leftDoorMeshObsctalce = leftDoor.GetComponent<NavMeshObstacle>();
        }
        if(rightDoorMeshObstacle == null) 
        {
            rightDoorMeshObstacle = rightDoor.GetComponent<NavMeshObstacle>();
        }
    }
    public override void OnInteractStart(PlayerController player) { }

    public override void OnExecute(PlayerController player)
    {
        if (runningCoroutine is not null) 
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
            SetDoorNavMeshObstacleState(true);
        }

        isOpen = !isOpen;
        runningCoroutine = StartCoroutine(ProcessDoorMotion(isOpen));
        soundController.PlayDoorMotionStartSound(isOpen);
    }

    private IEnumerator ProcessDoorMotion(bool targetOpen)
    {
        Vector3 lookDir = CurrentInteractor.transform.forward;
        Vector3 playerToBuilding = transform.position - CurrentInteractor.transform.position;
        float dot = Vector3.Dot(lookDir.normalized, playerToBuilding.normalized);

        float targetY = (dot < 0f ? -1f : 1f) * (targetOpen ? openAngle : 0f);
        
        Quaternion startLeftRotation = leftDoor.localRotation;
        Quaternion startRightRotation = rightDoor.localRotation;

        Quaternion endLeftRotation = Quaternion.Euler(0, -targetY, 0);
        Quaternion endRightRotation = Quaternion.Euler(0, targetY, 0);

        float elapsed = 0f;
        
        SetDoorNavMeshObstacleState(false);

        while (elapsed < openTime)
        {
            elapsed += Time.deltaTime;

            float ratio = elapsed / openTime;

            leftDoor.localRotation = Quaternion.Slerp(startLeftRotation, endLeftRotation, ratio);
            rightDoor.localRotation = Quaternion.Slerp(startRightRotation, endRightRotation, ratio);

            yield return null;
        }
        
        SetDoorNavMeshObstacleState(true);

        leftDoor.localRotation = endLeftRotation;
        rightDoor.localRotation = endRightRotation;
        runningCoroutine = null;
    }

    private void SetDoorNavMeshObstacleState(bool enabled)
    {
        if (leftDoorMeshObsctalce != null) leftDoorMeshObsctalce.enabled = enabled;
        if (rightDoorMeshObstacle != null) rightDoorMeshObstacle.enabled = enabled;
    }

    protected override void InitiateInteractPositions()
    {
        Vector3 centerPos = GetDoorCenterPos();

        Vector3 frontPos = centerPos + (transform.forward * offsetDistance);
        Vector3 backPos = centerPos + (-transform.forward * offsetDistance);

        interactPositionCandidates = new Vector3[] { frontPos, backPos };
    }

    private Vector3 GetDoorCenterPos()
    {
        if (leftDoor == null || rightDoor == null)  return transform.position;
        else return (leftDoor.position + rightDoor.position) * 0.5f;
    }

    protected override Vector3 GetInteractLookDir(Vector3 sampledPosition)
    {
        Vector3 centerPos = GetDoorCenterPos();
        Vector3 lookDir = centerPos - sampledPosition;
        lookDir.y = 0f;

        return lookDir.sqrMagnitude > 0.0001f ? lookDir.normalized : Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (leftDoor == null || rightDoor == null) return;

        Gizmos.color = Color.red;
        
        Vector3 centerPos = GetDoorCenterPos();
        
        // 중앙을 기준으로 앞/뒤 목표 위치
        Vector3 frontPos = centerPos + (transform.forward * offsetDistance);
        Vector3 backPos = centerPos + (-transform.forward * offsetDistance);

        Gizmos.DrawWireSphere(frontPos, 0.2f);
        Gizmos.DrawWireSphere(backPos, 0.2f);
    }

    public override void OnHoverEnter()
    {
        Managers.Instance.CursorManager.SetImage(CursorType.Door);

        if (!isSelected)
        {
            outlinable.OutlineParameters.Enabled = true;
        }
    }

    public override void OnHoverExit()
    {
        if (!isSelected)
        {
            outlinable.OutlineParameters.Enabled = false;
        }
    }

    public override void OnTargeted()
    {
        isSelected = true;
        outlinable.OutlineParameters.Enabled = true;
    }

    public override void OnUntargeted()
    {
        isSelected = false;
        outlinable.OutlineParameters.Enabled = false;
    }
}
