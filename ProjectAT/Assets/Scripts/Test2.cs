using UnityEngine;
using UnityEngine.UI;

public class Test2 : MonoBehaviour
{
    [SerializeField]
    private Button button;

    private Test test;

    private void Awake()
    {
        test = GetComponent<Test>();
        button.onClick.AddListener(ButtonClicked);
    }

    private void ButtonClicked()
    {
        test.HahaRpc();
    }
}
