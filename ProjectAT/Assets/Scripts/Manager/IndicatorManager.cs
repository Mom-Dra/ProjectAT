using UnityEngine;
using System;

public enum IndicatorType : ushort
{
    MoveIndicator,
    GroundSkillIndicator, //AOE Inicator 로 바꾸는건?
    TargettingSkillIndicator
}

public class IndicatorManager : MonoBehaviour
{
    public static IndicatorManager Instance { get; private set; }

    [Header("Indicator Prefabs")]
    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private GameObject groundSkillIndicatorPrefab;
    [SerializeField] private Sprite targettingSkillCursor;

    [Header("Indicator References")]
    [SerializeField] private IndicatorBase[] indicators;
    [SerializeField] private LineRenderer lineRenderer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    private void Initialize()
    {
        InitializeIndicators();
        
        if (lineRenderer == null)
        {
            GameObject obj = new GameObject("LineRenderer");
            obj.transform.SetParent(transform);
            lineRenderer = obj.AddComponent<LineRenderer>();
            lineRenderer.colorGradient = new Gradient()
            {
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(new Color(249, 77, 77), 0f),
                    new GradientColorKey(new Color(249, 77, 77), 1f)
                },
                alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            };
            lineRenderer.widthCurve = new AnimationCurve(new Keyframe(0f, 0.1f), new Keyframe(1f, 0.1f));
        }
    }

    private void InitializeIndicators()
    {
        indicators = new IndicatorBase[Enum.GetNames(typeof(IndicatorType)).Length];
        
        indicators[(int)IndicatorType.MoveIndicator] = Instantiate(moveIndicatorPrefab).GetComponent<IndicatorBase>();
        indicators[(int)IndicatorType.MoveIndicator].Hide();
        indicators[(int)IndicatorType.GroundSkillIndicator] = Instantiate(groundSkillIndicatorPrefab).GetComponent<IndicatorBase>();
        indicators[(int)IndicatorType.GroundSkillIndicator].Hide();
    }

    public void HideIndicator(IndicatorType type)
    {
        switch (type)
        {
            case IndicatorType.TargettingSkillIndicator:
                ResetCursor();
                break;
            case IndicatorType.GroundSkillIndicator:
                HideAoeIndicator();
                break;
            default:
                break;
        }
    }

    public void ShowMoveIndicator(Vector3 dest, IndicatorType type, float radius = 1f)
    {
        if (type == IndicatorType.TargettingSkillIndicator)
        {
            ShowAimingCursor();
            return;
        }
        indicators[(int)type].transform.position = dest;
        indicators[(int)type].Show(radius * 2);
    }

    public void ShowAimingCursor()
    {
        Cursor.SetCursor(targettingSkillCursor.texture, new Vector2(targettingSkillCursor.texture.width / 2, targettingSkillCursor.texture.height / 2), CursorMode.Auto);
    }

    private void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ShowAreaIndicator(Vector3 dest, float radius)
    {
        indicators[(int)IndicatorType.GroundSkillIndicator].transform.position = dest;
        indicators[(int)IndicatorType.GroundSkillIndicator].Show(radius * 2);
    }

    public void UpdateAoeIndicator(Vector3 fromPos, Vector3 toPos, Vector3 velocity, float PlayerRange)
    {
        indicators[(int)IndicatorType.GroundSkillIndicator].UpdateIndicator(toPos, velocity);
        DrawThrowingLine(fromPos + Vector3.up, toPos, 1f, PlayerRange);
    }
    public void HideAoeIndicator()
    {
        indicators[(int)IndicatorType.GroundSkillIndicator].Hide();
        ClearLine();
    }

    // public void DrawThrowingLine(Vector3 fromPos, Vector3 toPos, float height)
    // {
    //     lineRenderer.enabled = true;
    //     int segmentCount = 20;
    //     lineRenderer.positionCount = segmentCount + 1;

    //     for (int i = 0; i <= segmentCount; i++)
    //     {
    //         float t = (float)i / segmentCount;
    //         // 매니저의 위치(transform.position)가 아니라, 던지는 사람의 위치(fromPos)를 기준으로 계산해야 합니다.
    //         Vector3 point = Vector3.Lerp(fromPos, toPos, t); 
    //         point.y += height * 4 * t * (1 - t); // 포물선 효과
    //         lineRenderer.SetPosition(i, point);
    //     }
    // }
    public void DrawThrowingLine(Vector3 fromPos, Vector3 toPos, float arcHeight, float PlayerRange)
    {
        if(Vector3.SqrMagnitude(toPos - fromPos) > PlayerRange * PlayerRange)
        {
            lineRenderer.enabled = false;
            return;
        }
        // ★ 단 한 줄로 속도와 비행 시간을 모두 알아옵니다!
        if (!PhysicsMathUtility.CalculateTrajectory(fromPos, toPos, arcHeight, out Vector3 initialVelocity, out float totalTime))
        {
            // 타겟이 너무 높아 계산 실패 시 선을 숨김
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        int segmentCount = 20;
        lineRenderer.positionCount = segmentCount + 1;
        
        // 선을 그리기 위한 시간 간격
        float deltaTime = totalTime / segmentCount;

        for (int i = 0; i <= segmentCount; i++)
        {
            float t = i * deltaTime;
            
            // 물리 공식: 현재 위치 = 시작위치 + (초기속도 * 시간) + (0.5 * 중력 * 시간^2)
            Vector3 point = fromPos + (initialVelocity * t) + (0.5f * Physics.gravity * t * t);
            
            lineRenderer.SetPosition(i, point);
        }
    }

    public void ClearLine()
    {
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
        //Debug
    }
}