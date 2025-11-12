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

    public static void RedLog(string message)
    {
        Log(message, Color.red);
    }

    public static void GreenLog(string message)
    {
        Log(message, Color.green);
    }

    public static void BlueLog(string message)
    {
        Log(message, Color.blue);
    }

    public static void OrangeLog(string message)
    {
        Log(message, Color.orange);
    }
}
