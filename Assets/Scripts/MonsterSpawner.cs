using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private Transform tower;

    [SerializeField] private int spawnPointsAmount = 4;
    [SerializeField] private float edgeDistance = 1.5f;
    [SerializeField] private float distanceFromTower = 50f;
    
    private List<Vector3> spawnPoints = new List<Vector3>();

    public void GenerateSpawnPoints()
    {
        spawnPoints.Clear();
        
        int attempts = 0;
        int maxAttempts = spawnPointsAmount * 2;

        while (spawnPoints.Count < spawnPointsAmount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 candidate = GetRandomEdgePosition();

            // checks if the position is on the nav mesh to know if the spawn point can be placed there
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 20f, NavMesh.AllAreas))
            {
                continue;
            }

            // this checks that the spawn point spawns away from the tower 
            if (Vector3.Distance(hit.position, tower.position) < distanceFromTower)
            {
                continue;
            }
            
            // checks that there is not another spawn point too close to it
            if (CloseToOtherSP(hit.position))
            {
                continue;
            }
            
            spawnPoints.Add(hit.position);
            
            Debug.Log("Crow spawn point created at" + hit.position);
        }
        
        Debug.Log("Generated" + spawnPoints.Count + " spawn points");
    }

    private Vector3 GetRandomEdgePosition()
    {
        float mapWidth = mapGenerator.Width * mapGenerator.CellSize;
        
        float mapHeight = mapGenerator.Height * mapGenerator.CellSize;
        
        int side = Random.Range(0, 4);

        float x;
        float z;

        switch (side)
        {
            case 0: 
                x = Random.Range(0f, mapWidth);
                z = edgeDistance;
                break;
            case 1:
                x = Random.Range(0f, mapWidth);
                z = mapHeight - edgeDistance;
                break;
            case 2:
                x = edgeDistance;
                z = Random.Range(0f, mapHeight);
                break;
            default:
                x = mapWidth - edgeDistance;
                z = Random.Range(0f, mapHeight);
                break;
        }
        return new Vector3(x, 100f, z);
    }

    private bool CloseToOtherSP(Vector3 point)
    {
        foreach (Vector3 spawnPoint in spawnPoints)
        {
            if (Vector3.Distance(point, spawnPoint) < 15f)
                return true;
        }
        return false;
    }
}
