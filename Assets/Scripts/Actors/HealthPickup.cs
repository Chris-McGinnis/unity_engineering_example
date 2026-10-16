using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField, Min(0f)] private float healAmount = 25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealthSystem health = other.GetComponentInParent<PlayerHealthSystem>();
        if (health == null)
            return;

        health.Heal(healAmount);
        Destroy(gameObject);
    }
}
