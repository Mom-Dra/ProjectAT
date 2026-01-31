using UnityEngine;

public class CoverObject : MonoBehaviour
{
    private CoverPoint[] coverPoints;

    private void Awake()
    {
        coverPoints = GetComponentsInChildren<CoverPoint>();
    }

    public void ShowCoverPoint()
    {
        foreach(CoverPoint p in coverPoints)
        {
            p.ShowIndicator();
        }
    }

    public void HideCoverPoint()
    {
        foreach(CoverPoint p in coverPoints)
        {
            p.HideIndicator();
        }
    }

    public void ShowSelectedCoverPoint(CoverPoint coverPoint)
    {
        foreach(CoverPoint p in coverPoints)
        {
            if (p == coverPoint)
            {
                p.ShowIndicator();
                return;
            }
        }
    }
}
