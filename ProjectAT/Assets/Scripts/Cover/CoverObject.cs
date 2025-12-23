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
            p.Show();
        }
    }

    public void HideCoverPoint()
    {
        foreach(CoverPoint p in coverPoints)
        {
            p.Hide();
        }
    }
}
