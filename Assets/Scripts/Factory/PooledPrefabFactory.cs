using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// FACTORY PATTERN (template method): the base class owns the pooling/validation workflow and
/// subclasses fill in the type-specific steps (SupportsPrefab, AssignPool, OnGet, OnRelease).
/// </summary>
public abstract class PooledPrefabFactory : ScriptableObject
{
    [Header("Factory Configuration")] [SerializeField]
    private GameObject prefab;

    [SerializeField] private string instanceName = "Pooled Object";
    [SerializeField, Min(1)] private int defaultCapacity = 4;
    [SerializeField, Min(1)] private int maxPoolSize = 20;

    private IObjectPool<GameObject> pool;

    protected abstract bool SupportsPrefab(GameObject candidate);
    protected abstract void AssignPool(GameObject instance, IObjectPool<GameObject> ownerPool);
    protected abstract void OnGet(GameObject instance);
    protected abstract void OnRelease(GameObject instance);

    protected GameObject GetInstance(Vector3 position, Quaternion rotation)
    {
        if (!ValidateConfiguration())
            return null;

        EnsurePool();
        GameObject instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.name = instanceName;
        OnGet(instance);
        instance.SetActive(true);
        return instance;
    }

    private bool ValidateConfiguration()
    {
        if (prefab == null)
        {
            Debug.LogError($"[Factory] Prefab missing on {name}!", this);
            return false;
        }

        if (!SupportsPrefab(prefab))
        {
            Debug.LogError($"[Factory] Prefab {prefab.name} is not supported by {name}!", this);
            return false;
        }

        return true;
    }

    private void EnsurePool()
    {
        if (pool != null)
            return;

        int capacity = Mathf.Max(1, defaultCapacity);
        pool = new ObjectPool<GameObject>(
            CreatePooledObject,
            actionOnRelease: ReleasePooledObject,
            actionOnDestroy: DestroyPooledObject,
            collectionCheck: true,
            defaultCapacity: capacity,
            maxSize: Mathf.Max(capacity, maxPoolSize));
    }

    private GameObject CreatePooledObject()
    {
        GameObject instance = Instantiate(prefab);
        instance.name = instanceName;
        instance.SetActive(false);
        AssignPool(instance, pool);
        return instance;
    }

    private void ReleasePooledObject(GameObject instance)
    {
        OnRelease(instance);
        instance.SetActive(false);
    }

    private static void DestroyPooledObject(GameObject instance)
    {
        Destroy(instance);
    }
}
