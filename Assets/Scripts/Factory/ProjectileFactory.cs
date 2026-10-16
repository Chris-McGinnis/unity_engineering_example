using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "ProjectileFactory", menuName = "McG/Factories/Projectile Factory")]
public class ProjectileFactory : PooledPrefabFactory
{
    [Header("Projectile Configuration")]
    [SerializeField, Min(0f)] private float damage = 10f;

    protected override bool SupportsPrefab(GameObject candidate)
    {
        return candidate.GetComponent<Projectile>() != null;
    }

    protected override void AssignPool(GameObject instance, IObjectPool<GameObject> ownerPool)
    {
        instance.GetComponent<Projectile>().SetPool(ownerPool);
    }

    protected override void OnGet(GameObject instance)
    {
        instance.GetComponent<Projectile>().OnPoolGet();
    }

    protected override void OnRelease(GameObject instance)
    {
        instance.GetComponent<Projectile>().OnPoolRelease();
    }

    public Projectile CreateProjectile(Vector3 position, Quaternion rotation, Vector2 direction)
    {
        Projectile projectile = GetInstance(position, rotation)?.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(damage, direction);
        }

        return projectile;
    }
}
