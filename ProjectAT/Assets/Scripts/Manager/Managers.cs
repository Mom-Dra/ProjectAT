using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEditor.SearchService;
using UnityEngine;

public class Managers : Singleton<Managers>
{
    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private CursorSettings cursorSettings;

    [SerializeField]
    private PoolConfigObject[] pooledPrefabs;

    private InputManager inputManager;
    private CursorManager cursorManager;
    private PoolManager poolManager;
    private SceneManager sceneManager;
    private EventManager eventManager;
    private SoundManager soundManager;

    public InputManager InputManager => inputManager;
    public CursorManager CursorManager => cursorManager;
    public PoolManager PoolManager => poolManager;
    public SceneManager SceneManager => sceneManager;
    public EventManager EventManager => eventManager;
    public SoundManager SoundManager => soundManager;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        inputManager = new InputManager(inputReader);
        cursorManager = new CursorManager(cursorSettings);
        poolManager = new PoolManager(pooledPrefabs);
        sceneManager = new SceneManager();
        eventManager = new EventManager();
        soundManager = GetComponent<SoundManager>();

        sceneManager.Initialize();
    }
}
