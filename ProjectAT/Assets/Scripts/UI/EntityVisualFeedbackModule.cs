using UnityEngine;
using EPOOutline;

public class EntityVisualFeedbackModule : MonoBehaviour, IHoverableFeedback, IStatusPresentable
{
    [SerializeField] private Transform hpBarPositionAnchor;
    [SerializeField] private Outlinable outline;
    [SerializeField] private EntityStatus myStatus;

    #region  Property_Getters
    public EntityStatus Target => myStatus;
    public Transform StatusAnchor => hpBarPositionAnchor;
    #endregion

    private void Awake()
    {
        if (outline == null) outline = GetComponent<Outlinable>();
        if (myStatus == null) myStatus = GetComponent<EntityStatus>();
    }

    public void OnHoverEnter()
    {
        outline.OutlineParameters.Enabled = true;
    }

    public void OnHoverExit()
    {
        outline.OutlineParameters.Enabled = false;
    }
}
