using System;
using System.Collections.Generic;
using UnityEngine;

public class CrowdControlModule : MonoBehaviour
{
    private readonly HashSet<object> stunSources = new HashSet<object>();

    public bool IsStunned => stunSources.Count > 0;

    public event Action OnStunStarted;
    public event Action OnStunEnded;

    public void ApplyStun(object source)
    {
        if (source == null)
        {
            Debug.LogWarning("Cannot apply stun with a null source.", this);
            return;
        }

        bool wasStunned = IsStunned;

        if (!stunSources.Add(source))
        {
            return;
        }

        if (!wasStunned)
        {
            OnStunStarted?.Invoke();
        }
    }

    public void RemoveStun(object source)
    {
        if (source == null)
        {
            return;
        }

        bool wasStunned = IsStunned;

        if (!stunSources.Remove(source))
        {
            return;
        }

        if (wasStunned && !IsStunned)
        {
            OnStunEnded?.Invoke();
        }
    }

    public void ClearStuns()
    {
        if (!IsStunned)
        {
            return;
        }

        stunSources.Clear();
        OnStunEnded?.Invoke();
    }
}
