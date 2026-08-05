using UnityEngine;
using ProjectAT.Mission;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField] private MissionTrackerUI missionTrackerUI;
    [SerializeField] private Canvas fieldOverlayCanvas;

    public Canvas FieldOverlayCanvas => fieldOverlayCanvas;

    private void Awake()
    {
        if (fieldOverlayCanvas == null)
        {
            GameObject canvasObject = GameObject.Find("FieldOverlayCanvas");

            if (canvasObject != null)
            {
                fieldOverlayCanvas = canvasObject.GetComponent<Canvas>();
            }
        }

        if (missionTrackerUI == null)
        {
            missionTrackerUI = FindAnyObjectByType<MissionTrackerUI>();
        }
    }
}