using UnityEngine;

[CreateAssetMenu(fileName = "CursorSettings", menuName = "Scriptable Objects/CursorSettings")]
public class CursorSettings : ScriptableObject
{
    [System.Serializable]
    public struct CursorMapping
    {
        public CursorType type;
        public Texture2D texture;
        public Vector2 hotspot;
    }

    [SerializeField]
    private CursorMapping[] cursors;

    public CursorMapping GetCursor(CursorType type)
    {
        return cursors[(int)type];
    }
}
