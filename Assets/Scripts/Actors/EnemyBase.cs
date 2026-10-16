using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public float moveSpeed;
    [SerializeField, Min(0f)] protected float stoppingDistance = 0.5f;

    [Header("Scene Placement Defaults")]
    [Tooltip("Used only when the enemy is placed in a scene instead of spawned by a factory.")]
    [SerializeField, Min(0f)]
    protected float defaultMoveSpeed = 3.5f;

    [SerializeField, Min(1f)] protected float defaultMaxHealth = 100f;

    [Header("Contact Damage")] [SerializeField, Min(0f)]
    private float contactDamage = 10f;

    [SerializeField, Min(0f)] private float contactCooldown = 1f;

    [Header("Hit Flash")] [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField, Min(0f)] private float flashDuration = 0.1f;

    [Tooltip("Scales incoming damage. Below 1 makes the enemy armored.")]
    [SerializeField, Min(0f)] private float damageMultiplier = 1f;

    private IObjectPool<GameObject> pool;
    private Color baseColor = Color.white;
    private Coroutine flashRoutine;
    private float nextContactDamageTime;
    protected Rigidbody2D body;
    protected Transform player;
    private bool initialized;

    // OBSERVER (subject side): raised when this enemy dies. LevelSpawner listens.
    public event Action<EnemyBase> ReturnedToPool;
    public bool IsPooled { get; private set; } = true;

    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void TryDamagePlayer(Collision2D collision)
    {
        if (IsPooled || Time.time < nextContactDamageTime || !collision.collider.CompareTag("Player"))
            return;

        if (collision.collider.GetComponentInParent<IDamageable>() is IDamageable target)
        {
            target.TakeDamage(contactDamage);
            nextContactDamageTime = Time.time + contactCooldown;
        }
    }

    private void Flash()
    {
        if (spriteRenderer == null || !isActiveAndEnabled)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        ResetFlash();
    }

    private void ResetFlash()
    {
        flashRoutine = null;
        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;
    }

    private void Start()
    {
        if (initialized)
            return;

        // Enemy was placed in the scene, so no factory configured it.
        Initialize(defaultMoveSpeed, defaultMaxHealth);
        IsPooled = false;
    }

    protected bool TryFindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
        return player != null;
    }

    public void Initialize(float speed, float health)
    {
        initialized = true;
        moveSpeed = speed;
        MaxHealth = health;
        CurrentHealth = health;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || CurrentHealth <= 0f)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount * DamageMultiplier);
        Flash();
        if (CurrentHealth <= 0f)
            ReleaseToPool();
    }

    protected virtual float DamageMultiplier => damageMultiplier;

    public void SetPool(IObjectPool<GameObject> ownerPool)
    {
        pool = ownerPool;
    }

    public virtual void OnPoolGet()
    {
        IsPooled = false;
        nextContactDamageTime = 0f;
        ResetFlash();
        player = null;
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }

    public virtual void OnPoolRelease()
    {
        IsPooled = true;
        CurrentHealth = 0f;
        ResetFlash();
        player = null;
        ReturnedToPool?.Invoke(this);
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }

    public void ReleaseToPool()
    {
        if (pool != null)
        {
            pool.Release(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
