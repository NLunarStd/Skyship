using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private readonly Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();

    private readonly Dictionary<Delegate, Delegate> _delegateLookup = new Dictionary<Delegate, Delegate>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public static void Publish<T>(T gameEvent) where T : IGameEvent
    {
        if (Instance == null) return;

        if (Instance._events.TryGetValue(typeof(T), out Delegate del))
        {
            if (del is Action<T> callback)
            {
                callback.Invoke(gameEvent);
            }
        }
    }

    public static void Subscribe<T>(Action<T> listener) where T : IGameEvent
    {
        if (Instance == null)
        {
            Debug.LogWarning($"EventManager: Cannot subscribe to {typeof(T).Name} because Instance is null!");
            return;
        }
        if (Instance._delegateLookup.ContainsKey(listener)) return;

        Instance._delegateLookup[listener] = listener;

        if (Instance._events.TryGetValue(typeof(T), out Delegate existingDel))
        {
            Instance._events[typeof(T)] = Delegate.Combine(existingDel, listener);
        }
        else
        {
            Instance._events[typeof(T)] = listener;
        }
    }

    public static void UnSubscribe<T>(Action<T> listener) where T : IGameEvent
    {
        if (Instance == null) return;

        if (Instance._events.TryGetValue(typeof(T), out Delegate existingDel))
        {
            Delegate newDel = Delegate.Remove(existingDel, listener);

            if (newDel == null)
            {
                Instance._events.Remove(typeof(T));
            }
            else
            {
                Instance._events[typeof(T)] = newDel;
            }

            Instance._delegateLookup.Remove(listener);
        }
    }
}