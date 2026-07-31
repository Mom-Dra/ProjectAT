using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using EPOOutline;
using Interactable;

public class AutomaticDoor : MonoBehaviour, IInteractSignalReceiver
{
    [Header("References")]
    [SerializeField] private Animator myAnimator;
    [SerializeField] private Outlinable outline;
    [SerializeField] private NavMeshObstacle navMeshObstacle;

    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float outlineTime = 1.0f;
    private Coroutine outlineCoroutine;

    private void Awake()
    {
        if (myAnimator == null)
        {
            myAnimator = GetComponent<Animator>();
        }
        if (outline == null)
        {
            outline = GetComponent<Outlinable>();
        }
        if (navMeshObstacle == null)
        {
            navMeshObstacle = GetComponent<NavMeshObstacle>();
        }
    }

    private void Start()
    {
        StatusReset();
    }

    private void OnDisable()
    {
        StatusReset();
    }

    public void OnInteractSignalReceived()
    {
        if (outlineCoroutine != null)
        {
            StopCoroutine(outlineCoroutine);
        }
        ToggleDoor();
        outlineCoroutine = StartCoroutine(OutlineTimeCoroutine());
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        navMeshObstacle.enabled = !isOpen;
        myAnimator.SetBool("IsOpen", isOpen);
    }


    private IEnumerator OutlineTimeCoroutine()
    {
        float elapsedTime = 0f;
        outline.OutlineParameters.Enabled = true;

        while (elapsedTime < outlineTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        outline.OutlineParameters.Enabled = false;
    }

    private void StatusReset()
    {
        if (outlineCoroutine != null)
        {
            StopCoroutine(outlineCoroutine);
            outlineCoroutine = null;
        }
        outline.OutlineParameters.Enabled = false;
        navMeshObstacle.enabled = true;
        myAnimator.SetBool("IsOpen", false);
    }
}
