using UnityEngine;

public interface IHoverableFeedback
{
    void OnHoverEnter();
    void OnHoverExit();
}

public interface ISelectableFeedback
{
    void OnSelected();
    void OnDeselected();
}
