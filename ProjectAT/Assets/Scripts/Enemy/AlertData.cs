using UnityEngine;

[CreateAssetMenu(fileName = "AlertData", menuName = "Scriptable Objects/AlertData")]
public class AlertData : ScriptableObject
{
    [Header("Thresholds")]
    [SerializeField, Range(0, 100)]
    private int suspiciousThreshold = 10;
    [SerializeField, Range(0, 100)]
    private int alertThreshold = 30;
    [SerializeField, Range(0, 100)]
    private int combatThreshold = 80;

    [Header("Rates")]
    [SerializeField]
    private int alertPerSecond = 10;
    [SerializeField]
    private float alertCheckInterval = 0.1f;

    public int SuspiciousThreshold => suspiciousThreshold;
    public int AlertThreshold => alertThreshold;
    public int CombatThreshold => combatThreshold;

    public int AlertPerSecond => alertPerSecond;
    public float AlertCheckInterval => alertCheckInterval;

    public const int MIN = 0;
    public const int MAX = 100;
}
