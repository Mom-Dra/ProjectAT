using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public static InGameUI Instance { get; private set; }
    
    [SerializeField] private Texture2D[] cursors;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void ChangeCursor(CursorType newType)
    {
        if( newType == CursorType.Default)         {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        Cursor.SetCursor(cursors[(int)newType], Vector2.zero, CursorMode.Auto);
    }
}
