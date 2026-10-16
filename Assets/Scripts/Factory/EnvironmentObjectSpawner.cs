using UnityEngine;

public class EnvironmentObjectSpawner : MonoBehaviour
{
    [SerializeField] private CrateFactory crateFactory;
    [SerializeField] private BarrelFactory barrelFactory;

    public DestructibleCrate SpawnCrate(Vector3 position, Quaternion rotation)
    {
        if (crateFactory == null)
        {
            Debug.LogError("[EnvironmentObjectSpawner] Assign a crate factory.", this);
            return null;
        }

        return crateFactory.CreateCrate(position, rotation);
    }

    public ExplosiveBarrel SpawnBarrel(Vector3 position, Quaternion rotation)
    {
        if (barrelFactory == null)
        {
            Debug.LogError("[EnvironmentObjectSpawner] Assign a barrel factory.", this);
            return null;
        }

        return barrelFactory.CreateBarrel(position, rotation);
    }

    [ContextMenu("Spawn Crate Here")]
    private void SpawnCrateHere()
    {
        SpawnCrate(transform.position, transform.rotation);
    }

    [ContextMenu("Spawn Barrel Here")]
    private void SpawnBarrelHere()
    {
        SpawnBarrel(transform.position, transform.rotation);
    }
}
