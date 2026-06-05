using UnityEngine;
using System;

public class EffectModule : MonoBehaviour
{
    [Header("Indicator Prefabs")]
    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private GameObject groundSkillIndicatorPrefab;
    [SerializeField] private Sprite targettingSkillCursor; //TargettingSkill의 커서 스킨. EffectModule이 해당 스킬의 IndicatorType을 보고 커서 스킨을 바꿔주는 방식으로 처리.

    [Header("Indicator References")]
    [SerializeField] private IndicatorBase[] indicators;
    [SerializeField] private LineRenderer lineRenderer;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
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

        indicators[(int)IndicatorType.SkillAoEIndicator] = Instantiate(groundSkillIndicatorPrefab).GetComponent<IndicatorBase>();
        indicators[(int)IndicatorType.SkillAoEIndicator].Hide();
    }

    private void Update()
    {
        
    }

    public void ShowIndicator(Vector3 dest, IndicatorType type, float radius)
    {
        if(type == IndicatorType.TargettingSkillIndicator)
        {
            ShowAimingCursor();
            return;
        }
        indicators[(int)type].transform.position = dest;
        indicators[(int)type].Show(radius * 2);
    }

    private void ShowAimingCursor()
    {
        Cursor.SetCursor(targettingSkillCursor.texture, new Vector2(targettingSkillCursor.texture.width / 2, targettingSkillCursor.texture.height / 2), CursorMode.Auto);
    }

    public void HideIndicator(IndicatorType type)
    {
        if(type == IndicatorType.TargettingSkillIndicator)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }
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
