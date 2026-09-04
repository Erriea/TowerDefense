using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;
    
    [SerializeField] private TowerManager towerManager;
    
    public event Action<int> OnCrowSpawned;
    private int totalCrowsSpawned;

    public void BeginSpawning()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            int pathIndex = Random.Range(0, mapGenerator.Paths.Count);
            SpawnEnemyOnPath(mapGenerator.Paths[pathIndex]);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    /*
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            foreach (var path in mapGenerator.Paths)
            {
                SpawnEnemyOnPath(path);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
    */

    private void SpawnEnemyOnPath(IReadOnlyList<Vector2Int> path)
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
        
        totalCrowsSpawned++;
        OnCrowSpawned?.Invoke(totalCrowsSpawned);
    }
}