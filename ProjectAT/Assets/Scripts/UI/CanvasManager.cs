using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        hostButton = transform.GetChild(0).GetChild(0).GetComponent<Button>();
        clientButton = transform.GetChild(0).GetChild(1).GetComponent<Button>();

        hostButton.onClick.AddListener(OnHostButtonClicked);
        clientButton.onClick.AddListener(OnClientButtonClicked);
    }

    public void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void OnClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
    }
}
