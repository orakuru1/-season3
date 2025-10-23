using System.Collections.Generic;
using UnityEngine;

#if UNITY_AI_PRESENT
using Unity.AI.Navigation;
#endif

public class ElementGenerator : MonoBehaviour
{
    [Header("Prefab設定")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject treasurePrefab;
    public GameObject goalLightPrefab;

    [Header("ステージ生成設定")]
    public Transform stageParent;
    public float cellSize = 2f;
    public int mapWidth = 40;
    public int mapHeight = 40;

    [Header("ミニマップ設定")]
    public UnityEngine.UI.RawImage minimapUI;
    public int minimapScale = 4;
    public int revealRadius = 3;

    private int[,] mapData;
    private bool[,] explored;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private GameObject playerInstance;
    private Texture2D minimapTexture;

#if UNITY_AI_PRESENT
    private NavMeshSurface surface;
#endif

    void Awake()
    {
        Debug.Log("🧱 ElementGenerator 起動中...");

        ReadResources();
        GenerateDungeonRogueLike();
        BuildStageObjects();

#if UNITY_AI_PRESENT
        SetupNavMeshSurface();
#endif

        InitMinimap();
    }

    // ----------------------------------------------------
    // ✅ リソース確認
    void ReadResources()
    {
        if (!playerPrefab) Debug.LogError("❌ Player プレハブが設定されていません！");
        if (!enemyPrefab) Debug.LogError("❌ Enemy プレハブが設定されていません！");
        if (!treasurePrefab) Debug.LogError("❌ Treasure プレハブが設定されていません！");
        if (!goalLightPrefab) Debug.LogError("❌ Goal_Light プレハブが設定されていません！");
    }

    // ----------------------------------------------------
    // 🏰 迷路＋部屋つきローグ風ダンジョン生成
    void GenerateDungeonRogueLike()
    {
        mapData = new int[mapHeight, mapWidth];
        explored = new bool[mapHeight, mapWidth];

        // 初期化：すべて壁で埋める
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                mapData[y, x] = 1; // 壁

        // 部屋をランダムに作る
        int roomCount = Random.Range(6, 10);
        List<Rect> rooms = new List<Rect>();

        for (int i = 0; i < roomCount; i++)
        {
            int w = Random.Range(4, 8);
            int h = Random.Range(4, 8);
            int x = Random.Range(1, mapWidth - w - 1);
            int y = Random.Range(1, mapHeight - h - 1);
            Rect newRoom = new Rect(x, y, w, h);

            bool overlaps = false;
            foreach (Rect room in rooms)
            {
                if (newRoom.Overlaps(room))
                {
                    overlaps = true;
                    break;
                }
            }
            if (!overlaps)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }

        // 部屋同士を通路で接続
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int prev = GetRoomCenter(rooms[i - 1]);
            Vector2Int current = GetRoomCenter(rooms[i]);
            CarveCorridor(prev, current);
        }

        Debug.Log($"✅ ダンジョン生成完了: 部屋={rooms.Count}, サイズ={mapWidth}x{mapHeight}");
    }

    void CarveRoom(Rect room)
    {
        for (int y = (int)room.yMin; y < (int)room.yMax; y++)
            for (int x = (int)room.xMin; x < (int)room.xMax; x++)
                mapData[y, x] = 0; // 床
    }

    void CarveCorridor(Vector2Int from, Vector2Int to)
    {
        // まず横に、次に縦に掘る（L字通路）
        int x = from.x, y = from.y;
        while (x != to.x)
        {
            mapData[y, x] = 0;
            x += x < to.x ? 1 : -1;
        }
        while (y != to.y)
        {
            mapData[y, x] = 0;
            y += y < to.y ? 1 : -1;
        }
    }

    Vector2Int GetRoomCenter(Rect room)
    {
        return new Vector2Int(
            Mathf.RoundToInt(room.x + room.width / 2),
            Mathf.RoundToInt(room.y + room.height / 2)
        );
    }

    // ----------------------------------------------------
    // 🧱 3Dオブジェクト生成
    void BuildStageObjects()
    {
        if (stageParent == null)
        {
            GameObject stageObj = new GameObject("StageMake");
            stageParent = stageObj.transform;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (mapData[y, x] == 0)
                {
                    Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    floor.transform.position = pos;
                    floor.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                    floor.GetComponent<Renderer>().material.color = Color.gray;
                    floor.transform.SetParent(stageParent);
                }
            }
        }

        // スポーン系
        playerInstance = SpawnAtRandom(playerPrefab, "Player");
        SpawnAtRandom(goalLightPrefab, "Goal_Light");
        for (int i = 0; i < 10; i++) SpawnAtRandom(enemyPrefab, "Enemy");
        for (int i = 0; i < 5; i++) SpawnAtRandom(treasurePrefab, "Treasure");

        Debug.Log("✅ ステージオブジェクト生成完了");
    }

    GameObject SpawnAtRandom(GameObject prefab, string nameTag)
    {
        if (!prefab) return null;

        for (int i = 0; i < 1000; i++)
        {
            int x = Random.Range(0, mapWidth);
            int y = Random.Range(0, mapHeight);
            if (mapData[y, x] == 0)
            {
                Vector3 pos = new Vector3(x * cellSize, 1, y * cellSize);
                GameObject obj = Instantiate(prefab, pos, Quaternion.identity, stageParent);
                obj.name = nameTag;
                spawnedObjects.Add(obj);
                return obj;
            }
        }
        return null;
    }

#if UNITY_AI_PRESENT
    void SetupNavMeshSurface()
    {
        surface = GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = gameObject.AddComponent<NavMeshSurface>();
            Debug.Log("🧭 NavMeshSurface を自動追加しました。");
        }

        surface.BuildNavMesh();
        Debug.Log("✅ NavMesh Bake 完了（自動）");
    }
#endif

    // ----------------------------------------------------
    // 🗺️ ミニマップ初期化
    void InitMinimap()
    {
        minimapTexture = new Texture2D(mapWidth * minimapScale, mapHeight * minimapScale);
        minimapTexture.filterMode = FilterMode.Point;

        if (minimapUI != null)
            minimapUI.texture = minimapTexture;

        UpdateMinimap();
    }

    void Update()
    {
        if (playerInstance == null) return;

        Vector3 playerPos = playerInstance.transform.position;
        int px = Mathf.RoundToInt(playerPos.x / cellSize);
        int py = Mathf.RoundToInt(playerPos.z / cellSize);

        RevealAround(px, py, revealRadius);
        UpdateMinimap();
    }

    void RevealAround(int cx, int cy, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int nx = cx + x;
                int ny = cy + y;
                if (nx >= 0 && ny >= 0 && nx < mapWidth && ny < mapHeight)
                    explored[ny, nx] = true;
            }
        }
    }

    void UpdateMinimap()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Color c = Color.black;

                if (explored[y, x])
                {
                    c = (mapData[y, x] == 0) ? new Color(0.4f, 0.4f, 0.4f) : Color.black;
                }

                for (int yy = 0; yy < minimapScale; yy++)
                    for (int xx = 0; xx < minimapScale; xx++)
                        minimapTexture.SetPixel(x * minimapScale + xx, y * minimapScale + yy, c);
            }
        }

        // 敵・宝箱・ゴールなどのマーカー
        foreach (var obj in spawnedObjects)
        {
            if (obj == null) continue;
            Vector3 pos = obj.transform.position;
            int x = Mathf.RoundToInt(pos.x / cellSize);
            int y = Mathf.RoundToInt(pos.z / cellSize);

            if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) continue;
            if (!explored[y, x]) continue;

            Color mark = Color.white;
            if (obj.name.Contains("Enemy")) mark = Color.red;
            else if (obj.name.Contains("Treasure")) mark = Color.yellow;
            else if (obj.name.Contains("Goal")) mark = Color.cyan;
            else if (obj.name.Contains("Player")) mark = Color.green;

            for (int yy = 0; yy < minimapScale; yy++)
                for (int xx = 0; xx < minimapScale; xx++)
                    minimapTexture.SetPixel(x * minimapScale + xx, y * minimapScale + yy, mark);
        }

        minimapTexture.Apply();
    }

    // ----------------------------------------------------
    // Getter
    public int GetMapWidth() => mapWidth;
    public int GetMapHeight() => mapHeight;
}
