using UnityEngine;

public class BuffInstance
{
    public BuffData BuffData;
    public float RemainingTime;
    public float NextTickTime;

    public BuffInstance(BuffData buffData)
    {
        BuffData = buffData;
        RemainingTime = buffData.Duration;
        NextTickTime = buffData.TickInterval;
    }
}
