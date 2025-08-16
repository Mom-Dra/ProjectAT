using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkView : MonoBehaviour
{
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button clientButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(HostButtonClicked);
        clientButton.onClick.AddListener(ClientButtonClicked);
    }
    
    private void HostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
    }

    private void ClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
    }
}
