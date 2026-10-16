using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// FACTORY PATTERN: callers ask for an enemy via CreateEnemy() and never call Instantiate or know
/// which concrete class they get. Each factory is a ScriptableObject asset, so designers configure
/// speed/health/prefab in the Inspector. Subclasses only declare which enemy type they accept.
/// The factory also owns the object pool, so spawners stay unaware of reuse.
/// </summary>
public abstract class EnemyFactory : ScriptableObject
{
    [Header("Factory Configuration")] [SerializeField]
    private GameObject enemyPrefab;    
    [SerializeField, Min(1)] private int defaultCapacity = 4;
    [SerializeField, Min(1)] private int maxPoolSize = 20;

    [Header("Enemy Configuration")]
    [SerializeField] private string enemyName = "Enemy";
    [SerializeField] private float baseMoveSpeed = 3.5f;
    [SerializeField] private float maxHealth = 100f;

    private IObjectPool<GameObject> pool;

    protected abstract bool SupportsEnemy(EnemyBase enemy);

    public GameObject CreateEnemy(Vector3 spawnPosition, Quaternion rotation)
    {
        if (!ValidateConfiguration())
            return null;

        EnsurePool();
        GameObject instance = pool.Get();
        instance.transform.SetPositionAndRotation(spawnPosition, rotation);
        instance.name = enemyName;

        EnemyBase enemy = instance.GetComponent<EnemyBase>();
        enemy.Initialize(baseMoveSpeed, maxHealth);
        enemy.OnPoolGet();
        instance.SetActive(true);
        return instance;
    }
// THIS WILL NOT SHOW UP IN THE INSPECTOR BY DEFAULT
    [SerializeField] private IDamageable target;
    private bool ValidateConfiguration()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"[Factory] Prefab missing on {name}!", this);
            return false;
        }

        if (enemyPrefab.GetComponent<EnemyBase>() is not EnemyBase enemy || !SupportsEnemy(enemy))
        {
            Debug.LogError($"[Factory] Prefab {enemyPrefab.name} does not contain a supported enemy for {name}!", this);
            return false;
        }

        if (maxHealth <= 0f)
        {
            Debug.LogError($"[Factory] Max health must be greater than zero on {name}!", this);
            return false;
        }

        return true;
    }

    private void EnsurePool()
    {
        if (pool != null)
            return;

        int capacity = Mathf.Max(1, defaultCapacity);
        int poolSize = Mathf.Max(capacity, maxPoolSize);
        pool = new ObjectPool<GameObject>(
            CreatePooledEnemy,
            actionOnRelease: ReleasePooledEnemy,
            actionOnDestroy: DestroyPooledEnemy,
            collectionCheck: true,
            defaultCapacity: capacity,
            maxSize: poolSize);
    }

    private GameObject CreatePooledEnemy()
    {
        GameObject instance = Instantiate(enemyPrefab);
        instance.name = enemyName;
        instance.SetActive(false);

        instance.GetComponent<EnemyBase>().SetPool(pool);
        return instance;
    }

    private static void ReleasePooledEnemy(GameObject instance)
    {
        instance.GetComponent<EnemyBase>().OnPoolRelease();
        instance.SetActive(false);
    }

    private static void DestroyPooledEnemy(GameObject instance)
    {
        Destroy(instance);
    }
}
