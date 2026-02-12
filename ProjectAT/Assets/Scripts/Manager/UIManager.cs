using UnityEngine;
using UnityEngine.Pool;

public class UIManager
{
    private GameObject healthUIPrefab;

    public UIManager(GameObject healthUIPrefab)
    {
        this.healthUIPrefab = healthUIPrefab;
    }

    public void ShowHealthUI(EntityStatus entityStatus)
    {
        GameObject healthUIObject = Managers.Instance.PoolManager.GetObject(healthUIPrefab);
        HealthUI healthUI = healthUIObject.GetComponentInChildren<HealthUI>();
        healthUI.Bind(entityStatus.transform, entityStatus);
    }

    public void HideHealthUI(HealthUI healthUI)
    {
        healthUI.UnBind();
        Managers.Instance.PoolManager.ReturnObject(healthUI.transform.parent.gameObject, healthUIPrefab);
    }
}
