using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class StartSceneMenuUI : MonoBehaviour
{
    [Header("UGUI Screens")]
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject missionSelectScreen;
    [SerializeField] private GameObject loadGameScreen;

    [Header("UGUI Interaction")]
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;

    [Header("UGUI Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button[] returnButtons;

    [Header("UI Toolkit")]
    [SerializeField] private OptionMenuUI optionMenuUI;

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        newGameButton.onClick.AddListener(ShowMissionSelect);
        optionButton.onClick.AddListener(ShowOption);

        foreach (Button button in returnButtons)
        {
            if (button != null)
            {
                button.onClick.AddListener(ShowMainMenu);
            }
        }

        optionMenuUI.SaveCompleted += HandleOptionClosed;
        optionMenuUI.CancelCompleted += HandleOptionClosed;
    }

    private void UnSubscribeEvents()
    {
        newGameButton.onClick.RemoveListener(ShowMissionSelect);
        optionButton.onClick.RemoveListener(ShowOption);

        foreach (Button button in returnButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(ShowMainMenu);
            }
        }

        optionMenuUI.SaveCompleted -= HandleOptionClosed;
        optionMenuUI.CancelCompleted -= HandleOptionClosed;
    }

    private void ShowMissionSelect()
    {
        SetMainMenuInteractive(false);

        missionSelectScreen.SetActive(true);
        loadGameScreen.SetActive(false);
    }

    private void ShowMainMenu()
    {
        mainMenuScreen.SetActive(true);
        missionSelectScreen.SetActive(false);
        loadGameScreen.SetActive(false);

        SetMainMenuInteractive(true);
    }

    private void ShowOption()
    {
        optionMenuUI.Open();

        // OptionManager가 준비되지 않아 Open이 실패할 수도 있다.
        if (!optionMenuUI.IsOpen)
        {
            return;
        }

        missionSelectScreen.SetActive(false);
        loadGameScreen.SetActive(false);

        // 뒤쪽 UGUI가 키보드/마우스 입력을 받지 않게 한다.
        SetMainMenuInteractive(false);
    }

    private void HandleOptionClosed()
    {
        ShowMainMenu();
        StartCoroutine(RestoreOptionButtonFocus());
    }

    private void SetMainMenuInteractive(bool interactive)
    {
        if (mainMenuCanvasGroup == null)
        {
            return;
        }

        mainMenuCanvasGroup.interactable = interactive;
        mainMenuCanvasGroup.blocksRaycasts = interactive;
    }

    private IEnumerator RestoreOptionButtonFocus()
    {
        // 현재 클릭/포커스 이벤트 처리가 끝난 뒤 선택을 해제한다.
        yield return null;

        EventSystem.current?.SetSelectedGameObject(null);
    }
}