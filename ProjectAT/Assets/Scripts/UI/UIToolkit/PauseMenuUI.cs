using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
public class PauseMenuUI : MonoBehaviour
{

    private UIDocument uiDocument;
    [SerializeField] private OptionMenuUI optionMenuUI;

    private VisualElement pauseOverlay;
    private VisualElement pauseView;
    private Button resumeButton;
    private Button saveButton;
    private Button loadButton;
    private Button optionsButton;
    private Button titleButton;

    private Managers managers;
    private InputManager inputManager;

    private bool uiReady;
    private bool callbacksRegistered;
    private bool isPauseMenuOpen;
    private bool isSceneTransitioning;

    private float previousTimeScale = 1f;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (optionMenuUI == null) {
            optionMenuUI = GetComponentInChildren<OptionMenuUI>(true);
        }
        uiReady = CacheUIElements();

        if (uiReady)
        {
            SetMenuVisible(false);
            SetPauseViewVisible(true);
            SetButtonsEnabled(true);
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
            Debug.LogWarning("PauseMenuUI: UI elements are not ready. Cannot register callbacks.", this);
            return;
        }

        RegisterButtonCallbacks();

        managers = Managers.Instance;
        if (managers == null || managers.InputManager == null)
        {
            Debug.LogError($"{name}: Managers or InputManager was not found. Check scene initialization order.", this);
            return;
        }

        inputManager = managers.InputManager;
        inputManager.OnPauseInputEvent += TogglePauseMenu;

        SetMenuVisible(false);
        SetPauseViewVisible(true);
        SetButtonsEnabled(true);
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnPauseInputEvent -= TogglePauseMenu;
        }

        UnregisterButtonCallbacks();
        ReleasePauseState();

        inputManager = null;
        managers = null;
        isSceneTransitioning = false;
    }

    private bool CacheUIElements()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (optionMenuUI == null)
        {
            optionMenuUI = GetComponentInChildren<OptionMenuUI>(true);
        }

        if (uiDocument == null)
        {
            Debug.LogError($"{name}: UIDocument is missing.", this);
            return false;
        }

        VisualElement root = uiDocument.rootVisualElement;
        pauseOverlay = root.Q<VisualElement>("PauseOverlay");
        pauseView = root.Q<VisualElement>("PauseView");
        resumeButton = root.Q<Button>("ResumeButton");
        saveButton = root.Q<Button>("SaveButton");
        loadButton = root.Q<Button>("LoadButton");
        optionsButton = root.Q<Button>("OptionsButton");
        titleButton = root.Q<Button>("TitleButton");

        bool foundAll = pauseOverlay != null && pauseView != null
            && resumeButton != null && saveButton != null && loadButton != null
            && optionsButton != null && titleButton != null && optionMenuUI != null;

        if (!foundAll)
        {
            Debug.LogError($"{name}: Required PauseMenu UI elements or OptionMenuUI were not found.", this);
        }

        return foundAll;
    }

    private void RegisterButtonCallbacks()
    {
        if (callbacksRegistered)
        {
            return;
        }

        resumeButton.clicked += ClosePauseMenu;
        saveButton.clicked += LogSaveButton;
        loadButton.clicked += LogLoadButton;
        optionsButton.clicked += OpenOptions;
        titleButton.clicked += GoToTitleScene;
        optionMenuUI.SaveCompleted += ReturnToPauseView;
        optionMenuUI.CancelCompleted += ReturnToPauseView;

        callbacksRegistered = true;
    }

    private void UnregisterButtonCallbacks()
    {
        if (!callbacksRegistered)
        {
            return;
        }

        resumeButton.clicked -= ClosePauseMenu;
        saveButton.clicked -= LogSaveButton;
        loadButton.clicked -= LogLoadButton;
        optionsButton.clicked -= OpenOptions;
        titleButton.clicked -= GoToTitleScene;
        optionMenuUI.SaveCompleted -= ReturnToPauseView;
        optionMenuUI.CancelCompleted -= ReturnToPauseView;

        callbacksRegistered = false;
    }

    private void SetButtonsEnabled(bool enabled)
    {
        resumeButton?.SetEnabled(enabled);
        saveButton?.SetEnabled(enabled);
        loadButton?.SetEnabled(enabled);
        optionsButton?.SetEnabled(enabled);
        titleButton?.SetEnabled(enabled);
    }

    private void SetMenuVisible(bool visible)
    {
        if (pauseOverlay == null)
        {
            return;
        }

        pauseOverlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void SetPauseViewVisible(bool visible)
    {
        if (pauseView == null)
        {
            return;
        }

        pauseView.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void TogglePauseMenu()
    {
        if (isSceneTransitioning)
        {
            return;
        }

        if (isPauseMenuOpen && optionMenuUI != null && optionMenuUI.IsOpen) //이미 열려있을 때
        {
            if (optionMenuUI.IsCapturingKey)
            {
                optionMenuUI.CancelKeyCapture();
            }
            else
            {
                optionMenuUI.Cancel();
            }

            return;
        }
        else
        {
            if (isPauseMenuOpen)
            {
                ClosePauseMenu();
            }
            else
            {
                OpenPauseMenu();            
            }
        }
    }

    private void OpenPauseMenu()
    {
        if (isPauseMenuOpen || inputManager == null)
        {
            return;
        }

        previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        isPauseMenuOpen = true;
        inputManager.SetGameplayInputEnabled(false);

        Time.timeScale = 0f;
        optionMenuUI.CloseWithoutSaving();
        SetPauseViewVisible(true);
        SetMenuVisible(true);
        SetButtonsEnabled(true);
        resumeButton.Focus();
    }

    private void ClosePauseMenu()
    {
        if (!isPauseMenuOpen || isSceneTransitioning)
        {
            return;
        }

        ReleasePauseState();
    }

    private void ReleasePauseState()
    {
        if (!isPauseMenuOpen)
        {
            return;
        }

        optionMenuUI?.CloseWithoutSaving();
        SetPauseViewVisible(true);
        
        Time.timeScale = previousTimeScale;

        if (inputManager != null)
        {
            inputManager.SetGameplayInputEnabled(true);
        }

        isPauseMenuOpen = false;
        SetMenuVisible(false);
    }

    private void OpenOptions()
    {
        if (!isPauseMenuOpen || optionMenuUI == null)
        {
            return;
        }

        SetButtonsEnabled(false);
        SetPauseViewVisible(false);
        SetMenuVisible(false);

        optionMenuUI.Open();

        if (!optionMenuUI.IsOpen)
        {
            SetMenuVisible(true);
            SetPauseViewVisible(true);
            SetButtonsEnabled(true);
        }
    }

    private void ReturnToPauseView()
    {
        if (!isPauseMenuOpen)
        {
            return;
        }

        SetMenuVisible(true);
        SetPauseViewVisible(true);
        SetButtonsEnabled(true);

        optionsButton.Focus();
    }

    private void GoToTitleScene()
    {
        if (!isPauseMenuOpen || isSceneTransitioning)
        {
            return;
        }

        if (managers == null || managers.SceneManager == null)
        {
            Debug.LogError($"{name}: SceneManager was not found.", this);
            return;
        }

        isSceneTransitioning = true;
        SetButtonsEnabled(false);
        managers.SceneManager.LoadSceneAsync(SceneType.Start);
    }

    private void LogSaveButton()
    {
        Debug.LogWarning("[PauseMenu] Save UI is not implemented yet.");
    }

    private void LogLoadButton()
    {
        Debug.LogWarning("[PauseMenu] Load UI is not implemented yet.");
    }
}
