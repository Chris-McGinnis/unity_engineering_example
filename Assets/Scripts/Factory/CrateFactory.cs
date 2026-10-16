using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "CrateFactory", menuName = "McG/Factories/Crate Factory")]
public class CrateFactory : PooledPrefabFactory
{
    [Header("Crate Configuration")] [SerializeField, Min(1f)]
    private float maxHealth = 50f;

    [Header("Drops")]
    [Tooltip("One random entry is spawned when a crate breaks. Duplicate entries increase the odds.")]
    [SerializeField]
    private List<GameObject> dropPrefabs = new List<GameObject>();

    [SerializeField, Range(0f, 1f)] private float dropChance = 1f;

    protected override bool SupportsPrefab(GameObject candidate)
    {
        return candidate.GetComponent<DestructibleCrate>() != null;
    }

    protected override void AssignPool(GameObject instance, IObjectPool<GameObject> ownerPool)
    {
        instance.GetComponent<DestructibleCrate>().SetPool(ownerPool);
    }

    protected override void OnGet(GameObject instance)
    {
        DestructibleCrate crate = instance.GetComponent<DestructibleCrate>();
        crate.Configure(maxHealth, this);
        crate.OnPoolGet();
    }

    protected override void OnRelease(GameObject instance)
    {
        instance.GetComponent<DestructibleCrate>().OnPoolRelease();
    }

    public GameObject SpawnRandomDrop(Vector3 position)
    {
        if (dropPrefabs == null || dropPrefabs.Count == 0 || Random.value > dropChance)
            return null;

        GameObject prefab = dropPrefabs[Random.Range(0, dropPrefabs.Count)];
        return prefab != null ? Instantiate(prefab, position, Quaternion.identity) : null;
    }

    public DestructibleCrate CreateCrate(Vector3 position, Quaternion rotation)
    {
        return GetInstance(position, rotation)?.GetComponent<DestructibleCrate>();
    }
}
