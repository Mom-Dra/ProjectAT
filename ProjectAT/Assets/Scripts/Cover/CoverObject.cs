using UnityEngine;

public class CoverObject : MonoBehaviour, IHoverableFeedback
{
    private CoverPoint[] coverPoints;

    private void Awake()
    {
        coverPoints = GetComponentsInChildren<CoverPoint>();
    }

    public void ShowCoverPoint()
    {
        for(int i = 0 ; i < coverPoints.Length; i++)
        {
            if (!coverPoints[i].IsInUse)
            {
                coverPoints[i].ShowIndicator();
            }
        }
    }

    public void HideCoverPoint()
    {
        for(int i = 0 ; i < coverPoints.Length; i++)
        {
            coverPoints[i].HideIndicator();
        }
    }

    public void ShowSelectedCoverPoint(CoverPoint coverPoint)
    {
        for(int i = 0 ; i < coverPoints.Length; i++)
        {
            if (coverPoints[i] == coverPoint)
            {
                coverPoints[i].ShowIndicator();
                return;
            }
        }
    }

    public void OnHoverEnter()
    {
        ShowCoverPoint();
    }

    public void OnHoverExit()
    {
        HideCoverPoint();
    }
}
