using UnityEngine;
using UnityEngine.Events;

public class ZoneTrigger : MonoBehaviour
{
    // OBSERVER: two ways to broadcast. The ScriptableObject channel is code-subscribed (see ZoneTriggerUI),
    // the UnityEvent is wired up in the Inspector. Neither requires this class to know its listeners.
    [SerializeField] GameEventSO OnTriggerEnter;

    [Space(8)]
    [SerializeField] UnityEvent OnTriggerEnterUnityEvent;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnTriggerEnter?.RaiseEvent();
            OnTriggerEnterUnityEvent?.Invoke();
        }
    }
}
