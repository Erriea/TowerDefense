using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private TowerManager towerManager;

    public event Action<int> OnEnemySpawned;
    private int totalEnemiesSpawned;

    public void SpawnEnemyOnPath(IReadOnlyList<Vector2Int> path, GameObject enemyPrefab)
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

        GameObject enemyObject = Instantiate(enemyPrefab, worldWaypoints[0], Quaternion.identity);
        enemyObject.GetComponent<Enemy>().Initialize(worldWaypoints, towerManager.TowerTarget);

        totalEnemiesSpawned++;
        OnEnemySpawned?.Invoke(totalEnemiesSpawned);
    }
}