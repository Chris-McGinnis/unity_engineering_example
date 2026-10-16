using UnityEngine;
using UnityEngine.Pool;

public class DestructibleCrate : MonoBehaviour, IDamageable
{
    [Tooltip("Only needed for crates placed in a scene; factory-spawned crates set this automatically.")]
    [SerializeField]
    private CrateFactory dropSource;

    private float maxHealth = 50f;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;

    private IObjectPool<GameObject> pool;
    private Rigidbody2D body;
    private CrateFactory owner;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        OnPoolGet();
    }

    public void Configure(float health, CrateFactory ownerFactory)
    {
        maxHealth = Mathf.Max(1f, health);
        owner = ownerFactory;
    }

    public void SetPool(IObjectPool<GameObject> ownerPool)
    {
        pool = ownerPool;
    }

    public void OnPoolGet()
    {
        CurrentHealth = maxHealth;
        ResetVelocity();
    }

    public void OnPoolRelease()
    {
        CurrentHealth = 0f;
        ResetVelocity();
    }

    public void Break()
    {
        CrateFactory dropper = owner != null ? owner : dropSource;
        if (dropper != null)
            dropper.SpawnRandomDrop(transform.position);
        ReleaseToPool();
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || CurrentHealth <= 0f)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        if (CurrentHealth <= 0f)
        {
            Break();
        }
    }

    private void ReleaseToPool()
    {
        if (pool != null)
        {
            pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ResetVelocity()
    {
        if (body == null)
            return;

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }
}
