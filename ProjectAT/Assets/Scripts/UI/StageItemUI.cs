using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageItemUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;

    private StageData stageData;
    private Action<StageData> onClicked;

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Setup(StageData stageData, Action<StageData> stageClickedCallback)
    {
        this.stageData = stageData;
        onClicked = stageClickedCallback;

        text.text = stageData.stageName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ButtonClicked);
    }

    private void ButtonClicked()
    {
        onClicked?.Invoke(stageData);
    }
}
