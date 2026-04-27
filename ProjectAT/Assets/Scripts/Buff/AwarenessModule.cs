using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(PerceptionSystem))]
public class AwarenessModule : MonoBehaviour
{
    public event Action<IPerceivable> onScanStarted;
    public event Action<IPerceivable> onTargetConfirmed;
    public event Action<IPerceivable> onTargetLost;
    public event Action<IPerceivable> onScanCanceled;

    [SerializeField] private float scanTime = 3f;
    [SerializeField] private float decayTime = 2f;

    private PerceptionSystem perceptionSystem;
    private Enemy enemy;

    private IPerceivable scanningTarget;
    private IPerceivable confirmedTarget;
    private float alertLevel;
    private Coroutine activeCoroutine;

    public float NormalizedAlert => alertLevel;
    public IPerceivable ConfirmedTarget => confirmedTarget;

    private void Awake()
    {
        perceptionSystem = GetComponent<PerceptionSystem>();
        enemy = GetComponent<Enemy>();

        Assert.IsNotNull(perceptionSystem, $"{name} is null");
        Assert.IsNotNull(enemy, $"{enemy} is null");
    }

    private void OnEnable()
    {
        perceptionSystem.onTargetDetected += TargetDetected;
        perceptionSystem.onTargetLost += TargetLost;
    }

    private void OnDisable()
    {
        perceptionSystem.onTargetDetected -= TargetDetected;
        perceptionSystem.onTargetLost -= TargetLost;

        StopActiveCoroutine();
    }

    private void TargetDetected(IPerceivable target)
    {
        if (enemy.IsEngaging)
        {
            Confirm(target);
            return;
        }

        if (ReferenceEquals(scanningTarget, target)) return;
        if (ReferenceEquals(confirmedTarget, target)) return;

        StopActiveCoroutine();
        scanningTarget = target;
        onScanStarted?.Invoke(target);
        Debug.Log("onScanStarted");
        activeCoroutine = StartCoroutine(ScanCoroutine(target));
    }

    private void TargetLost(IPerceivable target)
    {
        if (ReferenceEquals(confirmedTarget, target))
        {
            confirmedTarget = null;
            Debug.Log("onTargetLost");
            onTargetLost?.Invoke(target);
            StopActiveCoroutine();
            activeCoroutine = StartCoroutine(DecayCoroutine());
            return;
        }

        if (ReferenceEquals(scanningTarget, target))
        {
            scanningTarget = null;
            Debug.Log("onScanCanceled");
            onScanCanceled?.Invoke(target);
            StopActiveCoroutine();
            activeCoroutine = StartCoroutine(DecayCoroutine());
        }
    }

    private void Confirm(IPerceivable target)
    {
        alertLevel = 1f;
        scanningTarget = null;
        confirmedTarget = target;
        activeCoroutine = null;
        Debug.Log("onTargetConfirmed");
        onTargetConfirmed?.Invoke(target);
    }

    private IEnumerator ScanCoroutine(IPerceivable target)
    {
        float startAlert = alertLevel;
        float duration = scanTime * (1f - startAlert);

        if (duration <= 0f)
        {
            Confirm(target);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (!target.IsValidTarget)
            {
                scanningTarget = null;
                onScanCanceled?.Invoke(target);
                Debug.Log("onScanCanceled");
                activeCoroutine = StartCoroutine(DecayCoroutine());
                yield break;
            }

            alertLevel = Mathf.Lerp(startAlert, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Confirm(target);
    }

    private IEnumerator DecayCoroutine()
    {
        float startAlert = alertLevel;
        float duration = decayTime * startAlert;

        if (duration <= 0f)
        {
            alertLevel = 0f;
            activeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            alertLevel = Mathf.Lerp(startAlert, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        alertLevel = 0f;
        activeCoroutine = null;
    }

    private void StopActiveCoroutine()
    {
        if (activeCoroutine is not null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }
}
