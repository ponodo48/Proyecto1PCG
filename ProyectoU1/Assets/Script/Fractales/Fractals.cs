using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class DiamondSquareTerrain : MonoBehaviour
{
    [SerializeField] int size = 129; // debe ser 2^n + 1
    [SerializeField, Range(0.1f, 10f)] float heightScale = 5f;
    [SerializeField] float roughness = 0.5f;
    [SerializeField] float cellSize = 1f;
    [SerializeField] float baseHeight = 0f; // altura base del plano

    public float Size { get => size; set => size = (int)ClosestDiamondSquareSize((int)value); }
    public float HeightScale { get => heightScale; set => heightScale = Mathf.Clamp(value, 0.1f, 10f); }
    public float Roughness { get => roughness; set => roughness = Mathf.Clamp(value, 0.1f, 1f); }

    public float CellSize { get => cellSize; set => cellSize = Mathf.Clamp(value, 0.1f, 10f); }
    public float BaseHeight { get => baseHeight; set => baseHeight = Mathf.Clamp(value, 0.1f, 10f); }
    private Mesh mesh;
    private float ClosestDiamondSquareSize(int desired)
    {
        return Mathf.Pow(2, desired) + 1;
    }
    void Start()
    {
        GenerateTerrain();
    }

  

    void GenerateTerrain()
    {
        // 1. Generar heightmap con Diamond-Square
        float[,] heightMap = DiamondSquare(size, roughness);

        // 2. Generar mesh sobre plano base
        mesh = GenerateMesh(heightMap);

        // 3. Asignar a componentes
        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        var meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
        
    }

    float[,] DiamondSquare(int size, float roughness)
    {
        float[,] map = new float[size, size];
        int step = size - 1;
        float scale = roughness;

        // Inicializar esquinas con valores aleatorios
        map[0, 0] = Random.value;
        map[0, step] = Random.value;
        map[step, 0] = Random.value;
        map[step, step] = Random.value;

        while (step > 1)
        {
            int half = step / 2;

            // Diamond step: calcular puntos centrales de los cuadrados
            for (int y = half; y < size; y += step)
            {
                for (int x = half; x < size; x += step)
                {
                    float avg = (map[x - half, y - half] +
                                 map[x - half, y + half] +
                                 map[x + half, y - half] +
                                 map[x + half, y + half]) * 0.25f;

                    map[x, y] = avg + (Random.value * 2f - 1f) * scale;
                }
            }

            // Square step: calcular puntos medios de los diamantes
            for (int y = 0; y < size; y += half)
            {
                for (int x = (y + half) % step; x < size; x += step)
                {
                    float sum = 0f;
                    int count = 0;

                    // Sumar vecinos válidos
                    if (x - half >= 0) { sum += map[x - half, y]; count++; }
                    if (x + half < size) { sum += map[x + half, y]; count++; }
                    if (y - half >= 0) { sum += map[x, y - half]; count++; }
                    if (y + half < size) { sum += map[x, y + half]; count++; }

                    map[x, y] = sum / count + (Random.value * 2f - 1f) * scale;
                }
            }

            step /= 2;
            scale *= roughness;
        }

        return map;
    }

    Mesh GenerateMesh(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        int vertexCount = width * height;

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];
        Vector2[] uv = new Vector2[vertexCount];

        // Generar vértices sobre el plano base
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int vertexIndex = y * width + x;

                // Posición: plano base + altura del heightmap
                float worldX = (x - width * 0.5f) * cellSize;
                float worldZ = (y - height * 0.5f) * cellSize;
                float worldY = baseHeight + heightMap[x, y] * heightScale;

                vertices[vertexIndex] = new Vector3(worldX, worldY, worldZ);
                uv[vertexIndex] = new Vector2((float)x / (width - 1), (float)y / (height - 1));
            }
        }

        // Generar triángulos
        int triangleIndex = 0;
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int bottomLeft = y * width + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + width;
                int topRight = topLeft + 1;

                // Primer triángulo (bottom-left, top-left, bottom-right)
                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = bottomRight;

                // Segundo triángulo (bottom-right, top-left, top-right)
                triangles[triangleIndex++] = bottomRight;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = topRight;
            }
        }

        // Crear y configurar el mesh
        Mesh newMesh = new Mesh();
        newMesh.name = "Diamond-Square Terrain";
        newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Para soportar más de 65k vértices
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.uv = uv;
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        return newMesh;
    }

    // Método público para regenerar el terreno (útil para testing)
    [ContextMenu("Regenerate Terrain")]
    public void RegenerateTerrain()
    {
        GenerateTerrain();
    }
}
