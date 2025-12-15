using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Phase2 完全版: BSPで部屋を作り、必ず接続（掘削）するダンジョン生成器
/// - 生成した rooms, startRoom, endRoom, mapData を公開
/// - ElementGenerator.GenerateFromMap(mapData, settings, rooms, startRoom, endRoom) を呼ぶ
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    public bool showMarkers = false;
    private List<Vector2Int> floorList = new List<Vector2Int>();
    public IReadOnlyList<Vector2Int> FloorList => floorList;

    [Header("マップ基本設定")]
    public int width = 80;
    public int height = 60;
    public float cellSize = 1f;

    [Header("BSP設定")]
    public int minRoomSize = 6;
    public int maxRoomSize = 16;
    public int maxDepth = 5;

    [Header("可視化プレハブ (任意)")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public Transform stageParent;

    [Header("連携")]
    public ElementGenerator elementGenerator;
    public DungeonSettings settings; // 既存の設定オブジェクト（Inspectorで割当て）

    // map: 0=floor, 1=wall
    private int[,] mapData;
    private List<Room> rooms = new List<Room>();

    // 入口/出口（部屋）
    public Room startRoom { get; private set; }
    public Room endRoom { get; private set; }

    // 公開アクセス
    public int[,] MapData => mapData;
    public IReadOnlyList<Room> Rooms => rooms;
    public int MapWidth => width;
    public int MapHeight => height;
    public float CellSize => cellSize;

    IEnumerator Start()
    {
        // small delay to let inspector values settle when pressing Play
        yield return null;

        if (settings == null)
        {
            Debug.LogWarning("[DungeonGenerator] DungeonSettings が Inspector に割当てられていません。既定値で生成します。");
        }

        GenerateNow();
    }

    [ContextMenu("Generate Now")]
    public void GenerateNow()
    {
        // safety fix
        if (minRoomSize < 3) minRoomSize = 3;
        if (maxRoomSize < minRoomSize) maxRoomSize = minRoomSize + 1;
        if (width < 10 || height < 10) Debug.LogWarning("[DungeonGenerator] 小さいマップサイズが設定されています。推奨は >= 20");

        // init
        mapData = new int[height, width];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                mapData[y, x] = 1; // wall

        rooms.Clear();
        startRoom = null;
        endRoom = null;

        // BSP start
        SplitArea(new RectInt(1, 1, width - 2, height - 2), 0);

        // connect rooms (ensures connectivity)
        ConnectRoomsByTree();

        // Decide start/end rooms (choose two rooms far apart if possible)
        AssignStartAndEnd();

        Debug.Log($"[DungeonGenerator] 生成完了: rooms={rooms.Count} map={width}x{height}");

        // =====================================================
        // 【修正 A】 VisualizeMap の自動実行を ElementGenerator に応じて抑制
        // =====================================================
        bool shouldVisualizeHere =
            elementGenerator == null ||
            (elementGenerator.floorPrefab == null && elementGenerator.wallPrefab == null);

        if (shouldVisualizeHere)
        {
            if (floorPrefab != null && wallPrefab != null)
            {
                Debug.Log("[DungeonGenerator] Visualizing map here (ElementGenerator inactive).");
                VisualizeMap();
            }
            else
            {
                Debug.LogWarning("[DungeonGenerator] floorPrefab or wallPrefab not assigned.");
            }
        }
        else
        {
            Debug.Log("[DungeonGenerator] Skipping VisualizeMap because ElementGenerator will place tiles.");
        }

        // ---- create floorList BEFORE notifying ElementGenerator ----
        floorList.Clear();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (mapData[y, x] == 0)
                    floorList.Add(new Vector2Int(x, y));
            }
        }
        Debug.Log("[DungeonGenerator] floorList count = " + floorList.Count);

        // Notify ElementGenerator (now floorList is ready)
        NotifyElementGenerator();
    }

    // -----------------------
    // BSP（領域分割） -> 部屋生成
    // -----------------------
    private void SplitArea(RectInt area, int depth)
    {
        if (depth >= maxDepth || area.width < minRoomSize * 2 || area.height < minRoomSize * 2)
        {
            CreateRoomInArea(area);
            return;
        }

        // decide split direction
        bool splitVert = area.width >= area.height;
        if (Random.value > 0.5f) splitVert = !splitVert;

        if (splitVert)
        {
            int min = area.x + minRoomSize;
            int max = area.x + area.width - minRoomSize;
            if (max <= min)
            {
                CreateRoomInArea(area);
                return;
            }
            int splitX = Random.Range(min, max);
            RectInt left = new RectInt(area.x, area.y, splitX - area.x, area.height);
            RectInt right = new RectInt(splitX, area.y, area.x + area.width - splitX, area.height);
            SplitArea(left, depth + 1);
            SplitArea(right, depth + 1);
        }
        else
        {
            int min = area.y + minRoomSize;
            int max = area.y + area.height - minRoomSize;
            if (max <= min)
            {
                CreateRoomInArea(area);
                return;
            }
            int splitY = Random.Range(min, max);
            RectInt bottom = new RectInt(area.x, area.y, area.width, splitY - area.y);
            RectInt top = new RectInt(area.x, splitY, area.width, area.y + area.height - splitY);
            SplitArea(bottom, depth + 1);
            SplitArea(top, depth + 1);
        }
    }

    private void CreateRoomInArea(RectInt area)
    {
        int roomW = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, area.width) + 1);
        int roomH = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, area.height) + 1);
        int roomX = Random.Range(area.x, area.x + area.width - roomW + 1);
        int roomY = Random.Range(area.y, area.y + area.height - roomH + 1);

        Room r = new Room(roomX, roomY, roomW, roomH);
        rooms.Add(r);

        // dig room
        for (int y = r.y; y < r.y + r.height; y++)
            for (int x = r.x; x < r.x + r.width; x++)
                if (IsInBounds(x, y))
                    mapData[y, x] = 0;
    }

    // -----------------------
    // 接続ロジック（全部屋を確実に接続）
    // -----------------------
    private void ConnectRoomsByTree()
    {
        if (rooms.Count < 2) return;

        var connected = new HashSet<int>();
        var remaining = new HashSet<int>();
        for (int i = 0; i < rooms.Count; i++) remaining.Add(i);

        connected.Add(0);
        remaining.Remove(0);

        while (remaining.Count > 0)
        {
            int bestFrom = -1;
            int bestTo = -1;
            float bestDist = float.MaxValue;

            foreach (int c in connected)
            {
                foreach (int r in remaining)
                {
                    float d = Vector2Int.Distance(rooms[c].Center, rooms[r].Center);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestFrom = c;
                        bestTo = r;
                    }
                }
            }

            if (bestFrom >= 0 && bestTo >= 0)
            {
                CreateCorridorBetweenPoints(rooms[bestFrom].Center, rooms[bestTo].Center);
                connected.Add(bestTo);
                remaining.Remove(bestTo);
            }
            else
            {
                var en = new System.Collections.Generic.List<int>(remaining);
                int r = en[0];
                CreateCorridorBetweenPoints(rooms[0].Center, rooms[r].Center);
                connected.Add(r);
                remaining.Remove(r);
            }
        }

        int extras = Mathf.Max(1, rooms.Count / 6);
        for (int i = 0; i < extras; i++)
        {
            int a = Random.Range(0, rooms.Count);
            int b = Random.Range(0, rooms.Count);
            if (a != b) CreateCorridorBetweenPoints(rooms[a].Center, rooms[b].Center);
        }
    }

    private void CreateCorridorBetweenPoints(Vector2Int a, Vector2Int b)
    {
        if (Random.value < 0.5f)
        {
            int y = a.y;
            for (int x = Mathf.Min(a.x, b.x); x <= Mathf.Max(a.x, b.x); x++)
                if (IsInBounds(x, y)) mapData[y, x] = 0;

            int xEnd = b.x;
            for (int y2 = Mathf.Min(a.y, b.y); y2 <= Mathf.Max(a.y, b.y); y2++)
                if (IsInBounds(xEnd, y2)) mapData[y2, xEnd] = 0;
        }
        else
        {
            int x = a.x;
            for (int y = Mathf.Min(a.y, b.y); y <= Mathf.Max(a.y, b.y); y++)
                if (IsInBounds(x, y)) mapData[y, x] = 0;

            int yEnd = b.y;
            for (int x2 = Mathf.Min(a.x, b.x); x2 <= Mathf.Max(a.x, b.x); x2++)
                if (IsInBounds(x2, yEnd)) mapData[yEnd, x2] = 0;
        }
    }

    private bool IsInBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    // -----------------------
    // start/end room assignment
    // -----------------------
    private void AssignStartAndEnd()
    {
        if (rooms.Count == 0) return;

        int bestA = 0;
        int bestB = 0;
        float bestDist = -1f;
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                float d = Vector2Int.Distance(rooms[i].Center, rooms[j].Center);
                if (d > bestDist)
                {
                    bestDist = d;
                    bestA = i;
                    bestB = j;
                }
            }
        }

        startRoom = rooms[bestA];
        endRoom = rooms[bestB];
    }

    // -----------------------
    // Visualize
    // -----------------------
    [ContextMenu("Visualize Map")]
    public void VisualizeMap()
    {
        if (floorPrefab == null || wallPrefab == null)
        {
            Debug.LogWarning("[DungeonGenerator] floorPrefab / wallPrefab が未設定です。可視化をスキップします。");
            return;
        }

        if (stageParent == null)
        {
            GameObject go = GameObject.Find("StageParent");
            if (go == null) go = new GameObject("StageParent");
            stageParent = go.transform;
        }

#if UNITY_EDITOR
        for (int i = stageParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(stageParent.GetChild(i).gameObject);
#else
        for (int i = stageParent.childCount - 1; i >= 0; i--)
            Destroy(stageParent.GetChild(i).gameObject);
#endif

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject prefab = (mapData[y, x] == 0) ? floorPrefab : wallPrefab;
                Vector3 pos = new Vector3(x * cellSize, 0f, y * cellSize);
                GameObject obj = Instantiate(prefab, pos, Quaternion.identity, stageParent);
                obj.name = (mapData[y, x] == 0) ? $"Floor_{x}_{y}" : $"Wall_{x}_{y}";
            }
        }

        if (showMarkers && startRoom != null)
        {
            Vector3 s = new Vector3(startRoom.Center.x * cellSize, 0.1f, startRoom.Center.y * cellSize);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.transform.position = s;
            marker.transform.localScale = Vector3.one * cellSize * 0.8f;
            marker.GetComponent<Renderer>().material.color = Color.green;
            marker.name = "StartMarker";
            marker.transform.SetParent(stageParent, true);
        }

        if (showMarkers && endRoom != null)
        {
            Vector3 e = new Vector3(endRoom.Center.x * cellSize, 0.1f, endRoom.Center.y * cellSize);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.transform.position = e;
            marker.transform.localScale = Vector3.one * cellSize * 0.8f;
            marker.GetComponent<Renderer>().material.color = Color.red;
            marker.name = "EndMarker";
            marker.transform.SetParent(stageParent, true);
        }
    }

    // -----------------------
    // Notify ElementGenerator
    // -----------------------
    private void NotifyElementGenerator()
    {
        if (elementGenerator == null)
            elementGenerator = FindObjectOfType<ElementGenerator>();

        if (elementGenerator == null)
        {
            Debug.LogWarning("[DungeonGenerator] ElementGenerator が見つかりません。要素配置をスキップします。");
            return;
        }

        DungeonSettings s = settings;
        if (s == null)
        {
            s = new DungeonSettings();
            s.ApplyRouteSettings(GameManager.Instance.CurrentRoute);
        }

        elementGenerator.GenerateFromMap(mapData, s, rooms, startRoom, endRoom, floorList);
    }

    public void GameOveaaaaaa()
    {
        GameManager.Instance.GameOvedUI();
        TurnManager.Instance.allUnits.Clear();
        GenerateNow();
        StartCoroutine(GameManager.Instance.SpawnPlayerAfterGenerate());
    }
}

[System.Serializable]
public class Room
{
    public int x, y, width, height;
    public Room(int x, int y, int width, int height) { this.x = x; this.y = y; this.width = width; this.height = height; }
    public Vector2Int Center => new Vector2Int(x + width / 2, y + height / 2);
}
