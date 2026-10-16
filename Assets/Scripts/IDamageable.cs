/// <summary>
/// Contract for any entity in the game that can take damage.
/// Decouples weapon/projectile logic from concrete character classes.
/// </summary>
public interface IDamageable
{
    float CurrentHealth { get; }
    void TakeDamage(float amount);
}
