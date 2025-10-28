using UnityEngine;
using System.Diagnostics;

public static class ColorDebug
{
    [Conditional("UNITY_EDITOR")]
    public static void Log(string message, Color color)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        UnityEngine.Debug.Log($"<color=#{hexColor}>{message}</color>");
    }
}
