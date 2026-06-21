using System.Collections;
using UnityEngine;
using EPOOutline;
using Interactable;
using UnityEngine.AI; 

public class Door : InteractableObject
{
    [Header("Door Wings")]
    [SerializeField]
    private Transform leftDoor;

    [SerializeField]
    private Transform rightDoor;

    [Header("Settings")]
    [SerializeField]
    private float openAngle = 90f;

    [SerializeField]
    private float openTime = 1f;
    [SerializeField] private float offsetDistance = 0.3f; 
    [SerializeField] private const float navMeshSearchRadius = 1.0f;

    private Outlinable outlinable;
    private bool isOpen = false;
    private Coroutine runningCoroutine;

    protected override void Awake()
    {
        base.Awake();
        outlinable = GetComponent<Outlinable>();
    }
    public override void OnInteractStart(PlayerController player) { }

    public override void OnExecute(PlayerController player)
    {
        if (runningCoroutine is not null) StopCoroutine(runningCoroutine);

        isOpen = !isOpen;
        runningCoroutine = StartCoroutine(ProcessDoorMotion(isOpen));
    }

    private IEnumerator ProcessDoorMotion(bool targetOpen)
    {
        float targetLeftY = targetOpen ? -openAngle : 0f;
        float targetRightY = targetOpen ? openAngle : 0f;

        Quaternion startLeftRotation = leftDoor.localRotation;
        Quaternion startRightRotation = rightDoor.localRotation;

        Quaternion endLeftRotation = Quaternion.Euler(0, targetLeftY, 0);
        Quaternion endRightRotation = Quaternion.Euler(0, targetRightY, 0);

        float elapsed = 0f;

        while (elapsed < openTime)
        {
            elapsed += Time.deltaTime;

            float ratio = elapsed / openTime;

            leftDoor.localRotation = Quaternion.Slerp(startLeftRotation, endLeftRotation, ratio);
            rightDoor.localRotation = Quaternion.Slerp(startRightRotation, endRightRotation, ratio);

            yield return null;
        }

        leftDoor.localRotation = endLeftRotation;
        rightDoor.localRotation = endRightRotation;

        runningCoroutine = null;
    }
    
    public override bool TryGetInteractLocation(Transform playerTransform, out Vector3 sampledPosition, out Vector3 sampledLookDir, NavMeshAgent agent)
    {
        sampledPosition = Vector3.zero;
        sampledLookDir = Vector3.zero;
        float bestSqrDistance = float.MaxValue;
        Vector3 playerPosXZ = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);

        foreach (Vector3 candidate in interactPositionCandidates)
        {
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSearchRadius, agent.areaMask))
            {
                continue;
            }

            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            Vector3 hitPosXZ = new Vector3(hit.position.x, 0f, hit.position.z);
            float sqrDistance = Vector3.SqrMagnitude(playerPosXZ - hitPosXZ);

            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                sampledPosition = hit.position;
            }
        }

        if (sampledPosition == Vector3.zero)
        {
            return false;
        }

        sampledLookDir = GetInteractLookDir(sampledPosition);
        return true;
    }

    protected override Vector3 GetInteractPosition(Transform playerTransform)
    {
        Vector3 centerPos = GetDoorCenterPos();

        Vector3 dirToPlayer = (playerTransform.position - centerPos).normalized;
        float dot = Vector3.Dot(transform.forward, dirToPlayer);
        Vector3 interactionSide = dot > 0 ? transform.forward : -transform.forward;

        Vector3 calculatedPos = centerPos + (interactionSide * offsetDistance);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(calculatedPos, out hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            return hit.position; 
        }

        Debug.LogWarning($"Door 상호작용 위치({calculatedPos}) 근처에 NavMesh가 없습니다!");
        return calculatedPos;
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
