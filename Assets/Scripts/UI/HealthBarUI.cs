using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Subscriber: Listens to health events and updates UI accordingly.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private void OnEnable()
    {
        // Subscribe to event broadcast
        PlayerHealthSystem.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDisable()
    {
        // MANDATORY: Unsubscribe to prevent memory leaks and ghost execution
        PlayerHealthSystem.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (fillImage != null && maxHealth > 0)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }
    }
}