using UnityEngine;
using System;

/// <summary>
/// Broadcaster: Manages health and notifies subscribers of state changes.
/// Has ZERO dependencies on UI, audio, or visual effect components.
/// </summary>
public class PlayerHealthSystem : MonoBehaviour, IDamageable
{
    // Pure C# events for performance and zero heap allocation on invocation
    public static event Action<float, float> OnHealthChanged; // Current, Max
    public static event Action OnDied;

    [SerializeField] private float maxHealth = 100f;
    public float CurrentHealth { get; private set; }
    private bool isDead;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || isDead)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        // Invoke events safely using the null-conditional operator
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
        {
            isDead = true;
            OnDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || isDead) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}