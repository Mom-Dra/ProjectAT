using System;
using UnityEngine;
using ProjectAT.Mission;

public class InGameManager : MonoBehaviour
{
    private static InGameManager instance;


    [SerializeField] private LayerMask interactionLayerMask;
    [SerializeField] private GameObject healthUIPrefab;
    [SerializeField] private PoolConfigObject[] pooledPrefabs;
    [SerializeField] private Camera mainCamera;

    private bool initialized;
    public static InGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<InGameManager>();
                instance?.Initialize();
            }

            return instance;
        }
    }

    public InteractionUIManager InteractionManager { get; private set; }
    public InGameUIManager UIManager { get; private set; }
    public PoolManager PoolManager { get; private set; }
    public EventManager EventManager { get; private set; }
    public MissionManager MissionManager {get; private set;}

     #region Character Selection System ===
    [SerializeField] private PlayerController selectedPlayer;
    public event Action<PlayerController> SelectedPlayerChanged; // 캐릭터 선택 시스템을 위해 일단 이벤트 파놓음. 따로 다른 매니저에 보내야할지도.
    public PlayerController SelectedPlayer => selectedPlayer;

    /// <summary>
    /// 선택된 플레이어를 변경합니다.
    /// </summary>
    /// <param name="player"></param>
    public void SetSelectedPlayer(PlayerController player)
    {
        if (selectedPlayer == player)
        {
            return;
        }

        selectedPlayer = player;
        SelectedPlayerChanged?.Invoke(selectedPlayer); // 연결되는 함수가 적으면 그냥 직접 호출하게 하는 방식도 나쁘진 않을지도
    }
    #endregion ============================


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Initialize();

    }

    private void Update()
    {
        InteractionManager?.Update();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Initialize()
    {
        if (initialized) return;
        if (mainCamera == null) mainCamera = Camera.main;
        if( selectedPlayer == null)
        {
            selectedPlayer = FindAnyObjectByType<PlayerController>();
        }

        PoolManager = new PoolManager(pooledPrefabs);
        EventManager = new EventManager();
        InteractionManager = new InteractionUIManager(Managers.Instance.InputManager, interactionLayerMask, mainCamera);
        
        MissionManager = GetComponent<MissionManager>();
        UIManager = GetComponent<InGameUIManager>();

        initialized = true;
    }
}
