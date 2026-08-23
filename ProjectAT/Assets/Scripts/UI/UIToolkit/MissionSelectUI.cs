using System;
using System.Collections.Generic;
using ProjectAT.Mission;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
public class MissionSelectUI : MonoBehaviour
{
    public event Action Closed;

    public bool IsOpen { get; private set; }

    [SerializeField] private StageInfo[] stageInfos;

    private UIDocument uiDocument;
    private VisualElement root;
    private Button returnButton;
    private Button startButton;
    private VisualElement stageDetailContent;
    private Label stageNameLabel;
    private Label stageDescriptionLabel;
    private ScrollView stageListScrollView;
    private ScrollView objectivesScrollView;
    private ScrollView descriptionScrollView;
    private Label objectivesEmptyLabel;
    private Label descriptionEmptyLabel;
    private Image stageThumbnailImage;

    private StageInfo selectedStageInfo;
    private readonly List<Button> stageListButtons = new List<Button>();

    private bool uiReady;
    private bool callbacksRegistered;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        uiReady = CacheUIElements();

        if (uiReady)
        {
            SetScreenVisible(false);
        }
    }

    private void OnEnable()
    {
        if (!uiReady)
        {
            uiReady = CacheUIElements();
        }

        if (!uiReady)
        {
            Debug.LogWarning($"{name}: Mission select UI elements are not ready.", this);

            return;
        }

        RegisterCallbacks();
        SetScreenVisible(false);
    }

    private void OnDisable()
    {
        UnregisterCallbacks();

        IsOpen = false;
    }

    public void Open()
    {
        if (!uiReady || IsOpen)
        {
            return;
        }

        RebuildStageList();
        EnterInitialState();

        SetScreenVisible(true);
        IsOpen = true;
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        SetScreenVisible(false);

        Closed?.Invoke();
    }

    private bool CacheUIElements()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError($"{name}: UIDocument is missing.", this);

            return false;
        }

        root = uiDocument.rootVisualElement;

        if (root == null)
        {
            return false;
        }

        returnButton = root.Q<Button>("ReturnButton");
        stageListScrollView = root.Q<ScrollView>("StageListScrollView");
        stageNameLabel = root.Q<Label>("StageNameLabel");
        objectivesScrollView = root.Q<ScrollView>("ObjectivesScrollView");
        descriptionScrollView = root.Q<ScrollView>("DescriptionScrollView");
        stageDescriptionLabel = root.Q<Label>("StageDescriptionLabel");
        startButton = root.Q<Button>("StartButton");
        stageDetailContent = root.Q<VisualElement>("StageDetailContent");
        objectivesEmptyLabel = root.Q<Label>("ObjectivesEmptyLabel");
        descriptionEmptyLabel = root.Q<Label>("DescriptionEmptyLabel");
        stageThumbnailImage = root.Q<Image>("StageThumbnail");

        (string elementName, VisualElement element)[] requiredElements =
        {
            ("ReturnButton", returnButton),
            ("StageListScrollView", stageListScrollView),
            ("StageNameLabel", stageNameLabel),
            ("ObjectivesScrollView", objectivesScrollView),
            ("DescriptionScrollView", descriptionScrollView),
            ("StageDescriptionLabel", stageDescriptionLabel),
            ("StartButton", startButton),
            ("StageDetailContent", stageDetailContent),
            ("ObjectivesEmptyLabel", objectivesEmptyLabel),
            ("DescriptionEmptyLabel", descriptionEmptyLabel),
            ("StageThumbnail", stageThumbnailImage),
        };

        List<string> missingElementNames = new List<string>();

        foreach ((string elementName, VisualElement element) in requiredElements)
        {
            if (element == null)
            {
                missingElementNames.Add(elementName);
            }
        }

        if (missingElementNames.Count > 0)
        {
            Debug.LogError(
                $"{name}: Required elements were not found in MissionSelect.uxml: {string.Join(", ", missingElementNames)}",
                this);

            return false;
        }

        return true;
    }

    private void RegisterCallbacks()
    {
        if (callbacksRegistered)
        {
            return;
        }

        returnButton.clicked += HandleReturnClicked;
        startButton.clicked += HandleStartClicked;
        callbacksRegistered = true;
    }

    private void UnregisterCallbacks()
    {
        if (!callbacksRegistered)
        {
            return;
        }

        returnButton.clicked -= HandleReturnClicked;
        startButton.clicked -= HandleStartClicked;
        callbacksRegistered = false;
    }

    private void HandleReturnClicked()
    {
        Close();
    }

    private void RebuildStageList()
    {
        stageListButtons.Clear();
        stageListScrollView.contentContainer.Clear();

        if (stageInfos == null || stageInfos.Length == 0)
        {
            AddEmptyListLabel();

            return;
        }

        foreach (StageInfo stageInfo in stageInfos)
        {
            if (stageInfo == null)
            {
                continue;
            }

            Button button = new Button(() => SelectStage(stageInfo)) { text = stageInfo.StageName };
            button.userData = stageInfo;
            button.AddToClassList("stage-button");
            stageListScrollView.contentContainer.Add(button);
            stageListButtons.Add(button);
        }

        if (stageListButtons.Count == 0)
        {
            AddEmptyListLabel();

            return;
        }

        // 마지막 버튼 하단 여백 제거용 마커(USS :last-child 미지원 대체)
        stageListButtons[stageListButtons.Count - 1].AddToClassList("stage-button--last");
    }

    private void AddEmptyListLabel()
    {
        Label emptyLabel = new Label("No Stages");
        emptyLabel.AddToClassList("stage-list-empty-label");
        stageListScrollView.contentContainer.Add(emptyLabel);
    }

    private void EnterInitialState()
    {
        // 요구 2-2, 3: 선택 해제 + 우측 패널 내용 전체 숨김(회색 패널 배경만 남음)
        selectedStageInfo = null;

        foreach (Button button in stageListButtons)
        {
            button.RemoveFromClassList("stage-button--selected");
        }

        stageDetailContent.style.display = DisplayStyle.None;
    }

    private void SelectStage(StageInfo stageInfo)
    {
        selectedStageInfo = stageInfo;

        foreach (Button button in stageListButtons)
        {
            button.EnableInClassList("stage-button--selected", button.userData is StageInfo info && info == stageInfo);
        }

        // 요구 2: 상세 내용 표시
        stageDetailContent.style.display = DisplayStyle.Flex;
        stageNameLabel.text = stageInfo.StageName;
        stageThumbnailImage.sprite = stageInfo.StageThumbnail;

        PopulateObjectives(stageInfo);
        PopulateDescription(stageInfo);
    }

    private void PopulateObjectives(StageInfo stageInfo)
    {
        objectivesScrollView.contentContainer.Clear();

        int addedObjectives = 0;

        IReadOnlyList<MissionData> objectives = stageInfo.StageObjectives;

        for (int i = 0; i < objectives.Count; i++)
        {
            MissionData missionData = objectives[i];

            if (missionData == null)
            {
                continue;
            }

            addedObjectives++;

            Label objectiveRow = new Label($"{addedObjectives}. {missionData.MissionName}");
            objectiveRow.AddToClassList("objective-label");
            objectivesScrollView.contentContainer.Add(objectiveRow);
        }

        bool hasNoObjectives = addedObjectives == 0;
        objectivesScrollView.style.display = hasNoObjectives ? DisplayStyle.None : DisplayStyle.Flex;
        objectivesEmptyLabel.style.display = hasNoObjectives ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void PopulateDescription(StageInfo stageInfo)
    {
        string description = stageInfo.StageDescription;
        bool hasNoDescription = string.IsNullOrWhiteSpace(description);

        descriptionScrollView.style.display = hasNoDescription ? DisplayStyle.None : DisplayStyle.Flex;
        descriptionEmptyLabel.style.display = hasNoDescription ? DisplayStyle.Flex : DisplayStyle.None;

        if (!hasNoDescription)
        {
            stageDescriptionLabel.text = description;
        }
    }

    private void HandleStartClicked()
    {
        if (selectedStageInfo == null)
        {
            Debug.LogWarning($"{name}: 시작할 StageInfo가 선택되지 않았습니다.", this);

            return;
        }

        Managers managers = Managers.Instance;

        if (managers == null || managers.SceneManager == null)
        {
            Debug.LogError($"{name}: SceneManager is not ready.", this);

            return;
        }

        managers.SceneManager.LoadScene(selectedStageInfo.StageNumber);
    }

    private void SetScreenVisible(bool visible)
    {
        if (root == null)
        {
            return;
        }

        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
