using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    private Slider healthImage;
    private Transform targetAnchor;
    [SerializeField] private EntityStatus entityStatus;

    [SerializeField]
    private Vector3 offset;

    private void Awake()
    {
        healthImage = GetComponent<Slider>();
    }

    public void Bind(Transform targetAnchor, EntityStatus entityStatus)
    {
        this.targetAnchor = targetAnchor;
        this.entityStatus = entityStatus;

        entityStatus.onHealthChanged += HealthChanged;
        entityStatus.onDeath += TargetDied;

        SetHpRatio(entityStatus.Ratio);
    }

    public void UnBind()
    {
        if (entityStatus == null) return;
        entityStatus.onHealthChanged -= HealthChanged;
        entityStatus.onDeath -= TargetDied;

        targetAnchor = null;
        entityStatus = null;
    }

    public void SetHpRatio(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        healthImage.value = ratio;
    }

    public void SetActive(bool isActive)
    {
        healthImage.enabled = isActive;
    }

    private void Update()
    {
    }

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (targetAnchor is null) return;

        //healthImage.transform.position = target.position + offset;

        Debug.Log("FollowTarget");
        Debug.Log($"{Camera.main.WorldToScreenPoint(targetAnchor.position)}");
        transform.position = Camera.main.WorldToScreenPoint(targetAnchor.position);
    }

    private void HealthChanged(float ratio)
    {
        Debug.Log($"HealthChanged: {ratio}");

        SetHpRatio(ratio);
    }

    private void TargetDied()
    {
        //Managers.Instance.UIManager.HideHealthUI(this);
    }
}
