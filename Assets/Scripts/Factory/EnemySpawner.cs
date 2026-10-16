using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyFactory[] enemyFactories;
    [SerializeField] private Transform[] spawnPoints;

    private int nextFactoryIndex;
    private int nextSpawnPointIndex;

    [ContextMenu("Spawn Next Enemy")]
    public GameObject SpawnNextEnemy()
    {
        if (enemyFactories == null || enemyFactories.Length == 0)
        {
            Debug.LogError("[EnemySpawner] Assign at least one enemy factory.", this);
            return null;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] Assign at least one spawn point.", this);
            return null;
        }

        EnemyFactory factory = enemyFactories[nextFactoryIndex];
        Transform spawnPoint = spawnPoints[nextSpawnPointIndex];
        if (factory == null || spawnPoint == null)
        {
            Debug.LogError("[EnemySpawner] Factory and spawn point entries cannot be empty.", this);
            return null;
        }

        GameObject enemy = factory.CreateEnemy(spawnPoint.position, spawnPoint.rotation);
        if (enemy == null)
            return null;

        nextFactoryIndex = (nextFactoryIndex + 1) % enemyFactories.Length;
        nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;
        return enemy;
    }
}
