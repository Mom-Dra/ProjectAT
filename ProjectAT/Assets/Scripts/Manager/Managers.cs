using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class Managers : Singleton<Managers>
{
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

    private CursorManager cursorManager;
    private InteractionManager interactionManager = new InteractionManager();
    private UIManager uIManager;
    private PoolManager poolManager;

    public CursorManager CursorManager => cursorManager;
    public InteractionManager InteractionManager => interactionManager;
    public UIManager UIManager => uIManager;
    public PoolManager PoolManager => poolManager;

    [SerializeField]
    protected override void Awake()
    {
        base.Awake();

        cursorManager = new CursorManager(cursorSettings);
        uIManager = new UIManager(healthUIPrefab);
        poolManager = new PoolManager(pooledPrefabs, poolParentTransform);


        uIManager.ShowHealthUI(target.GetComponent<EntityStatus>());
        //uIManager.EnableEnemyHealthUI();
        //uIManager.SetHpBarFollowingTarget(target);
    }

    private void Update()
    {
        interactionManager.Update();
    }
}
