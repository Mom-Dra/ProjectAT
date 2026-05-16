public readonly struct DetectedTarget
{
    public readonly IPerceivable Perceivable;
    public readonly float Distance;

    public DetectedTarget(IPerceivable perceivable, float distance)
    {
        Perceivable = perceivable;
        Distance = distance;
    }
}
