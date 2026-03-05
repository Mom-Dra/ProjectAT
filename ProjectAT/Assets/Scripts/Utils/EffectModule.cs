using UnityEngine;
using System;
using EPOOutline.Demo;

public enum IndicatorType : ushort
{
    MoveIndicator,
    GroundSkillIndicator,
    TargettingSkillIndicator
}

public class EffectModule : MonoBehaviour
{
    [Header("Firing Effect")]
    [SerializeField] private GameObject bulletProjectile;
    [SerializeField] private Transform firingEffectSpawnPoint;

    [Header("Indicator Prefabs")]
    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private GameObject groundSkillIndicatorPrefab;

    [Header("Indicator Params")]
    [SerializeField] private IndicatorBase[] indicators;
    [SerializeField] private LineRenderer lineRenderer;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (firingEffectSpawnPoint is null)
            firingEffectSpawnPoint = transform.GetChild(2).transform;

        InitializeIndicators();
        if (lineRenderer is null)
        {
            GameObject obj = Instantiate(new GameObject("LineRenderer"));

            obj.transform.parent = transform;
            lineRenderer = obj.AddComponent<LineRenderer>();
        }
    }

    private void InitializeIndicators()
    {
        indicators = new IndicatorBase[Enum.GetNames(typeof(IndicatorType)).Length];
        indicators[(int)IndicatorType.MoveIndicator] = Instantiate(moveIndicatorPrefab).GetComponent<IndicatorBase>();

        indicators[(int)IndicatorType.GroundSkillIndicator] = Instantiate(groundSkillIndicatorPrefab).GetComponent<IndicatorBase>();
        indicators[(int)IndicatorType.GroundSkillIndicator].Hide();
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

    public void ShowIndicator(Vector3 dest, IndicatorType type, float size)
    {
        indicators[(int)type].transform.position = dest;
        indicators[(int)type].Show(size);
    }

    public void HideIndicator(IndicatorType type)
    {
        indicators[(int)type].Hide();
    }

    public void UpdateIndicator(Vector3 position, Vector3 velocity, IndicatorType type)
    {
        indicators[(int)type].UpdateIndicator(position, velocity);
    }

    public void DrawThrowingLine(Vector3 toPos, float height)
    {
        lineRenderer.enabled = true;
        int segmentCount = 20;
        lineRenderer.positionCount = segmentCount + 1;

        for (int i = 0; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 point = Vector3.Lerp(transform.position + Vector3.up, toPos, t);
            point.y += height * 4 * t * (1 - t); // 포물선 효과
            lineRenderer.SetPosition(i, point);
        }
    }

    public void ClearThrowingLine()
    {
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
    }

    // public void PlayMoveIndicatorEffect(Vector3 dest)
    // {
    //     indicators[(int)IndicatorType.MoveIndicator].transform.position = dest;
    //     indicators[(int)IndicatorType.MoveIndicator].Show();
    // }
}
