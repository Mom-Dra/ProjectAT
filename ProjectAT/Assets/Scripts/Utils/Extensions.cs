using System;
using Unity.VisualScripting;
using UnityEngine;

public static class Extensions
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject.TryGetComponent(out T component))
            return component;

        return gameObject.AddComponent<T>();
    }

    public static Transform FindChild(this Transform transform, string name = null)
    {
        return Util.FindChild<Transform>(transform, name);
    }

    public static Transform FindChildRecursive(this Transform transform, string name = null)
    {
        return Util.FindChildRecursive<Transform>(transform, name);
    }

    public static int GetLayerMask(this GameObject gameObject)
    {
        return 1 << gameObject.layer;
    }

    public static bool IsSameLayer(this GameObject gameObject, LayerMask layerMask)
    {
        return (layerMask.value & (1 << gameObject.layer)) != 0;
    }
}
