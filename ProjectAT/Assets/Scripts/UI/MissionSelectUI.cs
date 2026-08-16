using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MissionSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject boxUI;

    [FormerlySerializedAs("stageDatas")]
    [SerializeField] private StageInfo[] stageInfos;

    [SerializeField] private Transform contentTransform;
    [SerializeField] private Image thumbNaail;
    [SerializeField] private Button startButton;

    private StageInfo selectedStageInfo;

    private void Awake()
    {
        startButton.onClick.AddListener(StartButtonClicked);
        InitBoxUI();
    }

    private void InitBoxUI()
    {
        if (stageInfos == null)
        {
            return;
        }

        foreach (StageInfo stageInfo in stageInfos)
        {
            if (stageInfo == null)
            {
                continue;
            }

            GameObject uiObject = Instantiate(boxUI, contentTransform);

            if (uiObject.TryGetComponent(out StageItemUI stageItemUI))
            {
                stageItemUI.Setup(stageInfo, StageItemClicked);
            }
        }
    }

    private void StageItemClicked(StageInfo stageInfo)
    {
        selectedStageInfo = stageInfo;

        thumbNaail.gameObject.SetActive(true);
        thumbNaail.sprite = stageInfo.StageThumbnail;
    }

    private void StartButtonClicked()
    {
        if (selectedStageInfo == null)
        {
            Debug.LogWarning(
                $"{name}: 시작할 StageInfo가 선택되지 않았습니다.",
                this);
            return;
        }

        Managers.Instance.SceneManager.LoadScene(selectedStageInfo.StageNumber);
    }
}