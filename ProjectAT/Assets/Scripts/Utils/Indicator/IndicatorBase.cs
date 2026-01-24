using UnityEngine;

public abstract class IndicatorBase : MonoBehaviour
{
    public abstract void Show();
    public abstract void Hide();
    public abstract void UpdateIndicator(Vector3 position, Vector3 velocity);
}
