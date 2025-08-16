using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField]
    protected GameObject projectilePrefab;

    [SerializeField]
    protected Transform projectileSpawnPoint;

    protected Transform target;
    protected float damage;

    [SerializeField]
    private float maxCooldownTime;
    private float currentCooldownTime;
    private bool isSkillAvailable = true;

    public void Setup(Transform target, float damage, float cooldownTime)
    {
        this.target = target;
        this.damage = damage;
        maxCooldownTime = cooldownTime;
    }

    private void Update()
    {
        if (!isSkillAvailable && Time.time - currentCooldownTime > maxCooldownTime)
            isSkillAvailable = true;
    }

    public void TryAttack()
    {
        if(isSkillAvailable)
        {
            OnAttack();
            isSkillAvailable = false;
            currentCooldownTime = Time.time;
        }
    }

    public abstract void OnAttack();
}
