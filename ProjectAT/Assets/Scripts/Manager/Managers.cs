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

    private InputManager inputManager;
    private CursorManager cursorManager;
    private SceneManager sceneManager;
    private SoundManager soundManager;

    public InputManager InputManager => inputManager;
    public CursorManager CursorManager => cursorManager;
    public SceneManager SceneManager => sceneManager;
    public SoundManager SoundManager => soundManager;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        inputManager = new InputManager(inputReader);
        cursorManager = new CursorManager(cursorSettings);
        sceneManager = new SceneManager();
        soundManager = GetComponent<SoundManager>();

        sceneManager.Initialize();
    }
}
