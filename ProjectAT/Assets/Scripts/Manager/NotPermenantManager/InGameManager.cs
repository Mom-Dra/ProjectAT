using UnityEngine;

public class InGameManager : MonoBehaviour
{
    private static InGameManager instance;

    [SerializeField] private LayerMask interactionLayerMask;
    [SerializeField] private GameObject healthUIPrefab;
    [SerializeField] private PoolConfigObject[] pooledPrefabs;
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private Camera mainCamera;

    private bool initialized;

    public static InGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<InGameManager>();
                instance?.Initialize();
            }

            return instance;
        }
    }

    public InteractionUIManager InteractionManager { get; private set; }
    public InGameUIManager UIManager { get; private set; }
    public PoolManager PoolManager { get; private set; }
    public EventManager EventManager { get; private set; }

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

        if (playerHUD == null) playerHUD = FindFirstObjectByType<PlayerHUD>();

        PoolManager = new PoolManager(pooledPrefabs);
        EventManager = new EventManager();
        UIManager = new InGameUIManager(healthUIPrefab, playerHUD);
        InteractionManager = new InteractionUIManager(Managers.Instance.InputManager, interactionLayerMask, mainCamera);

        initialized = true;
    }
}
