using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class BSPDungeonMesh : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [SerializeField] private int dungeonWidth = 40;
    [SerializeField] private int dungeonHeight = 40;
    [SerializeField] private int minRoomSize = 6;
    [SerializeField] private int maxRoomSize = 12;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float wallHeight = 3f;
    [SerializeField] private float floorHeight = 0f;

    [Header("Materials")]
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material wallMaterial;

    // Properties para UI
    public float DungeonWidth { get => dungeonWidth; set => dungeonWidth = (int)Mathf.Clamp(value, 20, 100); }
    public float DungeonHeight { get => dungeonHeight; set => dungeonHeight = (int)Mathf.Clamp(value, 20, 100); }
    public float MinRoomSize { get => minRoomSize; set => minRoomSize = (int)Mathf.Clamp(value, 4, 8); }
    public float MaxRoomSize { get => maxRoomSize; set => maxRoomSize = (int)Mathf.Clamp(value, 8, 16); }
    public float TileSize { get => tileSize; set => tileSize = Mathf.Clamp(value, 0.1f, 16); }
    public float WallHeight { get => wallHeight; set => wallHeight = Mathf.Clamp(value, 0.1f, 16); }
    public float FloorHeight { get => floorHeight; set => floorHeight = Mathf.Clamp(value,0.1f, 16); }
    public void SetDungeonWidth(float value)
    {
        DungeonWidth = Mathf.RoundToInt(value);
    }

    // Datos
    private int[,] dungeon; // 0 = wall, 1 = floor
    private List<RoomData> rooms;
    private Mesh dungeonMesh;

    [System.Serializable]
    public class RoomData
    {
        public int x, y, width, height;
        public Vector2Int center;
        public RoomData(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.center = new Vector2Int(x + width / 2, y + height / 2);
        }
    }

    void Start()
    {
        if (Application.isPlaying)
            GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        try
        {
            dungeon = new int[dungeonWidth, dungeonHeight];
            rooms = new List<RoomData>();

            FillWithWalls();
            BSPGenerate(0, 0, dungeonWidth, dungeonHeight, 0);
            ConnectRooms();

            dungeonMesh = GenerateDungeonMesh();
            ApplyMesh();

            Debug.Log($"Dungeon generated: {rooms.Count} rooms");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error generating dungeon: {e.Message}");
        }
    }

    void FillWithWalls()
    {
        for (int x = 0; x < dungeonWidth; x++)
            for (int y = 0; y < dungeonHeight; y++)
                dungeon[x, y] = 0;
    }

    void BSPGenerate(int x, int y, int width, int height, int depth)
    {
        if (depth >= 4 || width < minRoomSize * 2 || height < minRoomSize * 2)
        {
            CreateRoom(x, y, width, height);
            return;
        }

        bool divideVertical = Random.Range(0, 2) == 0;
        if (divideVertical && width >= minRoomSize * 2)
        {
            int split = Random.Range(minRoomSize, width - minRoomSize);
            BSPGenerate(x, y, split, height, depth + 1);
            BSPGenerate(x + split, y, width - split, height, depth + 1);
        }
        else if (!divideVertical && height >= minRoomSize * 2)
        {
            int split = Random.Range(minRoomSize, height - minRoomSize);
            BSPGenerate(x, y, width, split, depth + 1);
            BSPGenerate(x, y + split, width, height - split, depth + 1);
        }
        else
        {
            CreateRoom(x, y, width, height);
        }
    }

    void CreateRoom(int areaX, int areaY, int areaWidth, int areaHeight)
    {
        int roomWidth = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, areaWidth - 1));
        int roomHeight = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, areaHeight - 1));

        int roomX = areaX + Random.Range(0, Mathf.Max(1, areaWidth - roomWidth));
        int roomY = areaY + Random.Range(0, Mathf.Max(1, areaHeight - roomHeight));

        for (int x = roomX; x < roomX + roomWidth; x++)
            for (int y = roomY; y < roomY + roomHeight; y++)
                if (IsValidPosition(x, y)) dungeon[x, y] = 1;

        rooms.Add(new RoomData(roomX, roomY, roomWidth, roomHeight));
    }

    void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
            CreateCorridor(rooms[i].center, rooms[i + 1].center);
    }

    void CreateCorridor(Vector2Int from, Vector2Int to)
    {
        int cx = from.x;
        int cy = from.y;
        while (cx != to.x)
        {
            dungeon[cx, cy] = 1;
            cx += (cx < to.x) ? 1 : -1;
        }
        while (cy != to.y)
        {
            dungeon[cx, cy] = 1;
            cy += (cy < to.y) ? 1 : -1;
        }
    }

    bool IsValidPosition(int x, int y) => x >= 0 && x < dungeonWidth && y >= 0 && y < dungeonHeight;

    // Helpers world coords
    float WX(int x) => (x - dungeonWidth * 0.5f) * tileSize;
    float WZ(int y) => (y - dungeonHeight * 0.5f) * tileSize;

    Mesh GenerateDungeonMesh()
    {
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var floorTriangles = new List<int>();
        var wallTriangles = new List<int>();

        // ====== FLOORS (un quad por celda de piso) ======
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (dungeon[x, y] != 1) continue;

                float wx = WX(x);
                float wz = WZ(y);
                AddFloorQuad(vertices, floorTriangles, uvs, wx, wz);
            }
        }

        // ====== WALLS como tiras extendidas y coherentes ======
        // Norte (entre celda [x,y] piso y [x,y+1] muro), fusiona a lo largo de X
        for (int y = 0; y < dungeonHeight; y++)
        {
            int x = 0;
            while (x < dungeonWidth)
            {
                bool isEdge = (dungeon[x, y] == 1) && (y + 1 >= dungeonHeight || dungeon[x, y + 1] == 0);
                if (!isEdge) { x++; continue; }

                int startX = x;
                while (x < dungeonWidth && (dungeon[x, y] == 1) && (y + 1 >= dungeonHeight || dungeon[x, y + 1] == 0))
                    x++;
                int endX = x; // exclusivo

                float baseX = WX(startX);
                float baseZ = WZ(y) + tileSize; // borde norte
                float length = (endX - startX) * tileSize;

                // Plano vertical: se extiende a lo largo de +X, en z fijo (baseZ)
                AddWallStrip(vertices, wallTriangles, uvs,
                    new Vector3(baseX, floorHeight, baseZ),
                    Vector3.right * length);
            }
        }

        // Sur (entre [x,y] piso y [x,y-1] muro), fusiona a lo largo de X
        for (int y = 0; y < dungeonHeight; y++)
        {
            int x = 0;
            while (x < dungeonWidth)
            {
                bool isEdge = (dungeon[x, y] == 1) && (y - 1 < 0 || dungeon[x, y - 1] == 0);
                if (!isEdge) { x++; continue; }

                int startX = x;
                while (x < dungeonWidth && (dungeon[x, y] == 1) && (y - 1 < 0 || dungeon[x, y - 1] == 0))
                    x++;
                int endX = x;

                float baseX = WX(startX);
                float baseZ = WZ(y); // borde sur
                float length = (endX - startX) * tileSize;

                AddWallStrip(vertices, wallTriangles, uvs,
                    new Vector3(baseX, floorHeight, baseZ),
                    Vector3.right * length);
            }
        }

        // Este (entre [x,y] piso y [x+1,y] muro), fusiona a lo largo de Z
        for (int x = 0; x < dungeonWidth; x++)
        {
            int y = 0;
            while (y < dungeonHeight)
            {
                bool isEdge = (dungeon[x, y] == 1) && (x + 1 >= dungeonWidth || dungeon[x + 1, y] == 0);
                if (!isEdge) { y++; continue; }

                int startY = y;
                while (y < dungeonHeight && (dungeon[x, y] == 1) && (x + 1 >= dungeonWidth || dungeon[x + 1, y] == 0))
                    y++;
                int endY = y;

                float baseX = WX(x) + tileSize; // borde este
                float baseZ = WZ(startY);
                float length = (endY - startY) * tileSize;

                // Plano vertical: se extiende a lo largo de +Z, en x fijo (baseX)
                AddWallStrip(vertices, wallTriangles, uvs,
                    new Vector3(baseX, floorHeight, baseZ),
                    Vector3.forward * length);
            }
        }

        // Oeste (entre [x,y] piso y [x-1,y] muro), fusiona a lo largo de Z
        for (int x = 0; x < dungeonWidth; x++)
        {
            int y = 0;
            while (y < dungeonHeight)
            {
                bool isEdge = (dungeon[x, y] == 1) && (x - 1 < 0 || dungeon[x - 1, y] == 0);
                if (!isEdge) { y++; continue; }

                int startY = y;
                while (y < dungeonHeight && (dungeon[x, y] == 1) && (x - 1 < 0 || dungeon[x - 1, y] == 0))
                    y++;
                int endY = y;

                float baseX = WX(x); // borde oeste
                float baseZ = WZ(startY);
                float length = (endY - startY) * tileSize;

                AddWallStrip(vertices, wallTriangles, uvs,
                    new Vector3(baseX, floorHeight, baseZ),
                    Vector3.forward * length);
            }
        }

        // ==== Construcción del mesh ====
        var mesh = new Mesh();
        mesh.name = "BSP Dungeon (Classic Walls)";
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);

        // Submeshes si hay 2 materiales; si no, un solo submesh
        bool twoMats = (floorMaterial != null && wallMaterial != null);
        if (twoMats)
        {
            mesh.subMeshCount = 2;
            mesh.SetTriangles(floorTriangles, 0);
            mesh.SetTriangles(wallTriangles, 1);
        }
        else
        {
            var all = new List<int>(floorTriangles.Count + wallTriangles.Count);
            all.AddRange(floorTriangles);
            all.AddRange(wallTriangles);
            mesh.subMeshCount = 1;
            mesh.SetTriangles(all, 0);
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    void AddFloorQuad(List<Vector3> v, List<int> t, List<Vector2> u, float x, float z)
    {
        int i = v.Count;

        v.Add(new Vector3(x, floorHeight, z));
        v.Add(new Vector3(x + tileSize, floorHeight, z));
        v.Add(new Vector3(x + tileSize, floorHeight, z + tileSize));
        v.Add(new Vector3(x, floorHeight, z + tileSize));

        // UV por tile (0..1)
        u.Add(new Vector2(0, 0));
        u.Add(new Vector2(1, 0));
        u.Add(new Vector2(1, 1));
        u.Add(new Vector2(0, 1));

        // Triángulos (CCW visto desde arriba)
        t.Add(i); t.Add(i + 2); t.Add(i + 1);
        t.Add(i); t.Add(i + 3); t.Add(i + 2);
    }

    /// <summary>
    /// Agrega una pared plana (doble cara) de altura wallHeight,
    /// que se extiende desde 'basePos' en la dirección 'along' (Vector3 con longitud en mundo).
    /// UVs: u escala con la longitud (tiles), v escala con la altura (en tiles).
    /// </summary>
    void AddWallStrip(List<Vector3> v, List<int> t, List<Vector2> u, Vector3 basePos, Vector3 along)
    {
        int i = v.Count;

        Vector3 up = Vector3.up * wallHeight;
        Vector3 right = along;

        Vector3 p0 = basePos;
        Vector3 p1 = basePos + up;
        Vector3 p2 = basePos + up + right;
        Vector3 p3 = basePos + right;

        v.Add(p0); // i
        v.Add(p1); // i+1
        v.Add(p2); // i+2
        v.Add(p3); // i+3

        float uLen = right.magnitude / tileSize;
        float vLen = wallHeight / tileSize;

        u.Add(new Vector2(0, 0));
        u.Add(new Vector2(0, vLen));
        u.Add(new Vector2(uLen, vLen));
        u.Add(new Vector2(uLen, 0));

        // Frente (normal hacia afuera)
        t.Add(i); t.Add(i + 1); t.Add(i + 2);
        t.Add(i); t.Add(i + 2); t.Add(i + 3);

        // Dorso (normal hacia adentro)
        int j = v.Count;
        v.Add(p0); v.Add(p1); v.Add(p2); v.Add(p3);
        u.Add(new Vector2(0, 0));
        u.Add(new Vector2(0, vLen));
        u.Add(new Vector2(uLen, vLen));
        u.Add(new Vector2(uLen, 0));

        t.Add(j); t.Add(j + 2); t.Add(j + 1);
        t.Add(j); t.Add(j + 3); t.Add(j + 2);
    }



    void ApplyMesh()
    {
        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        var mc = GetComponent<MeshCollider>();

        mf.sharedMesh = dungeonMesh;

        if (floorMaterial != null && wallMaterial != null)
            mr.sharedMaterials = new Material[] { floorMaterial, wallMaterial };
        else if (floorMaterial != null)
            mr.sharedMaterial = floorMaterial;
        else if (wallMaterial != null)
            mr.sharedMaterial = wallMaterial;

        mc.sharedMesh = null;
        mc.sharedMesh = dungeonMesh;
    }

    public void RegenerateFromUI()
    {
        if (Application.isPlaying) GenerateDungeon();
    }

    void OnDrawGizmosSelected()
    {
        if (rooms == null) return;
        Gizmos.color = Color.green;
        foreach (RoomData room in rooms)
        {
            Vector3 center = new Vector3(
                (room.x + room.width * 0.5f - dungeonWidth * 0.5f) * tileSize,
                wallHeight * 0.5f,
                (room.y + room.height * 0.5f - dungeonHeight * 0.5f) * tileSize
            );
            Vector3 size = new Vector3(room.width * tileSize, wallHeight, room.height * tileSize);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
