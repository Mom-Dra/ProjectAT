using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject boxUI;
    [SerializeField] private StageData[] stageDatas;
    [SerializeField] private Transform contentTransform;
    [SerializeField] private Image thumbNaail;
    [SerializeField] private Button startButton;

    private StageData selectedStage;


    private void Awake()
    {
        startButton.onClick.AddListener(StartButtonClicked);

        InitBoxUI();
    }

    private void InitBoxUI()
    {
        foreach (StageData stageData in stageDatas)
        {
            GameObject uiObject = Instantiate(boxUI, contentTransform);

            if (uiObject.TryGetComponent(out StageItemUI stageItemUI))
            {
                stageItemUI.Setup(stageData, StageItemClicked);
            }
        }
    }

    private void StageItemClicked(StageData stageData)
    {
        selectedStage = stageData;
        thumbNaail.sprite = stageData.thumbnail;
    }

    private void StartButtonClicked()
    {
        Managers.Instance.SceneManager.LoadScene(selectedStage.stageNumber);
    }
}