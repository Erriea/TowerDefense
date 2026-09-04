using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private float terrainHeight;
    [SerializeField] private float noiseScale;
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    [SerializeField] private AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool generateTerrain = true;
    [SerializeField] private TowerManager towerManager;
    
    [SerializeField] private int heightSteps = 8;
    
    [Header("Paths")]
    [SerializeField] private float pathWidth = 4f;
    [SerializeField] private float pathMeanderFrequency = 0.05f;
    [SerializeField] private float pathSteerStrength = 0.15f;
    [SerializeField] private int minPathCount = 3;
    [SerializeField] private int maxPathCount = 6;
    
    [SerializeField] private bool useRandomSeed = true;
    
    [Header("Terrain Colours")]
    [SerializeField] private string lowColorHex = "#1E88E5"; 
    [SerializeField] private string middleColorHex = "#8D6E63"; 
    [SerializeField] private string highColorHex = "#43A047";
    
    [Header("Random Object Spawning")]
    // arrays containing the prefabs so they can be randomised
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] rockPrefabs;
    
    [SerializeField] private int treeCount = 25;
    [SerializeField] private int rockCount = 20;
    
    [SerializeField] private float environmentPadding = 2f;
    [SerializeField] private float minTreeHeight = 2f;
    [SerializeField] private float maxTreeHeight = 5f;
    [SerializeField] private Transform environmentParent;
    
    public IReadOnlyList<IReadOnlyList<Vector2Int>> Paths => paths;
    [SerializeField] private float defenderPadRadius = 5f;
    [SerializeField] private DefenderPlacementGenerator defenderPlacementGenerator;

    private float[,] heightMap;
    
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    
    

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        if (useRandomSeed)
        {
            seed = System.Environment.TickCount;
        }
    }

    public void Start()
    {
        if (generateTerrain)
        {
            GenerateMap();
        }
    }

    public void GenerateMap()
    {
        GeneratePaths();
        defenderPlacementGenerator.GenerateCandidateGridPoints();
        GenerateHeightMap();
        GenerateMesh();
        
        defenderPlacementGenerator.ResolveWorldPositions();
        
        towerManager.SpawnTower();
        SpawnEnvironment();
        //defenderPlacementGenerator.ShowPlacementMarkers();
    }

    private void GenerateHeightMap()
    {
        heightMap = new float[width, height];
        
        //uses the seed to make a random offset that is different each time
        System.Random random = new System.Random(seed);

        float offsetX = random.Next(-10000, 10000) + offset.x;
        float offsetY = random.Next(-10000, 10000) + offset.y;

        if (noiseScale <= 0)
        {
            noiseScale = 0.001f;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
               float sampleX = (x + offsetX) / noiseScale;
               float sampleY = (y + offsetY) / noiseScale;
               float noise = Mathf.PerlinNoise(sampleX, sampleY);
               //heightMap[x, y] = heightCurve.Evaluate(noise) * terrainHeight;
               float terrainY = heightCurve.Evaluate(noise) * terrainHeight;

               // Make the terrain height stepped/pixelated
               float stepSize = terrainHeight / heightSteps;

               terrainY = Mathf.Round(terrainY / stepSize) * stepSize;

                // Centre of the map
               float centerX = width / 2f;
               float centerZ = height / 2f;

                // Distance from centre
               float distanceFromCenter = Vector2.Distance(
                   new Vector2(x, y),
                   new Vector2(centerX, centerZ)
               );

                // Flatten area around tower
                float towerStrength = CalculateFalloff(distanceFromCenter, towerRadius);

                float pathStrength = 0f;
                foreach (var path in paths)
                {
                    foreach (var point in path)
                    {
                        float distanceToPoint = Vector2.Distance(new Vector2(x, y), new Vector2(point.x, point.y));
                        float strength = CalculateFalloff(distanceToPoint, pathWidth);

                        if (strength > pathStrength)
                        {
                            pathStrength = strength;
                        }
                    }
                }

                float defenderStrength = 0f;
                foreach (var spot in defenderPlacementGenerator.CandidateGridPoints)
                {
                    float distanceToSpot = Vector2.Distance(new Vector2(x, y), spot);
                    float strength = CalculateFalloff(distanceToSpot, defenderPadRadius);

                    if (strength > defenderStrength)
                    {
                        defenderStrength = strength;
                    }
                }

                float flattenStrength = Mathf.Max(towerStrength, Mathf.Max(pathStrength, defenderStrength));
                terrainY = Mathf.Lerp(terrainY, towerHeight, flattenStrength);

               heightMap[x, y] = terrainY;
            }
        }
    }
    
    private void SpawnEnvironment()
    {
        ClearEnvironment();

        System.Random rng = new System.Random(seed);

        SpawnObjects(treePrefabs, treeCount, rng);
        SpawnObjects(rockPrefabs, rockCount, rng);
    }
    
    private void SpawnObjects(GameObject[] prefabs, int count, System.Random rng)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return;
        }

        int spawned = 0;
        int attempts = 0;

        int maxAttempts = count * 20;

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;

            // Pick a random position on the map
            int x = rng.Next(0, width);
            int z = rng.Next(0, height);

            Vector2Int gridPosition = new Vector2Int(x, z);

            // Don't spawn on paths
            if (IsNearPath(gridPosition))
            {
                continue;
            }

            // Don't spawn near the tower
            Vector2 center = new Vector2(width / 2f, height / 2f);

            float distanceFromTower = Vector2.Distance(
                new Vector2(x, z),
                center
            );

            if (distanceFromTower < towerRadius + environmentPadding)
            {
                continue;
            }

            // Get terrain height
            float terrainY = heightMap[x, z];

            // Don't spawn outside allowed height range
            if (terrainY < minTreeHeight || terrainY > maxTreeHeight)
            {
                continue;
            }

            // Choose a random prefab
            GameObject prefab =
                prefabs[rng.Next(0, prefabs.Length)];

            // Convert grid position into world position
            Vector3 spawnPosition = transform.TransformPoint(
                new Vector3(
                    x * cellSize,
                    terrainY,
                    z * cellSize
                )
            );

            // Random rotation
            Quaternion rotation = Quaternion.Euler(
                0f,
                (float)rng.NextDouble() * 360f,
                0f
            );

            GameObject spawnedObject = Instantiate(
                prefab,
                spawnPosition,
                rotation
            );

            // Parent the object
            if (environmentParent != null)
            {
                spawnedObject.transform.SetParent(environmentParent);
            }

            spawned++;
        }
    }
    
    private bool IsNearPath(Vector2Int position)
    {
        foreach (var path in paths)
        {
            foreach (var point in path)
            {
                float distance = Vector2.Distance(
                    position,
                    point
                );

                if (distance < pathWidth + environmentPadding)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private void ClearEnvironment()
    {
        if (environmentParent == null)
        {
            return;
        }

        for (int i = environmentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(environmentParent.GetChild(i).gameObject);
        }
    }
    
    //get places for paths to start
    private Vector2Int GetRandomEdgePoint(int pathIndex, System.Random rng)
    {
        // evenly space each path's starting direction around the full circle
        float baseAngle = (360f / pathCount) * pathIndex;

        // jitter within this path's own slice so it's not perfectly mechanical
        float maxJitter = (180f / pathCount) * 0.5f;
        float jitter = (float)(rng.NextDouble() * 2 - 1) * maxJitter;

        float angleRadians = (baseAngle + jitter) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        Vector2 center = new Vector2(halfWidth, halfHeight);

        // find whether this direction hits a side edge or a top/bottom edge first
        float tX = dir.x != 0 ? halfWidth / Mathf.Abs(dir.x) : float.MaxValue;
        float tY = dir.y != 0 ? halfHeight / Mathf.Abs(dir.y) : float.MaxValue;
        float t = Mathf.Min(tX, tY);

        Vector2 edgePoint = center + dir * t;

        int x = Mathf.Clamp(Mathf.RoundToInt(edgePoint.x), 0, width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(edgePoint.y), 0, height - 1);

        return new Vector2Int(x, y);
    }
    
    private void GeneratePaths()
    {
        paths.Clear();
        System.Random rng = new System.Random(seed);

        pathCount = rng.Next(minPathCount, maxPathCount + 1);

        for (int i = 0; i < pathCount; i++)
        {
            paths.Add(GeneratePath(i, rng));
        }
    }
    
    private float CalculateFalloff(float distance, float radius)
    {
        if (distance >= radius) return 0f;

        float t = distance / radius; // 0 at the centre point, 1 at the radius's edge
        return 1f - Mathf.SmoothStep(0f, 1f, t);
    }
    
    private List<Vector2Int> GeneratePath(int pathIndex, System.Random rng)
    {
        List<Vector2Int> waypoints = new List<Vector2Int>();

        Vector2 current = GetRandomEdgePoint(pathIndex, rng);
        Vector2 center = new Vector2(width / 2f, height / 2f);

        waypoints.Add(new Vector2Int(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y)));

        float noiseOffset = (float)rng.NextDouble() * 1000f;

        // the direction the path is currently heading, carried forward between steps
        Vector2 heading = (center - current).normalized;

        int maxSteps = 1000;
        int steps = 0;

        while (Vector2.Distance(current, center) > towerRadius && steps < maxSteps)
        {
            Vector2 directionToCenter = (center - current).normalized;

            // nudge the heading toward the centre a little each step, rather than
            // snapping fully onto it — this is what lets a bend actually persist
            heading = Vector2.Lerp(heading, directionToCenter, pathSteerStrength).normalized;

            float noiseValue = Mathf.PerlinNoise(noiseOffset + steps * pathMeanderFrequency, 0f);
            float angle = (noiseValue - 0.5f) * 2f * pathWobbleStrength;

            Vector2 stepDirection = RotateVector(heading, angle);
            current += stepDirection * pathStepSize;
            heading = stepDirection;

            waypoints.Add(new Vector2Int(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y)));
            steps++;
        }

        return waypoints;
    }

    private Vector2 RotateVector(Vector2 v, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
    
    private void OnDrawGizmos()
    {
        if (paths == null) return;

        Gizmos.color = Color.red;

        foreach (var path in paths)
        {
            foreach (var point in path)
            {
                Vector3 localPos = new Vector3(point.x * cellSize, terrainHeight, point.y * cellSize);
                Vector3 worldPos = transform.TransformPoint(localPos);
                Gizmos.DrawSphere(worldPos, 0.3f);
            }
        }
    }
    
    private Color HexToColor(string hex) 
    { 
        Color color; 
        if (ColorUtility.TryParseHtmlString(hex, out color)) 
            return color; 
        Debug.LogError("Invalid hex colour: " + hex); return Color.white; 
    }

    private void GenerateMesh()
    {
        int vertexCountX = width + 1;
        int vertexCountZ = height + 1;

        Vector3[] vertices =
            new Vector3[vertexCountX * vertexCountZ];

        Vector2[] uv =
            new Vector2[vertices.Length];
        
        Color[] colors =
            new Color[vertices.Length];

        int[] triangles =
            new int[width * height * 6];
        
        // vertices
        for (int z = 0; z < vertexCountZ; z++)
        {
            for (int x = 0; x < vertexCountX; x++)
            {
                int vertexIndex = z * vertexCountX + x;

                int sampleX = Mathf.Min(x, width - 1);
                int sampleZ = Mathf.Min(z, height - 1);

                float y = heightMap[sampleX, sampleZ];

                vertices[vertexIndex] =
                    new Vector3(
                        x * cellSize,
                        y,
                        z * cellSize
                    );

                uv[vertexIndex] =
                    new Vector2(
                        (float)x / width,
                        (float)z / height
                    );
                
                colors[vertexIndex] = GetTerrainColor(y);
            }
        }
        
        //triangles
        int triangleIndex = 0;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int vertex = z * vertexCountX + x;

                triangles[triangleIndex++] = vertex;
                triangles[triangleIndex++] = vertex + vertexCountX;
                triangles[triangleIndex++] = vertex + 1;

                triangles[triangleIndex++] = vertex + 1;
                triangles[triangleIndex++] = vertex + vertexCountX;
                triangles[triangleIndex++] = vertex + vertexCountX + 1;
            }
        }
        
        // create the mesh
        Mesh mesh = new Mesh();

        mesh.name = "Procedural Terrain";

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.colors = colors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        // Update collision mesh
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }
    
    private Color GetTerrainColor(float height)
    {
        float normalizedHeight = height / terrainHeight; 
        
        Color lowColor = HexToColor(lowColorHex); 
        Color middleColor = HexToColor(middleColorHex); ; 
        Color highColor = HexToColor(highColorHex);

        if (normalizedHeight < 0.2f)
        {
            return lowColor;
        }
        else if (normalizedHeight < 0.4f)
        {
            return middleColor;
        }
        else
        {
            return highColor;
        }
    }

    public float GetTerrainHeight(float worldX, float worldZ)
    {
        int x = Mathf.Clamp(
            Mathf.RoundToInt(worldX / cellSize),
            0,
            width - 1
        );

        int z = Mathf.Clamp(
            Mathf.RoundToInt(worldZ / cellSize),
            0,
            height - 1
        );

        return heightMap[x, z];
    }

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public float[,] HeightMap => heightMap;

    //Tower Spawning
    [SerializeField] private float towerRadius = 8f;
    [SerializeField] private float towerHeight = 3f;
    
    //paths settings
    [SerializeField] private int pathCount = 3;
    [SerializeField] private float pathStepSize = 1f;
    [SerializeField] private float pathWobbleStrength = 0.5f;
    
    private List<List<Vector2Int>> paths = new List<List<Vector2Int>>();
    
   
}


