using System.Collections;
using System.Collections.Generic;
using ProjectAT.Mission;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class MissionTrackerUI : MonoBehaviour
{
    #region Constants Values
    private const string TrackerVisibleClass = "mission-tracker--visible";
    private const string TrackerExitClass = "mission-tracker--exit";
    private const string PanelCompletedClass = "mission-panel--completed";
    private const string ObjectiveCompletedClass = "mission-objective--completed";
    private const string SweepActiveClass = "mission-sweep--active";
    private const string CompleteOverlayVisibleClass = "mission-complete-overlay--visible";
    private const string AllClearVisibleClass = "mission-all-clear--visible";
    private const float SweepDuration = 0.55f;
    #endregion

    [Header("UI Toolkit References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private StyleSheet missionStyleSheet;
    
    [Header("All Clear Icon")]
    [SerializeField] private Sprite allClearIcon;

    [Header("Optional Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip missionCompletedClip;
    [SerializeField] private AudioClip allClearClip;

    [Header("Timing")]
    [SerializeField, Min(0.55f)] private float minimumCompletionHold = 1f;
    [SerializeField, Min(0f)] private float panelSwapDuration = 0.25f;

    private MissionManager missionManager;

    #region UI Toolkit Elements
    private VisualElement tracker;
    private VisualElement panel;
    private VisualElement missionContent;
    private VisualElement objectiveContainer;
    private VisualElement sweep;
    private VisualElement completeOverlay;
    private VisualElement allClearView;
    private VisualElement allClearIconElement;

    private Label missionDescriptionLabel;
    private Label allClearFallbackIcon;
    #endregion

    private readonly Dictionary<MissionObjectiveDefinition, ObjectiveRow> objectiveRows = new Dictionary<MissionObjectiveDefinition, ObjectiveRow>();

    private MissionData displayedMission;
    private MissionData pendingMission;

    private bool pendingAllClear;
    private bool completionRunning;
    private bool styleSheetAdded;
    private bool warnedMissingIcon;

    private Coroutine completionCoroutine;
    private IVisualElementScheduledItem enterSchedule;

    private void Awake()
    {
        CacheVisualElements();
    }
    private void OnEnable()
    {
        if (!CacheVisualElements()) return;

        BindingMissionManager();
    }

    private void OnDisable()
    {
        UnbindMissionManager();
        if(completionCoroutine    != null)
        {
            StopCoroutine(completionCoroutine);
            completionCoroutine = null;
        }

        enterSchedule?.Pause();
        enterSchedule = null;

        completionRunning = false;
        pendingMission = null;
        pendingAllClear = false;
    }

    private bool CacheVisualElements()
    {
        if(uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if(audioSource == null) audioSource = GetComponent<AudioSource>();
        
        if(uiDocument == null)
        {
            Debug.LogError($"{name} : MissionTrackerUI에 UIDocument가 없습니다.", this);
            return false;
        }

        VisualElement root = uiDocument.rootVisualElement;

        if(missionStyleSheet != null && !styleSheetAdded)
        {
            root.styleSheets.Add(missionStyleSheet);
            styleSheetAdded = true;
        }

        tracker = root.Q<VisualElement>("MissionTracker");
        panel = root.Q<VisualElement>("MissionPanel");
        missionContent = root.Q<VisualElement>("MissionContent");
        objectiveContainer = root.Q<VisualElement>("MissionObjectives");
        sweep = root.Q<VisualElement>("MissionSweep");
        completeOverlay = root.Q<VisualElement>("MissionCompleteOverlay");
        allClearView = root.Q<VisualElement>("MissionAllClear");
        allClearIconElement = root.Q<VisualElement>("AllClearIcon");

        missionDescriptionLabel = root.Q<Label>("MissionDescription");
        allClearFallbackIcon = root.Q<Label>("AllClearFallbackIcon");

        bool allElementsFound = 
            tracker != null
            && panel != null
            && missionContent != null
            && objectiveContainer != null
            && sweep != null
            && completeOverlay != null
            && allClearView != null
            && allClearIconElement != null
            && missionDescriptionLabel != null
            && allClearFallbackIcon != null;

        if (!allElementsFound)
        {
            Debug.LogError($"{name}: Mission Tracker UXML 요소를 찾지 못했습니다. UXML name 값들을 확인하세요.", this);
        }

        return allElementsFound;
    }

    private void BindingMissionManager()
    {
        UnbindMissionManager();

        missionManager = InGameManager.Instance.MissionManager;

        if(missionManager == null)
        {
            Debug.LogWarning($"{name} : MissionManager를 찾을 수 없습니다.", this);
            HideTracker();
            return;
        }

        missionManager.MissionActivated += HandleMissionActivated;
        missionManager.ObjectiveProgressed += HandleObjectiveProgressed;
        missionManager.MissionCompleted += HandleMissionCompleted;
        missionManager.AllMissionsCompleted += HandleAllMissionsCompleted;

        SynchronizeWithManager();
    }

    private void UnbindMissionManager()
    {
        if(missionManager == null) return;

        missionManager.MissionActivated -= HandleMissionActivated;
        missionManager.ObjectiveProgressed -= HandleObjectiveProgressed;
        missionManager.MissionCompleted -= HandleMissionCompleted;
        missionManager.AllMissionsCompleted -= HandleAllMissionsCompleted;

        missionManager = null;
    }

    private void SynchronizeWithManager()
    {
        if(missionManager == null)
        {
            HideTracker();
            return;
        }

        if (missionManager.AreAllMissionsCompleted)
        {
            ShowAllClear(false);
            return;
        }

        MissionData currentMission = missionManager.CurrentMission;

        if(currentMission != null)
        {
            ShowMission(currentMission, false);
            return;
        }

        HideTracker();
    }

    private void HandleMissionActivated(MissionData mission)
    {
        if(mission == null) return;
        if (completionRunning)
        {
            pendingMission = mission;
            pendingAllClear = false;
            return;
        }

        ShowMission(mission, true);
    }

    private void HandleObjectiveProgressed(MissionData mission, MissionObjectiveDefinition objective, int currentProgress)
    {
        if(mission == null || mission != displayedMission || objective == null) return;
        if(!objectiveRows.TryGetValue(objective, out ObjectiveRow row)) return;
        UpdateObjectiveRow(row, currentProgress, objective.RequiredAmount);
    }

    private void HandleMissionCompleted(MissionData mission)
    {
        if(mission == null || mission != displayedMission || completionRunning) return;
        pendingMission = null;
        pendingAllClear = false;

        completionCoroutine = StartCoroutine(PlayMissionCompletion());
    }

    private void HandleAllMissionsCompleted()
    {
        pendingMission = null;
        pendingAllClear = true;

        if (!completionRunning)
        {
            ShowAllClear(true);
        }
    }

    private IEnumerator PlayMissionCompletion()
    {
        completionRunning = true;

        panel.AddToClassList(PanelCompletedClass);
        completeOverlay.style.display = DisplayStyle.Flex;
        completeOverlay.RemoveFromClassList(CompleteOverlayVisibleClass);
        
        sweep.style.display = DisplayStyle.Flex;
        sweep.RemoveFromClassList(SweepActiveClass);

        PlayClip(missionCompletedClip);

        yield return null; // 첫 프레임 이후에 클래스를 변경해야 USS transition이 실행됨.

        completeOverlay.AddToClassList(CompleteOverlayVisibleClass);
        sweep.AddToClassList(SweepActiveClass);

        yield return new WaitForSecondsRealtime(SweepDuration);

        sweep.style.display = DisplayStyle.None;
        sweep.RemoveFromClassList(SweepActiveClass);

        float remainingHold = Mathf.Max(0f, minimumCompletionHold - SweepDuration);

        if(remainingHold > 0f)
        {
            yield return new WaitForSecondsRealtime(remainingHold);
        }

        while (!TryResolvePendingState())
        {
            if(missionManager == null)
            {
                completionRunning = false;
                completionCoroutine = null;
                yield break;
            }

            yield return null;
        }

        tracker.RemoveFromClassList(TrackerVisibleClass);
        tracker.AddToClassList(TrackerExitClass);

        if(panelSwapDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(panelSwapDuration);
        }

        bool showAllClear = pendingAllClear;
        MissionData nextMission = pendingMission;

        pendingAllClear = false;
        pendingMission = null;
        completionRunning = false;
        completionCoroutine = null;

        if (showAllClear)
        {
            ShowAllClear(true);
        }
        else if(nextMission != null)
        {
            ShowMission(nextMission, true);
        }
        else
        {
            HideTracker();
        }
    }

    private bool TryResolvePendingState()
    {
        if(pendingAllClear || pendingMission != null) return true;
        if(missionManager == null) return false;
        if (missionManager.AreAllMissionsCompleted)
        {
            pendingAllClear = true;
            return true;
        }

        MissionData currentMission = missionManager.CurrentMission;

        if(currentMission != null && currentMission != displayedMission)
        {
            pendingMission = currentMission;
            return true;
        }

        return false;
    }

    private void ShowMission(MissionData mission, bool animateEntry)
    {
        if(mission == null)
        {
            HideTracker();
            return;
        }

        displayedMission = mission;
        ResetPresentation();
        missionContent.style.display = DisplayStyle.Flex;
        allClearView.style.display = DisplayStyle.None;

        missionDescriptionLabel.text = mission.Description ?? string.Empty;
        missionDescriptionLabel.style.display = string.IsNullOrWhiteSpace(mission.Description) ? DisplayStyle.None : DisplayStyle.Flex;

        objectiveRows.Clear();
        objectiveContainer.Clear();

        IReadOnlyList<MissionObjectiveDefinition> objectives = mission.Objectives;

        foreach(MissionObjectiveDefinition objective in objectives)
        {
            if(objective == null) continue;

            ObjectiveRow row = CreateObjectiveRow(objective);

            int currentProgress = 0;
            int requiredProgress = objective.RequiredAmount;

            missionManager?.TryGetObjectiveProgress(mission, objective.ObjectiveKey, objective.ObjectiveType, out currentProgress, out requiredProgress);
            UpdateObjectiveRow(row, currentProgress, requiredProgress);

            objectiveRows[objective] = row;
            objectiveContainer.Add(row.Root);
        }

        ShowTracker(animateEntry, false);
    }

    private ObjectiveRow CreateObjectiveRow(MissionObjectiveDefinition objective)
    {
        VisualElement root = new VisualElement{ pickingMode = PickingMode.Ignore };
        root.AddToClassList("mission-objective");

        Label marker = new Label("✓") { pickingMode = PickingMode.Ignore };
        marker.AddToClassList("mission-objective__marker");
        string objectiveText = (objective.ObjectiveKey != null) ? objective.ObjectiveKey.DisplayName : objective.ObjectiveType.ToString();
        
        Label text = new Label(objectiveText) { pickingMode = PickingMode.Ignore };
        text.AddToClassList("mission-objective__text");

        Label progress = new Label {pickingMode = PickingMode.Ignore };
        progress.AddToClassList("mission-objective__progress");

        root.Add(marker);
        root.Add(text);
        root.Add(progress);

        return new ObjectiveRow(root, progress);
    }

    private void UpdateObjectiveRow(ObjectiveRow row, int currentProgress, int requiredProgress)
    {
        requiredProgress = Mathf.Max(1, requiredProgress);
        currentProgress = Mathf.Clamp(currentProgress, 0, requiredProgress);

        row.ProgressLabel.text = $"{currentProgress}/{requiredProgress}";

        bool isNowCompleted = currentProgress >= requiredProgress;

        if(isNowCompleted && !row.IsComplete)
        {
            row.IsComplete = true;
            row.Root.AddToClassList(ObjectiveCompletedClass);
        }
    }

    private void ShowAllClear(bool playFeedback)
    {
        displayedMission = null;
        objectiveRows.Clear();
        objectiveContainer.Clear();

        ResetPresentation();

        missionContent.style.display = DisplayStyle.None;
        allClearView.style.display = DisplayStyle.Flex;

        if(allClearIcon != null)
        {
            allClearIconElement.style.display = DisplayStyle.Flex;
            allClearIconElement.style.backgroundImage = new StyleBackground(allClearIcon);
            allClearFallbackIcon.style.display = DisplayStyle.None;
        }
        else
        {
            allClearIconElement.style.display = DisplayStyle.None;
            allClearFallbackIcon.style.display = DisplayStyle.Flex;

            if (!warnedMissingIcon)
            {
                warnedMissingIcon = true;
                Debug.LogWarning($"{name}: All Clear 아이콘이 없어 fallback 체크 아이콘을 사용합니다.", this);
            }
        }

        if (playFeedback)
        {
            PlayClip(allClearClip);
        }

        ShowTracker(true, true);
    }

    private void ResetPresentation()
    {
        enterSchedule?.Pause();
        enterSchedule = null;

        tracker.RemoveFromClassList(TrackerExitClass);
        panel.RemoveFromClassList(PanelCompletedClass);

        completeOverlay.RemoveFromClassList(CompleteOverlayVisibleClass);
        completeOverlay.style.display = DisplayStyle.None;

        sweep.RemoveFromClassList(SweepActiveClass);
        sweep.style.display = DisplayStyle.None;

        allClearView.RemoveFromClassList(AllClearVisibleClass);
    }

    private void ShowTracker(bool animateEntry, bool animateAllClear)
    {
        tracker.style.display = DisplayStyle.Flex;
        tracker.RemoveFromClassList(TrackerExitClass);

        if (!animateEntry)
        {
            tracker.AddToClassList(TrackerVisibleClass);

            if (animateAllClear)
            {
                allClearView.AddToClassList(AllClearVisibleClass);
            }

            return;
        }

        tracker.RemoveFromClassList(TrackerVisibleClass);
        
        //첫 프레임에 바로 최종 클래스를 넣으면 UI Toolkit transition이 생략될 수 있음.
        enterSchedule = tracker.schedule.Execute(() =>
        {
            tracker.AddToClassList(TrackerVisibleClass);
            if (animateAllClear)
            {
                allClearView.AddToClassList(AllClearVisibleClass);
            }
        }).StartingIn(16); // 16ms 후에 실행되도록 예약.(1프레임 이후)
    }

    private void HideTracker()
    {
        if(tracker == null) return;

        displayedMission = null;
        objectiveRows.Clear();

        tracker.RemoveFromClassList(TrackerVisibleClass);
        tracker.RemoveFromClassList(TrackerExitClass);
        tracker.style.display = DisplayStyle.None;
    }

    private void PlayClip(AudioClip clip)
    {
        if(audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    private sealed class ObjectiveRow
    {
        public VisualElement Root { get;}
        public Label ProgressLabel { get; }
        public bool IsComplete { get; set; }

        public ObjectiveRow(VisualElement root, Label progressLabel)
        {
            Root = root;
            ProgressLabel = progressLabel;
        }
    }
}
