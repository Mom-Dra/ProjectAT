using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject missionSelectScreen;
    [SerializeField] private GameObject loadGameScreen;
    [SerializeField] private GameObject optionScreen;

    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button optionButton;

    [SerializeField] private Button[] returnButton;

    private void Awake()
    {
        newGameButton.onClick.AddListener(NewGameButtonClicked);
        optionButton.onClick.AddListener(ShowOption);

        foreach (Button button in returnButton)
            button.onClick.AddListener(ShowMainMenu);
    }

    private void NewGameButtonClicked()
    {
        ShowMissionSelect();
    }

    private void ShowMissionSelect()
    {
        missionSelectScreen.SetActive(true);
        loadGameScreen.SetActive(false);
        optionScreen.SetActive(false);
    }

    private void ShowMainMenu()
    {
        mainMenuScreen.SetActive(true);
        missionSelectScreen.SetActive(false);
        optionScreen.SetActive(false);
    }

    private void ShowOption()
    {
        optionScreen.SetActive(true);
    }
}
