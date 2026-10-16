using UnityEngine;

public class ZoneTriggerUI : MonoBehaviour
{
    [SerializeField] private GameEventSO PlayerEnteredEvent;

    private void OnEnable()
    {
        PlayerEnteredEvent?.Subscribe(OnPlayerEntered);
    }

    private void OnDisable()
    {
        // Must be the same method that was subscribed; a new lambda would never match and would leak.
        PlayerEnteredEvent?.Unsubscribe(OnPlayerEntered);
    }

    void OnPlayerEntered()
    {
        Debug.Log($"[ZoneTriggerUI] Player entered zone: {gameObject.name}");
    }

}
