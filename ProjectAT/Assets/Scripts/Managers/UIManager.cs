using Unity.Services.Lobbies.Models;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField]private Canvas canvas;
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private EntityStatus playerStatus;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        canvas = GetComponent<Canvas>();
        playerHUD = FindAnyObjectByType<PlayerHUD>();
    }

    public void SetPlayerStatus(EntityStatus status)
    {
        playerStatus = status;
    }

    public void UpdateHealthUI(float currentHealth)
    {
        if (playerHUD != null)
        {
            playerHUD.UpdateCurrentHealthUI(currentHealth, 100f); //100f는 나중에 maxHealth를 얻으면 그때 갱신
        }
    }

}
