using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private TowerManager towerManager;

    public event Action<int> OnEnemySpawned;
    private int totalEnemiesSpawned;

    public void SpawnEnemyOnPath(IReadOnlyList<Vector2Int> path, GameObject enemyPrefab, float spawnJitterRadius = 0f)
    {
        List<Vector3> worldWaypoints = new List<Vector3>();

        foreach (var point in path)
        {
            float worldX = point.x * mapGenerator.CellSize;
            float worldZ = point.y * mapGenerator.CellSize;
            float worldY = mapGenerator.GetTerrainHeight(worldX, worldZ);

            Vector3 localPos = new Vector3(worldX, worldY, worldZ);
            worldWaypoints.Add(mapGenerator.transform.TransformPoint(localPos));
        }

        Vector3 spawnPosition = worldWaypoints[0];

        if (spawnJitterRadius > 0f)
        {
            // scatter cluster members a bit so they don't spawn stacked on top of each other,
            // then re-sample the terrain height at the offset spot
            Vector2 jitter = UnityEngine.Random.insideUnitCircle * spawnJitterRadius;
            Vector3 jitteredWorld = spawnPosition + new Vector3(jitter.x, 0f, jitter.y);
            Vector3 localJittered = mapGenerator.transform.InverseTransformPoint(jitteredWorld);
            float jitteredY = mapGenerator.GetTerrainHeight(localJittered.x, localJittered.z);
            spawnPosition = mapGenerator.transform.TransformPoint(new Vector3(localJittered.x, jitteredY, localJittered.z));
        }

        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemyObject.GetComponent<Enemy>().Initialize(worldWaypoints, towerManager.TowerTarget);

        totalEnemiesSpawned++;
        OnEnemySpawned?.Invoke(totalEnemiesSpawned);
    }
}