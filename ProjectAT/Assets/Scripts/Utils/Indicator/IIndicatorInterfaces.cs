using UnityEngine;


namespace Indicators
{
    public interface IAttactedIndicator
    {
        Transform AttachedTarget { get; }
        void SetTarget(Transform target);
    }
    public interface ICircleIndicator
    {
        float Radius { get; }
        void SetRadius(float radius);
    }

    public interface ISectorIndicator : IAttactedIndicator
    {
        float Width { get; }
        float Angle { get; }
        void SetSize(float width, float angle);
    }
}