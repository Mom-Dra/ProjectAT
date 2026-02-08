using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement] // UI Builder 라이브러리에 등록하기 위한 속성
public partial class RadialProgressBar : VisualElement
{
    // 1. 선 두께
    private float m_LineWidth = 10f;
    [UxmlAttribute]
    public float LineWidth
    {
        get => m_LineWidth;
        set
        {
            m_LineWidth = value;
            MarkDirtyRepaint(); // <--- 핵심! "값이 바뀌었으니 다시 그려라"
        }
    }
    // 2. 진행도 (0~100)
    private float m_Progress = 0f;
    [UxmlAttribute]
    public float Progress
    {
        get => m_Progress;
        set
        {
            m_Progress = value;
            MarkDirtyRepaint();
        }
    }

    // 3. 배경색
    private Color m_TrackColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    [UxmlAttribute]
    public Color TrackColor
    {
        get => m_TrackColor;
        set
        {
            m_TrackColor = value;
            MarkDirtyRepaint();
        }
    }

    // 4. 진행색
    private Color m_ProgressColor = Color.red;
    [UxmlAttribute]
    public Color ProgressColor
    {
        get => m_ProgressColor;
        set
        {
            m_ProgressColor = value;
            MarkDirtyRepaint();
        }
    }

    public RadialProgressBar()
    {
        // 화면이 갱신될 때마다 그리기 함수 호출
        generateVisualContent += OnGenerateVisualContent;
    }

    private void OnGenerateVisualContent(MeshGenerationContext context)
    {
        float width = contentRect.width;
        float height = contentRect.height;
        float radius = Mathf.Min(width, height) / 2f - (LineWidth / 2f);

        var painter = context.painter2D;
        painter.lineWidth = LineWidth;
        painter.lineCap = LineCap.Round; // 끝부분 둥글게

        // 중심점 잡기
        Vector2 center = new Vector2(width / 2f, height / 2f);

        // 1. 배경 트랙 그리기 (회색 원)
        painter.strokeColor = TrackColor;
        painter.BeginPath();
        painter.Arc(center, radius, 0, 360);
        painter.Stroke();

        // 2. 진행 상태 그리기 (빨간 원호)
        // 0도가 3시 방향이므로 -90도(12시)부터 시작하게 조정
        float startAngle = -90f;
        float endAngle = startAngle + (360f * (Progress / 100f));

        painter.strokeColor = ProgressColor;
        painter.BeginPath();
        painter.Arc(center, radius, startAngle, endAngle);
        painter.Stroke();
    }

    // 값이 바뀌면 다시 그리라고 알리는 함수 (애니메이션용)
    public void UpdateProgress(float newProgress)
    {
        Progress = newProgress;
        MarkDirtyRepaint(); // "화면 다시 그려!" 명령
    }
}