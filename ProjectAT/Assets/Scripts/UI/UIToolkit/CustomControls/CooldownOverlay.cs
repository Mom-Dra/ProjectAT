using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class CooldownOverlay : VisualElement
{
    private float m_FillAmount = 0f;
    [UxmlAttribute]
    public float FillAmount
    {
        get => m_FillAmount;
        set
        {
            m_FillAmount = Mathf.Clamp01(value);
            MarkDirtyRepaint();
        }
    }

    private Color m_OverlayColor = new Color(0, 0, 0, 0.7f);
    [UxmlAttribute]
    public Color OverlayColor
    {
        get => m_OverlayColor;
        set
        {
            m_OverlayColor = value;
            MarkDirtyRepaint();
        }
    }

    public CooldownOverlay()
    {
        pickingMode = PickingMode.Ignore; 
        
        // [핵심 1] 삐져나온 거대한 원을 사각형 모양으로 잘라버립니다!
        style.overflow = Overflow.Hidden; 
        
        generateVisualContent += OnGenerateVisualContent;
    }

    private void OnGenerateVisualContent(MeshGenerationContext context)
    {
        if (m_FillAmount <= 0.001f) return;

        float width = contentRect.width;
        float height = contentRect.height;
        var painter = context.painter2D;
        painter.fillColor = OverlayColor;

        // [핵심 2] FillAmount가 1일 때 깨지는 버그 방지
        // 1.0(100%)에 도달하면 원을 그리지 않고, 그냥 꽉 찬 네모를 통째로 그립니다.
        if (m_FillAmount >= 0.999f)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(0, 0));
            painter.LineTo(new Vector2(width, 0));
            painter.LineTo(new Vector2(width, height));
            painter.LineTo(new Vector2(0, height));
            painter.ClosePath();
            painter.Fill();
            return; // 여기서 끝냄
        }

        // [핵심 3] 사각형의 모서리를 완전히 덮을 수 있도록 반지름을 아주 크게(대각선 길이로) 잡습니다.
        float radius = Mathf.Sqrt((width * width) + (height * height)); 
        Vector2 center = new Vector2(width / 2f, height / 2f);

        painter.BeginPath();
        painter.MoveTo(center); 

        float startAngle = -90f;
        float endAngle = startAngle + (360f * m_FillAmount);
        
        painter.Arc(center, radius, startAngle, endAngle, ArcDirection.Clockwise);
        painter.LineTo(center); 
        painter.ClosePath();
        painter.Fill();
    }
}