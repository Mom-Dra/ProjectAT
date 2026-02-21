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
    private Transform poolParentTransform;

    [SerializeField]
    private Transform target;
    

    private InputManager inputManager;
    private CursorManager cursorManager;
    private InteractionManager interactionManager;
    private UIManager uIManager;
    private PoolManager poolManager;

    public InputManager InputManager => inputManager;
    public CursorManager CursorManager => cursorManager;
    public InteractionManager InteractionManager => interactionManager;
    public UIManager UIManager => uIManager;
    public PoolManager PoolManager => poolManager;

    protected override void Awake()
    {
        base.Awake();

        Debug.LogError("InputManager");

        inputManager = new InputManager(inputReader);
        cursorManager = new CursorManager(cursorSettings);
        interactionManager = new InteractionManager(interactionLayerMask);
        uIManager = new UIManager(healthUIPrefab, FindFirstObjectByType<PlayerHUD>());
        poolManager = new PoolManager(pooledPrefabs, poolParentTransform);


        uIManager.ShowHealthUI(target.GetComponent<EntityStatus>());
        //uIManager.EnableEnemyHealthUI();
        //uIManager.SetHpBarFollowingTarget(target);
    }

    private void Update()
    {
        interactionManager.Update();
        uIManager.Update();
    }
}
