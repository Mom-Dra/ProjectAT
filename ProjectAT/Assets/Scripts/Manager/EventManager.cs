using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum EventType
{
    TargetDied,
    PlayerDied,
    Last
}

public class EventManager
{
    private readonly IDictionary<EventType, Delegate> events = new Dictionary<EventType, Delegate>();

    public void Subscribe<T>(EventType eventType, Action<T> listener)
    {
        if (events.TryGetValue(eventType, out Delegate del))
        {
            events[eventType] = Delegate.Combine(del, listener);
        }
        else
        {
            events[eventType] = listener;
        }
    }

    public void UnSubscribe<T>(EventType eventType, Action<T> listener)
    {
        if (events.TryGetValue(eventType, out Delegate del))
        {
            Delegate minusListner = Delegate.Remove(del, listener);

            if (minusListner is null) events.Remove(eventType);
            else events[eventType] = minusListner;
        }
    }

    public void Publish<T>(EventType eventType, T param)
    {
        if (events.TryGetValue(eventType, out Delegate del))
            (del as Action<T>)?.Invoke(param);
    }
}
