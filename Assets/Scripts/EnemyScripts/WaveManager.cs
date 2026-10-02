using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWaveEntry
{
    public GameObject enemyPrefab;
    public int unlockWave = 1;      // first wave this type is allowed to appear in
    public float spawnWeight = 1f;  // relative likelihood once unlocked
}

public class WaveManager : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private TowerManager towerManager;

    [SerializeField] private List<EnemyWaveEntry> enemyTypes;

    [SerializeField] private int baseEnemiesPerWave = 3;
    [SerializeField] private int enemiesPerWaveGrowth = 1;
    [SerializeField] private float baseSpawnInterval = 1.2f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float difficultyAdjustRate = 0.15f;
    [SerializeField] private float maxSpawnDistanceFromTower = 80f;

    public event Action<int> OnWaveStarted;
    public event Action<int> OnEnemiesRemainingChanged;

    private int currentWave;
    private int enemiesRemaining;
    private int pathIndex;
    private float difficultyMultiplier = 1f;
    private float towerHealthAtWaveStart;

    private void OnEnable()
    {
        Enemy.OnAnyEnemyDespawned += HandleEnemyDespawned;
    }

    private void OnDisable()
    {
        Enemy.OnAnyEnemyDespawned -= HandleEnemyDespawned;
    }

    private void HandleEnemyDespawned()
    {
        enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
        OnEnemiesRemainingChanged?.Invoke(enemiesRemaining);
    }

    public void BeginWaves()
    {
        currentWave = 0;
        enemiesRemaining = 0;
        difficultyMultiplier = 1f;
        OnEnemiesRemainingChanged?.Invoke(enemiesRemaining);
        StopAllCoroutines();
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        while (true)
        {
            currentWave++;
            OnWaveStarted?.Invoke(currentWave);

            towerHealthAtWaveStart = towerManager.TowerTarget is Tower t ? t.CurrentHealth : 0f;

            int enemyCount = Mathf.RoundToInt((baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveGrowth) * difficultyMultiplier);
            float spawnInterval = Mathf.Max(0.3f, baseSpawnInterval - currentWave * 0.03f);

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnNextEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(timeBetweenWaves);

            AdjustDifficultyForNextWave();
        }
    }

    private void SpawnNextEnemy()
    {
        IReadOnlyList<Vector2Int> path = mapGenerator.Paths[pathIndex];
        pathIndex = (pathIndex + 1) % mapGenerator.Paths.Count;

        GameObject prefab = ChooseEnemyType();
        if (prefab != null)
        {
            List<Vector2Int> spawnPath = TrimPathToDistance(path, maxSpawnDistanceFromTower);
            monsterSpawner.SpawnEnemyOnPath(spawnPath, prefab);
            enemiesRemaining++;
            OnEnemiesRemainingChanged?.Invoke(enemiesRemaining);
        }
    }

    // Returns only the last stretch of the path (closest to the tower), up to maxDistance
    // world units long, so enemies spawn near where the camera can actually see them instead
    // of at the true far end of a long procedurally-generated path.
    private List<Vector2Int> TrimPathToDistance(IReadOnlyList<Vector2Int> fullPath, float maxDistance)
    {
        int startIndex = 0;
        float accumulated = 0f;

        for (int i = fullPath.Count - 1; i > 0; i--)
        {
            float segment = Vector2.Distance(fullPath[i], fullPath[i - 1]);

            if (accumulated + segment > maxDistance)
            {
                startIndex = i;
                break;
            }

            accumulated += segment;
        }

        List<Vector2Int> trimmed = new List<Vector2Int>();
        for (int i = startIndex; i < fullPath.Count; i++)
        {
            trimmed.Add(fullPath[i]);
        }

        return trimmed;
    }

    private GameObject ChooseEnemyType()
    {
        List<EnemyWaveEntry> unlocked = new List<EnemyWaveEntry>();
        float totalWeight = 0f;

        foreach (var entry in enemyTypes)
        {
            if (currentWave >= entry.unlockWave)
            {
                unlocked.Add(entry);
                totalWeight += entry.spawnWeight;
            }
        }

        if (unlocked.Count == 0) return null;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in unlocked)
        {
            cumulative += entry.spawnWeight;
            if (roll <= cumulative)
                return entry.enemyPrefab;
        }

        return unlocked[unlocked.Count - 1].enemyPrefab;
    }

    private void AdjustDifficultyForNextWave()
    {
        if (!(towerManager.TowerTarget is Tower tower)) return;

        float healthLostFraction = tower.MaxHealth > 0
            ? (towerHealthAtWaveStart - tower.CurrentHealth) / tower.MaxHealth
            : 0f;

        if (healthLostFraction < 0.05f)
            difficultyMultiplier += difficultyAdjustRate;   // breezed through — ramp up
        else if (healthLostFraction > 0.25f)
            difficultyMultiplier -= difficultyAdjustRate;   // took a beating — ease off

        difficultyMultiplier = Mathf.Clamp(difficultyMultiplier, 0.5f, 3f);
    }
}