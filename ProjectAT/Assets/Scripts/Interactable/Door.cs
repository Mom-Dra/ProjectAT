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

    private void Awake()
    {
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

    public override Vector3 GetInteractPosition(Transform playerTransform)
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

    public override Vector3 GetInteractLookDir(Transform playerTransform)
    {
        Vector3 centerPos = GetDoorCenterPos();
        Vector3 lookDir = (centerPos - playerTransform.position);

        lookDir.y = 0;

        return lookDir.normalized;
    }

    private Vector3 GetDoorCenterPos()
    {
        if (leftDoor == null || rightDoor == null) 
            return transform.position;

        return (leftDoor.position + rightDoor.position) * 0.5f;
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

    public override void OnSelected()
    {
        isSelected = true;
        outlinable.OutlineParameters.Enabled = true;
    }

    public override void OnDeselected()
    {
        isSelected = false;
        outlinable.OutlineParameters.Enabled = false;
    }
}
