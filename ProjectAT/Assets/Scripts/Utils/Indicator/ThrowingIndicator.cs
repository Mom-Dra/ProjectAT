using UnityEngine;
using UnityEngine.Rendering.Universal;
using Indicators;

public class ThrowingIndicator : IndicatorBase, ICircleIndicator, IAttactedIndicator
{
    [SerializeField] private LineRenderer lineRenderer;
    private Transform attachedTarget;
    private DecalProjector decalProjector;
    public float Radius => decalProjector.size.x / 2f;

    public Transform AttachedTarget => attachedTarget;


    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();
        InitiateLineRenderer();
    }

    private void InitiateLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
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

    public void SetRadius(float radius)
    {
        decalProjector.size = new Vector3(radius, radius, decalProjector.size.z);
    }

    public void SetTarget(Transform target)
    {
        attachedTarget = target;
    }

    public override void Show()
    {
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
        attachedTarget = null;
        ClearLine();
    }

    public override void UpdateIndicator(Vector3 position)
    {
        transform.position = position;
    }

    public void DrawThrowingLine(Vector3 toPos, float arcHeight, float PlayerRange)
    {
        if(attachedTarget == null)
        {
            Debug.LogWarning("Attached target is null. Cannot draw throwing line.");
            return;
        }

        if(Vector3.SqrMagnitude(toPos - attachedTarget.position) > PlayerRange * PlayerRange)
        {
            lineRenderer.enabled = false;
            return;
        }
        // ★ 단 한 줄로 속도와 비행 시간을 모두 알아옵니다!
        if (!PhysicsMathUtility.CalculateTrajectory(attachedTarget.position, toPos, arcHeight, out Vector3 initialVelocity, out float totalTime))
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
            Vector3 point = attachedTarget.position + (initialVelocity * t) + (0.5f * Physics.gravity * t * t);
            
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
