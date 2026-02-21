using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    private Image healthImage;
    private Transform target;
    private EntityStatus entityStatus;

    private void Awake()
    {
        healthImage = GetComponent<Image>();
    }

    public void Bind(Transform target, EntityStatus entityStatus)
    {
        this.target = target;
        this.entityStatus = entityStatus;

        entityStatus.onHealthChanged += HealthChanged;
        entityStatus.onDeath += TargetDied;
    }

    public void UnBind()
    {
        entityStatus.onHealthChanged -= HealthChanged;
        entityStatus.onDeath -= TargetDied;

        target = null;
        entityStatus = null;
    }

    public void SetHpRatio(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        healthImage.fillAmount = ratio;
    }

    public void SetActive(bool isActive)
    {
        healthImage.enabled = isActive;
    }

    private void Update()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (target is null) return;

        healthImage.transform.position = target.position;
    }

    private void HealthChanged(float ratio)
    {
        healthImage.fillAmount = ratio;
    }

    private void TargetDied()
    {
        Managers.Instance.UIManager.HideHealthUI(this);
    }
}
