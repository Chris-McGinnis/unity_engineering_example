using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float speed = 5f;

    private Rigidbody2D body;
    private IObjectPool<GameObject> pool;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null)
        {
            Debug.LogError("[Projectile] A Rigidbody2D component is required.", this);
        }
    }

    public void Initialize(float damage, Vector2 direction)
    {
        this.damage = damage;
        if (body != null)
        {
            body.linearVelocity = direction.normalized * speed;
        }
    }

    public void SetPool(IObjectPool<GameObject> ownerPool)
    {
        pool = ownerPool;
    }

    public void OnPoolGet()
    {
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    public void OnPoolRelease()
    {
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<IDamageable>(out IDamageable target))
        {
            target.TakeDamage(damage);
        }

        ReleaseToPool();
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
}
