using UnityEngine;
using System;

public enum IndicatorType : ushort
{
    MoveIndicator,
    TargettingSkillIndicator,
    GroundSkillIndicator
}

public class EffectModule : MonoBehaviour
{


    [Header("Firing Effect")]
    [SerializeField] private GameObject bulletProjectile;
    [SerializeField] private Transform firingEffectSpawnPoint;

    [Header("Indicator Prefabs")]
    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private GameObject groundSkillIndicatorPrefab;

    [Header("Indicators")]
    [SerializeField] private IndicatorBase[] indicators;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (firingEffectSpawnPoint is null)
            firingEffectSpawnPoint = transform.GetChild(2).transform;

        InitializeIndicators();
    }

    private void InitializeIndicators()
    {
        indicators = new IndicatorBase[Enum.GetNames(typeof(IndicatorType)).Length];
        indicators[(int)IndicatorType.MoveIndicator] = Instantiate(moveIndicatorPrefab).GetComponent<IndicatorBase>();
        indicators[(int)IndicatorType.GroundSkillIndicator] = Instantiate(groundSkillIndicatorPrefab).GetComponent<IndicatorBase>();
    }

    public void PlayFiringEffect(Vector3 dest)
    {
        Bullet bulletComponent 
            = Instantiate(bulletProjectile, 
            firingEffectSpawnPoint.position, 
            firingEffectSpawnPoint.rotation
            ).GetComponent<Bullet>();   //���߿� ������Ʈ Ǯ���� ���̹Ƿ� ������û ������ ���� �������.
        
        //�̺κе� Bullet �Լ� �ȿ�..
        bulletComponent.Initialize(dest, 2f);
        bulletComponent.transform.forward = (dest - firingEffectSpawnPoint.position).normalized;
        bulletComponent.SetVelocity((dest - firingEffectSpawnPoint.position).normalized * 100f);
    }

    public void PlayIndicator(Vector3 dest, IndicatorType type)
    {
        indicators[(int)type].transform.position = dest;
        indicators[(int)type].Show();
    }

    public void UpdateIndicator(Vector3 position, Vector3 velocity, IndicatorType type)
    {
        indicators[(int)type].UpdateIndicator(position, velocity);
    }
    
    // public void PlayMoveIndicatorEffect(Vector3 dest)
    // {
    //     indicators[(int)IndicatorType.MoveIndicator].transform.position = dest;
    //     indicators[(int)IndicatorType.MoveIndicator].Show();
    // }
}
