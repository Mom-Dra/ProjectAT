using UnityEngine;

public abstract class IndicatorBase : MonoBehaviour
{
    public abstract void Show();
    public abstract void Hide();
    public virtual void UpdateIndicator(Vector3 position) { }
}
