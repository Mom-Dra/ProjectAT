using UnityEngine;
using static CursorSettings;

public enum CursorType
{
    Default,
    Crosshair,
    Door
}

public class CursorManager
{
    private CursorSettings cursorSettings;

    public CursorManager(CursorSettings cursorSettings)
    {
        if (cursorSettings is null)
            Debug.LogError("cursorSettings is null");

        this.cursorSettings = cursorSettings;
    }

    public void SetImage(CursorType cursorType)
    {
        CursorMapping cursorMapping = cursorSettings.GetCursor(cursorType);
        Cursor.SetCursor(cursorMapping.texture, cursorMapping.hotspot, CursorMode.Auto);
    }
}
