using System;
using System.Collections.Generic;
using UnityEngine;

public class DestructionTracker : MonoBehaviour
{
    public static DestructionTracker Instance { get; private set; }

    public event Action OnChanged;

    public readonly struct Event
    {
        public readonly string Type;
        public readonly int Points;

        public Event(string type, int points)
        {
            Type = type;
            Points = points;
        }
    }

    readonly List<Event> _events = new List<Event>();

    public IReadOnlyList<Event> Events => _events;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterDestruction(string type, int points)
    {
        if (string.IsNullOrEmpty(type)) type = "Unknown";
        _events.Add(new Event(type, points));
        OnChanged?.Invoke();
    }

    public void ResetEvents()
    {
        _events.Clear();
        OnChanged?.Invoke();
    }
}
