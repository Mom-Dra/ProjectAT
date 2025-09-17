using System;
using UnityEngine;

public class Util
{
    public static T FindChild<T>(Transform transform, string name = null) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (transform is null)
            throw new ArgumentNullException();
#else
        if (transform is null) return null;
#endif

        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            if (string.IsNullOrEmpty(name) || transform.name.Equals(name))
            {
                if (child.TryGetComponent(out T component))
                    return component;
            }
        }

        return null;
    }

    public static T FindChildRecursive<T>(Transform transform, string name) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (transform is null)
            throw new ArgumentNullException();
#else
        if (transform is null) return null;
#endif
        foreach (T component in transform.GetComponentsInChildren<T>())
        {
            if (string.IsNullOrEmpty(name) || component.name.Equals(name))
                return component;
        }

        return null;
    }
}
