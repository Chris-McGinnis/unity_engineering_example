using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyWaveEntry
{
    [SerializeField] private EnemyFactory factory;
    [SerializeField, Min(0)] private int count = 1;

    public EnemyFactory Factory => factory;
    public int Count => count;
}

[Serializable]
public class EnemyWave
{
    [SerializeField] private string waveName = "Wave";
    [SerializeField] private List<EnemyWaveEntry> enemies = new List<EnemyWaveEntry>();

    public string WaveName => waveName;
    public IReadOnlyList<EnemyWaveEntry> Enemies => enemies;
}

public class LevelSpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyWave> waves = new List<EnemyWave>();
    [SerializeField] private Transform[] spawnLocations;
    [SerializeField, Min(0f)] private float delayBetweenSpawns = 0.5f;
    [SerializeField, Min(0f)] private float delayBetweenWaves = 3f;
    [SerializeField] private bool startOnEnable = true;

    private readonly HashSet<EnemyBase> activeEnemies = new HashSet<EnemyBase>();
    private Coroutine spawnRoutine;
    private int waveIndex;
    private int entryIndex;
    private int spawnedFromEntry;
    private int nextSpawnLocation;
    private bool playerDied;

    private void OnEnable()
    {
        PlayerHealthSystem.OnDied += HandlePlayerDied;
        if (startOnEnable)
        {
            StartWaves();
        }
    }

    private void OnDisable()
    {
        PlayerHealthSystem.OnDied -= HandlePlayerDied;
        StopSpawning();
    }

    private void HandlePlayerDied()
    {
        playerDied = true;
        StopSpawning();
    }

    private void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = null;
    }

    private void OnDestroy()
    {
        foreach (EnemyBase enemy in activeEnemies)
        {
            if (enemy != null)
                enemy.ReturnedToPool -= HandleEnemyReturnedToPool;
        }

        activeEnemies.Clear();
    }

    public void StartWaves()
    {
        if (playerDied || spawnRoutine != null)
            return;

        if (!ValidateConfiguration())
            return;

        if (waveIndex >= waves.Count)
            return;

        spawnRoutine = StartCoroutine(SpawnWaves());
    }

    private bool ValidateConfiguration()
    {
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("[LevelSpawner] Configure at least one wave.", this);
            return false;
        }

        if (spawnLocations == null || spawnLocations.Length == 0)
        {
            Debug.LogError("[LevelSpawner] Configure at least one spawn location.", this);
            return false;
        }

        for (int i = 0; i < spawnLocations.Length; i++)
        {
            if (spawnLocations[i] == null)
            {
                Debug.LogError($"[LevelSpawner] Spawn location {i} is not assigned.", this);
                return false;
            }
        }

        for (int i = waveIndex; i < waves.Count; i++)
        {
            EnemyWave wave = waves[i];
            if (wave == null || wave.Enemies == null || wave.Enemies.Count == 0)
            {
                Debug.LogError($"[LevelSpawner] Wave {i + 1} has no enemy entries.", this);
                return false;
            }

            for (int j = 0; j < wave.Enemies.Count; j++)
            {
                EnemyWaveEntry entry = wave.Enemies[j];
                if (entry == null || entry.Factory == null || entry.Count < 0)
                {
                    Debug.LogError($"[LevelSpawner] Wave {i + 1}, entry {j + 1} has invalid configuration.", this);
                    return false;
                }
            }
        }

        return true;
    }

    private IEnumerator SpawnWaves()
    {
        while (waveIndex < waves.Count)
        {
            EnemyWave wave = waves[waveIndex];
            Debug.Log($"[LevelSpawner] Starting {wave.WaveName} ({waveIndex + 1}/{waves.Count}).", this);

            while (entryIndex < wave.Enemies.Count)
            {
                EnemyWaveEntry entry = wave.Enemies[entryIndex];
                while (spawnedFromEntry < entry.Count)
                {
                    SpawnEnemy(entry.Factory);
                    spawnedFromEntry++;
                    if (spawnedFromEntry < entry.Count && delayBetweenSpawns > 0f)
                        yield return new WaitForSeconds(delayBetweenSpawns);
                }

                entryIndex++;
                spawnedFromEntry = 0;
            }

            while (activeEnemies.Count > 0)
                yield return null;

            Debug.Log($"[LevelSpawner] {wave.WaveName} complete.", this);
            waveIndex++;
            entryIndex = 0;
            spawnedFromEntry = 0;

            if (waveIndex < waves.Count && delayBetweenWaves > 0f)
                yield return new WaitForSeconds(delayBetweenWaves);
        }

        Debug.Log("[LevelSpawner] All waves complete.", this);
        spawnRoutine = null;
    }

    private void SpawnEnemy(EnemyFactory factory)
    {
        Transform spawnLocation = spawnLocations[nextSpawnLocation];
        
        GameObject instance = factory.CreateEnemy(spawnLocation.position, spawnLocation.rotation);
        if (instance == null)
            return;

        EnemyBase enemy = instance.GetComponent<EnemyBase>();
        if (enemy == null)
        {
            Debug.LogError($"[LevelSpawner] Factory {factory.name} spawned an object without an EnemyBase.", this);
            Destroy(instance);
            return;
        }

        activeEnemies.Add(enemy);
        enemy.ReturnedToPool += HandleEnemyReturnedToPool;
        nextSpawnLocation = (nextSpawnLocation + 1) % spawnLocations.Length;
    }

    // OBSERVER: the spawner subscribes to each enemy's ReturnedToPool event to know when a wave is
    // cleared. The enemy never references the spawner. Always unsubscribe when done.
    private void HandleEnemyReturnedToPool(EnemyBase enemy)
    {
        enemy.ReturnedToPool -= HandleEnemyReturnedToPool;
        activeEnemies.Remove(enemy);
    }
}
