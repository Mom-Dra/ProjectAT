using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
public class PauseMenuUI : MonoBehaviour
{
    private UIDocument uiDocument;

    private VisualElement pauseOverlay;
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
        uiReady = CacheUIElements();

        if (uiReady)
        {
            SetMenuVisible(false);
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
            Debug.LogWarning("PauseMenuUI: UI elements not ready. Cannot register callbacks.");
            return;
        }

        RegisterButtonCallbacks();

        //managers = FindFirstObjectByType<Managers>(); // Don't DestryOnLoad 된 매니저이면 그냥 부른게 낫지 않나?
        managers = Managers.Instance;

        if(managers == null || managers.InputManager == null)
        {
            Debug.LogError($"{name}: Managers 또는 InputManager를 찾지 못했습니다. Start 씬부터 실행했는지 확인하세요.", this);
            return;
        }

        inputManager = managers.InputManager;
        inputManager.OnPauseInputEvent += TogglePauseMenu;

        SetMenuVisible(false);
        SetButtonsEnabled(true);
    }

    private void OnDisable()
    {
        if(inputManager != null)
        {
            inputManager.OnPauseInputEvent -= TogglePauseMenu;
        }

        UnregisteButtonCallbacks();
        ReleasePauseState();

        inputManager = null;
        managers = null;
        isSceneTransitioning = false;
    }

    private bool CacheUIElements()
    {
        if(uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError($"{name}: UIDocument가 없습니다.", this);
            return false;
        }

        VisualElement root = uiDocument.rootVisualElement;

        pauseOverlay = root.Q<VisualElement>("PauseOverlay");
        resumeButton = root.Q<Button>("ResumeButton");
        saveButton = root.Q<Button>("SaveButton");
        loadButton = root.Q<Button>("LoadButton");
        optionsButton = root.Q<Button>("OptionsButton");
        titleButton = root.Q<Button>("TitleButton");

        bool foundAll = pauseOverlay != null && resumeButton != null 
                    && saveButton != null && loadButton != null 
                    && optionsButton != null && titleButton != null;

        if (!foundAll)
        {
            Debug.LogError($"{name}: PauseMenu UXML에서 필요한 UI 요소를 찾지 못했습니다.", this);
        }

        return foundAll;
    }

    private void RegisterButtonCallbacks()
    {
        if (callbacksRegistered) return;

        resumeButton.clicked += ClosePauseMenu;
        saveButton.clicked += LogSaveButton;
        loadButton.clicked += LogLoadButton;
        optionsButton.clicked += LogOptionsButton;
        titleButton.clicked += GoToTitleScene;

        callbacksRegistered  = true;
    }

    private void UnregisteButtonCallbacks()
    {
        if (!callbacksRegistered) return;

        resumeButton.clicked -= ClosePauseMenu;
        saveButton.clicked -= LogSaveButton;
        loadButton.clicked -= LogLoadButton;
        optionsButton.clicked -= LogOptionsButton;
        titleButton.clicked -= GoToTitleScene;

        callbacksRegistered  = false;
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
        if(pauseOverlay == null) return;
        pauseOverlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void TogglePauseMenu()
    {
        if (isSceneTransitioning) return;
        if (isPauseMenuOpen)
        {
            ClosePauseMenu();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    private void OpenPauseMenu()
    {
        if(isPauseMenuOpen || inputManager == null) return;
        previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;

        isPauseMenuOpen = true;
        inputManager.SetGameplayInputEnabled(false);

        Time.timeScale = 0f;
        SetMenuVisible(true);
        resumeButton.Focus();
    }

    private void ClosePauseMenu()
    {
        if(!isPauseMenuOpen || isSceneTransitioning) return;
        ReleasePauseState();
    }

    private void ReleasePauseState()
    {
        if(!isPauseMenuOpen) return;
        Time.timeScale = previousTimeScale;
        
        if(inputManager != null) inputManager.SetGameplayInputEnabled(true);

        isPauseMenuOpen = false;
        SetMenuVisible(false);
    }

    private void GoToTitleScene()
    {
        if(!isPauseMenuOpen || isSceneTransitioning) return;
        if(managers == null || managers.SceneManager == null)
        {
            Debug.LogError($"{name} : SceneManager를 찾지 못했음.", this);
            return;
        }

        isSceneTransitioning = true;
        SetButtonsEnabled(false);

        managers.SceneManager.LoadSceneAsync(SceneType.Start);
    }

    #region  temporary button callbacks
    /// <summary>
    /// 이 구간에 있는 함수들은 임시로 PauseMenu 버튼에 연결된 콜백들입니다. 실제 게임에서는 Save, Load, Options UI가 구현되면 이 함수들을 대체해야 합니다.
    /// </summary>
    private void LogSaveButton()
    {
        Debug.LogWarning("[PauseMenu] Save UI is not implemented yet.");
    }

    private void LogLoadButton()
    {
        Debug.LogWarning("[PauseMenu] Load UI is not implemented yet.");
    }

    private void LogOptionsButton()
    {
        Debug.LogWarning("[PauseMenu] Options UI is not implemented yet.");
    }
    #endregion ==========================

}
