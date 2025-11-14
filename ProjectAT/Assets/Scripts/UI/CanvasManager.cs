using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{

    #region legacy Codes
    /* 
     [SerializeField] private Button hostButton;
     [SerializeField] private Button clientButton;
     [SerializeField] private Button enemySpawnButton;
     [SerializeField] private GameObject enemyPrefab;
     [SerializeField] private Transform spawnTf;

     private void Awake()
     {
         hostButton = transform.GetChild(0).GetChild(0).GetComponent<Button>();
         clientButton = transform.GetChild(0).GetChild(1).GetComponent<Button>();
         enemySpawnButton = transform.GetChild(0).GetChild(2).GetComponent<Button>();

         hostButton.onClick.AddListener(OnHostButtonClicked);
         clientButton.onClick.AddListener(OnClientButtonClicked);
         enemySpawnButton.onClick.AddListener(OnEnemyButtonClicked);
     }

     private void OnHostButtonClicked()
     {
         NetworkManager.Singleton.StartHost();
     }

     private void OnClientButtonClicked()
     {
         NetworkManager.Singleton.StartClient();
     }

     private void OnEnemyButtonClicked()
     {
         if (NetworkManager.Singleton.IsServer)
         {
             var Enemy = Instantiate(enemyPrefab, spawnTf.position, Quaternion.identity);
             Enemy.GetComponent<NetworkObject>().Spawn();
         }
     }*/
    #endregion
}
