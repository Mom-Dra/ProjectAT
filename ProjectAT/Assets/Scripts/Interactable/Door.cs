using System.Collections;
using UnityEngine;
using EPOOutline;
using Interactable;
using UnityEngine.AI; 

public class Door : InteractableObject, IUIHoverable
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

    public void OnHoverEnter()
    {
        Managers.Instance.CursorManager.SetImage(CursorType.Door);
        //outlinable.OutlineParameters.Enabled = true;
    }

    public void OnHoverExit()
    {
        //outlinable.OutlineParameters.Enabled = false;
    }

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
        // 1. 두 문의 정중앙 위치를 가져옵니다.
        Vector3 centerPos = GetDoorCenterPos();

        // 2. 방향 판별 (앞/뒤)
        // 중앙 위치를 기준으로 플레이어가 앞인지 뒤인지 판별합니다.
        Vector3 dirToPlayer = (playerTransform.position - centerPos).normalized;
        float dot = Vector3.Dot(transform.forward, dirToPlayer);
        Vector3 interactionSide = dot > 0 ? transform.forward : -transform.forward;

        // 3. 중앙 위치에서 앞/뒤로 offsetDistance만큼 이동한 좌표 계산
        Vector3 calculatedPos = centerPos + (interactionSide * offsetDistance);

        // 4. NavMesh 보정 로직
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
        // 1. 두 문의 정중앙 위치를 가져옵니다.
        Vector3 centerPos = GetDoorCenterPos();

        // 2. 플레이어의 위치에서 '문의 정중앙'을 향하는 방향 벡터를 계산합니다.
        Vector3 lookDir = (centerPos - playerTransform.position);

        // 3. Y축 차이로 인한 고개 숙임 방지
        lookDir.y = 0;

        return lookDir.normalized;
    }

    private Vector3 GetDoorCenterPos()
    {
        // 만약 둘 중 하나라도 할당되지 않았다면 안전하게 부모의 위치 반환
        if (leftDoor == null || rightDoor == null) 
            return transform.position;

        // 두 위치 벡터를 더하고 2로 나누어 정확한 중간 지점(Midpoint)을 구합니다.
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
}
