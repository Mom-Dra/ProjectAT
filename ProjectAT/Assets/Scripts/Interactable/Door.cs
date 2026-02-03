using System.Collections;
using UnityEngine;
using EPOOutline;

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

        leftDoor.localRotation = endLeftRotation;
        rightDoor.localRotation = endRightRotation;

        runningCoroutine = null;
    }
}
