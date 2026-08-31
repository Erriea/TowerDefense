using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private float terrainHeight;
    [SerializeField] private float noiseScale;
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    [SerializeField] private AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool generateTerrain = true;

    private float[,] heightMap;
    
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
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
        GenerateHeightMap();
        GenerateMesh();
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
               heightMap[x, y] = heightCurve.Evaluate(noise) * terrainHeight;
            }
        }
    }

    private void GenerateMesh()
    {
        int vertexCountX = width + 1;
        int vertexCountZ = height + 1;

        Vector3[] vertices =
            new Vector3[vertexCountX * vertexCountZ];

        Vector2[] uv =
            new Vector2[vertices.Length];

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
                triangles[triangleIndex++] =
                    vertex + vertexCountX + 1;
            }
        }
        
        // create the mesh
        Mesh mesh = new Mesh();

        mesh.name = "Procedural Terrain";

        mesh.indexFormat =
            UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        // Update collision mesh
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
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

}
    
    // public int mapWidth;
    // public int mapHeight;
    // public float noiseScale;
    //
    // public bool autoUpdate;
    //
    // public void GenerateMap()
    // {
    //     float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);
    //     
    //     MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
    //     mapDisplay.DrawNoiseMap(noiseMap);
    // }
    //


