using UnityEngine;

public class BuffInstance
{
    public BuffData BuffData { get; }
    public float RemainingTime { get; set; }
    public float NextTickTime { get; set; }
    public float Duration => BuffData != null ? BuffData.Duration : 0f;
    public float TickInterval => BuffData != null ? BuffData.TickInterval : 0f;
    public bool IsPermanent => Duration <= 0f;
    public float RemainingRatio => IsPermanent ? 1f : Mathf.Clamp01(RemainingTime / Duration);
    public Sprite Icon => BuffData != null ? BuffData.Icon : null;
    public string BuffName => BuffData != null ? BuffData.BuffName : string.Empty;

    public BuffInstance(BuffData buffData)
    {
        BuffData = buffData;
        Refresh();
    }

    public void Refresh()
    {
        RemainingTime = Mathf.Max(0f, Duration);
        NextTickTime = Mathf.Max(0f, TickInterval);
    }
}
