using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageItemUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;

    private StageInfo stageInfo;
    private Action<StageInfo> onClicked;

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Setup(
        StageInfo targetStageInfo,
        Action<StageInfo> stageClickedCallback)
    {
        stageInfo = targetStageInfo;
        onClicked = stageClickedCallback;

        text.text = stageInfo.StageName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ButtonClicked);
    }

    private void ButtonClicked()
    {
        onClicked?.Invoke(stageInfo);
    }
}