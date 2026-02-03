using UnityEngine;

public class Managers : Singleton<Managers>
{
    [SerializeField]
    private CursorSettings cursorSettings;

    private CursorManager cursorManager;
    private InteractionManager interactionManager;

    public CursorManager CursorManager => cursorManager;

    [SerializeField]
    protected override void Awake()
    {
        base.Awake();

        cursorManager = new CursorManager(cursorSettings);
    }

    private void Update()
    {
        interactionManager.Update();
    }
}
