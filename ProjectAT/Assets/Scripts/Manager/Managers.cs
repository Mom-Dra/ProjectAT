using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class Managers : Singleton<Managers>
{
    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private LayerMask interactionLayerMask;

    [SerializeField]
    private CursorSettings cursorSettings;

    [SerializeField]
    private PoolConfigObject[] pooledPrefabs;

    [SerializeField]
    private GameObject healthUIPrefab;

    [SerializeField]
    private Transform target;

    private InputManager inputManager;
    private CursorManager cursorManager;
    private InteractionUIManager interactionManager;
    private UIManager uIManager;
    private PoolManager poolManager;

    public InputManager InputManager => inputManager;
    public CursorManager CursorManager => cursorManager;
    public InteractionUIManager InteractionManager => interactionManager;
    public UIManager UIManager => uIManager;
    public PoolManager PoolManager => poolManager;

    protected override void Awake()
    {
        base.Awake();

        inputManager = new InputManager(inputReader);
        cursorManager = new CursorManager(cursorSettings);
        interactionManager = new InteractionUIManager(interactionLayerMask);
        uIManager = new UIManager(healthUIPrefab, FindFirstObjectByType<PlayerHUD>());
        poolManager = new PoolManager(pooledPrefabs);


        //uIManager.ShowHealthUI(target.GetComponent<EntityStatus>());
        //uIManager.EnableEnemyHealthUI();
        //uIManager.SetHpBarFollowingTarget(target);
    }

    private void Start()
    {
        interactionManager.Start();
    }

    private void Update()
    {
        interactionManager.Update();
    }
}
