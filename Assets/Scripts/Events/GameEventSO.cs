using System;
using UnityEngine;

/// <summary>
/// ScriptableObject Event Channel: Decouples events across scenes and prefabs.
/// </summary>
[CreateAssetMenu(fileName = "NewGameEvent", menuName = "McG/Events/Game Event Channel")]
public class GameEventSO : ScriptableObject
{
    private event Action OnEventRaised;

    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }

    public void Subscribe(Action listener)
    {
        OnEventRaised += listener;
    }

    public void Unsubscribe(Action listener)
    {
        OnEventRaised -= listener;
    }
}