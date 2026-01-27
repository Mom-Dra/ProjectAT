using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
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

    private bool isOpen = false;
    private Coroutine runningCoroutine;

    // IInteractable 구현
    public void OnHoverEnter()
    {
        // 아까 구현한 아웃라인/커서 로직 (생략 가능하면 생략)
        // 만약 문짝 각각에 아웃라인이 있다면 여기서 둘 다 켜주면 됩니다.
    }

    public void OnHoverExit()
    {
        // 아웃라인 끄기
    }

    public void OnInteract()
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

        // 끝난 후 정확한 각도로 보정
        leftDoor.localRotation = endLeftRotation;
        rightDoor.localRotation = endRightRotation;

        runningCoroutine = null;
    }
}
