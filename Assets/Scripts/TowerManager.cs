using UnityEngine;

public class TowerManager : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private GameObject towerPrefab;

    public void SpawnTower()
    {
        // Get the centre of the map
        float x = (mapGenerator.Width * mapGenerator.CellSize) / 2f;
        float z = (mapGenerator.Height * mapGenerator.CellSize) / 2f;

        // Get the terrain height at that position
        float y = mapGenerator.GetTerrainHeight(x, z);

        Vector3 towerPosition = new Vector3(x, y, z);

        // Spawn tower
        Instantiate(
            towerPrefab,
            towerPosition,
            Quaternion.identity
        );
    }
    
    // Vector3 towerPosition =
    //     mapGenerator.GetMapCenter();
    //
    // tower.transform.position =
    // towerPosition;
    //
    // float centerX = width / 2f;
    // float centerZ = height / 2f;
    //
    //     for (int y = 0; y < height; y++)
    // {
    //     for (int x = 0; x < width; x++)
    //     {
    //         float sampleX = (x + offsetX) / noiseScale;
    //         float sampleY = (y + offsetY) / noiseScale;
    //
    //         float noise =
    //             Mathf.PerlinNoise(sampleX, sampleY);
    //
    //         float terrainY =
    //             heightCurve.Evaluate(noise) * terrainHeight;
    //
    //         float distanceFromCenter =
    //             Vector2.Distance(
    //                 new Vector2(x, y),
    //                 new Vector2(centerX, centerZ)
    //             );
    //
    //         if (distanceFromCenter < towerRadius)
    //         {
    //             terrainY = towerHeight;
    //         }
    //
    //         heightMap[x, y] = terrainY;
    //     }
    // }
}
