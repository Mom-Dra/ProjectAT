using System;
using UnityEngine;

public class EnemyAlertnessModule : MonoBehaviour
{
    public event Action<Vector3, float> onAlertThresholdReached;

    [Header("Alertness")]
    [SerializeField, Min(0f)] private float alertness = 0f;
    [SerializeField, Min(0f)] private float maxAlertness = 100f;
    [SerializeField, Min(0f)] private float alertnessDecayRate = 5f;
    [SerializeField, Min(0f)] private float squadAlertThreshold = 40f;

    [Header("Stimulus Alertness")]
    [SerializeField, Min(0f)] private float soundStimulusAlertness = 15f;
    [SerializeField, Min(0f)] private float suspiciousEventStimulusAlertness = 20f;
    [SerializeField, Min(0f)] private float briefSightStimulusAlertness = 25f;
    [SerializeField, Min(0f)] private float customStimulusAlertness = 10f;

    [Header("Debug")]
    [SerializeField] private bool debugAlertnessLog;
    [SerializeField] private bool drawAlertnessGizmos = true;

    private Vector3 lastKnownStimulusPosition;
    private bool hasStimulusPosition;

    public float Alertness => alertness;
    public Vector3 LastKnownStimulusPosition => lastKnownStimulusPosition;
    public bool HasStimulusPosition => hasStimulusPosition;
    private float MaxAlertness => Mathf.Max(0f, maxAlertness);

    private void Awake()
    {
        alertness = Mathf.Clamp(alertness, 0f, MaxAlertness);
    }

    private void Update()
    {
        DecayAlertness();
    }

    public void ReportStimulus(Vector3 position, StimulusType stimulusType)
    {
        ReportStimulus(position, GetStimulusAlertness(stimulusType));
    }

    public void ReportStimulus(Vector3 position, float amount)
    {
        if (amount <= 0f)
        {
            if (debugAlertnessLog)
                Debug.Log($"[{name}] Stimulus ignored: amount={amount:0.##}", this);

            return;
        }

        lastKnownStimulusPosition = position;
        hasStimulusPosition = true;

        float previousAlertness = alertness;
        alertness = Mathf.Clamp(alertness + amount, 0f, MaxAlertness);

        if (debugAlertnessLog)
            Debug.Log($"[{name}] Stimulus +{amount:0.##}: {previousAlertness:0.##} -> {alertness:0.##}", this);

        if (alertness >= squadAlertThreshold)
            onAlertThresholdReached?.Invoke(position, alertness);
    }

    private void DecayAlertness()
    {
        if (alertness <= 0f) return;

        alertness = Mathf.Clamp(alertness - alertnessDecayRate * Time.deltaTime, 0f, MaxAlertness);
    }

    private float GetStimulusAlertness(StimulusType stimulusType)
    {
        return stimulusType switch
        {
            StimulusType.Sound => soundStimulusAlertness,
            StimulusType.SuspiciousEvent => suspiciousEventStimulusAlertness,
            StimulusType.BriefSight => briefSightStimulusAlertness,
            _ => customStimulusAlertness,
        };
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawAlertnessGizmos || !hasStimulusPosition) return;

        float ratio = MaxAlertness <= 0f ? 0f : Mathf.Clamp01(alertness / MaxAlertness);
        Gizmos.color = Color.Lerp(Color.green, Color.yellow, ratio);
        Gizmos.DrawWireSphere(lastKnownStimulusPosition, 1.5f);
    }
#endif
}
