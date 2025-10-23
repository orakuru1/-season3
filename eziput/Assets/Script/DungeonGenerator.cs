using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DungeonGenerator
/// - 部屋＋通路のローグライク風生成
/// - 床/壁/装飾配置
/// - 敵/宝箱/トラップ/ゴールを床セルにランダム配置（衝突回避）
///- - ミニマップ(RawImage);に探索表示(revealRadius);
/// Inspector で調整可能なパラメータを多めに用意しています。
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("Map Size")]
    public int mapWidth = 48;
    public int mapHeight = 40;
    public float cellSize = 2f;

    [Header("Room Settings")]
    public int minRoomCount = 6;
    public int maxRoomCount = 10;
    public int minRoomW = 4;
    public int maxRoomW = 10;
    public int minRoomH = 4;
    public int maxRoomH = 8;
    public int roomPadding = 1; // 部屋同士最小の隙間

    [Header("Prefabs (set in Inspector)")]
    public GameObject floorPrefab;   // 床（例: 平たいMesh）
    public GameObject wallPrefab;    // 壁
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject treasurePrefab;
    public GameObject trapPrefab;
    public GameObject goalPrefab;

    [Header("Decorations")]
    public GameObject[] decorationPrefabs; // 草や小石などランダムに置く

    [Header("Spawn Counts")]
    public int enemyCount = 12;
    public int treasureCount = 6;
    public int trapCount = 6;

    [Header("Minimap")]
    public RawImage minimapUI;
    public int minimapScale = 4; // 1セル = minimapScale px
    public int revealRadius = 3; // 探索半径

    [Header("Generation Options")]
    [Tooltip("通路太さ（1 = 1セル幅, 2 = 2セル幅）")]
    public int corridorWidth = 1;
    [Tooltip("少しランダムな蛇行を入れる確率")]
    [Range(0f, 1f)]
    public float corridorWiggleChance = 0.25f;

    // 内部データ
    private int[,] map; // 0 = floor, 1 = wall
    private bool[,] explored;
    private List<RectInt> rooms = new List<RectInt>();
    private List<Vector2Int> floorCells = new List<Vector2Int>();
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private GameObject playerInstance;
    private Texture2D minimapTexture;

    void Awake()
    {
        if (cellSize <= 0.01f) cellSize = 1f;
        GenerateDungeon();
        BuildLevel();
        InitMinimap();
    }

    #region Map Generation
    void GenerateDungeon()
    {
        map = new int[mapHeight, mapWidth];
        explored = new bool[mapHeight, mapWidth];

        // すべて壁で初期化
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                map[y, x] = 1;

        rooms.Clear();

        int targetRoomCount = Random.Range(minRoomCount, maxRoomCount + 1);
        int tries = 0;
        while (rooms.Count < targetRoomCount && tries < targetRoomCount * 8)
        {
            tries++;
            int rw = Random.Range(minRoomW, maxRoomW + 1);
            int rh = Random.Range(minRoomH, maxRoomH + 1);
            int rx = Random.Range(1, mapWidth - rw - 1);
            int ry = Random.Range(1, mapHeight - rh - 1);
            RectInt newRoom = new RectInt(rx, ry, rw, rh);

            bool overlap = false;
            foreach (var r in rooms)
            {
                // padding を確保して重なり判定
                RectInt padded = new RectInt(r.xMin - roomPadding, r.yMin - roomPadding, r.width + roomPadding * 2, r.height + roomPadding * 2);
                if (padded.Overlaps(newRoom))
                {
                    overlap = true;
                    break;
                }
            }
            if (!overlap)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }

        if (rooms.Count == 0)
        {
            Debug.LogWarning("部屋が1つも作成されませんでした。パラメータを見直してください。");
            return;
        }

        // rooms をランダム順にソートして接続をランダム化
        Shuffle(rooms);

        // 接続（最初の部屋から順にチェーン接続）
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int a = RoomCenter(rooms[i - 1]);
            Vector2Int b = RoomCenter(rooms[i]);
            CarveCorridorAtoB(a, b);
        }

        // 追加でランダムにより通路を伸ばす（自然感）
        for (int i = 0; i < rooms.Count / 2; i++)
        {
            Vector2Int start = RoomCenter(rooms[Random.Range(0, rooms.Count)]);
            Vector2Int end = new Vector2Int(Random.Range(1, mapWidth - 1), Random.Range(1, mapHeight - 1));
            CarveCorridorAtoB(start, end);
        }

        // floorCells を収集
        floorCells.Clear();
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                if (map[y, x] == 0)
                    floorCells.Add(new Vector2Int(x, y));
    }

    void CarveRoom(RectInt r)
    {
        for (int y = r.yMin; y < r.yMax; y++)
            for (int x = r.xMin; x < r.xMax; x++)
                SetFloor(x, y);
    }

    void CarveCorridorAtoB(Vector2Int a, Vector2Int b)
    {
        // L字＋wiggle を混ぜる
        Vector2Int cur = new Vector2Int(a.x, a.y);

        // horizontal first or vertical first randomly
        bool horizontalFirst = Random.value > 0.5f;

        if (horizontalFirst)
        {
            while (cur.x != b.x)
            {
                SetCorridorAt(cur.x, cur.y);
                cur.x += (b.x > cur.x) ? 1 : -1;

                // ときどき垂直に蛇行
                if (Random.value < corridorWiggleChance)
                {
                    int verticalSteps = Random.Range(1, 3);
                    for (int i = 0; i < verticalSteps; i++)
                    {
                        if (cur.y == b.y) break;
                        cur.y += (b.y > cur.y) ? 1 : -1;
                        SetCorridorAt(cur.x, cur.y);
                    }
                }
            }
            while (cur.y != b.y)
            {
                SetCorridorAt(cur.x, cur.y);
                cur.y += (b.y > cur.y) ? 1 : -1;
            }
        }
        else
        {
            while (cur.y != b.y)
            {
                SetCorridorAt(cur.x, cur.y);
                cur.y += (b.y > cur.y) ? 1 : -1;
                if (Random.value < corridorWiggleChance)
                {
                    int horizSteps = Random.Range(1, 3);
                    for (int i = 0; i < horizSteps; i++)
                    {
                        if (cur.x == b.x) break;
                        cur.x += (b.x > cur.x) ? 1 : -1;
                        SetCorridorAt(cur.x, cur.y);
                    }
                }
            }
            while (cur.x != b.x)
            {
                SetCorridorAt(cur.x, cur.y);
                cur.x += (b.x > cur.x) ? 1 : -1;
            }
        }

        // 最後のマスも床に
        SetCorridorAt(b.x, b.y);
    }

    void SetCorridorAt(int cx, int cy)
    {
        // corridorWidth 分を床にする（中心から半径）
        int half = Mathf.Max(0, corridorWidth - 1);
        for (int oy = -half; oy <= half; oy++)
            for (int ox = -half; ox <= half; ox++)
                SetFloor(cx + ox, cy + oy);
    }

    void SetFloor(int x, int y)
    {
        if (x <= 0 || y <= 0 || x >= mapWidth - 1 || y >= mapHeight - 1) return;
        map[y, x] = 0;
    }

    Vector2Int RoomCenter(RectInt r) => new Vector2Int(r.xMin + r.width / 2, r.yMin + r.height / 2);

    // Fisher-Yates
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
    #endregion

    #region Build Level (instantiate GameObjects)
    void BuildLevel()
    {
        // 親オブジェクト
        GameObject stageRoot = new GameObject("DungeonStage");
        stageRoot.transform.SetParent(transform, false);

        // 床/壁をインスタンス化（Prefabがない場合はPrimitiveで代用）
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0f, y * cellSize);
                if (map[y, x] == 0)
                {
                    GameObject f = InstantiatePrefabSafe(floorPrefab, PrimitiveType.Cube, pos, Quaternion.identity, stageRoot.transform);
                    f.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                    f.name = $"Floor_{x}_{y}";
                    // 少し装飾をランダムで置く
                    if (decorationPrefabs != null && decorationPrefabs.Length > 0 && Random.value < 0.06f)
                    {
                        GameObject deco = Instantiate(decorationPrefabs[Random.Range(0, decorationPrefabs.Length)], pos + Vector3.up * 0.05f, Quaternion.identity, stageRoot.transform);
                        spawnedObjects.Add(deco);
                    }
                }
                else
                {
                    GameObject w = InstantiatePrefabSafe(wallPrefab, PrimitiveType.Cube, pos + Vector3.up * (cellSize * 0.5f), Quaternion.identity, stageRoot.transform);
                    w.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                    w.name = $"Wall_{x}_{y}";
                }
            }
        }

        // プレイヤーを安全な床に生成（最初に見つかったfloorの中心をstartに）
        Vector2Int startCell = FindBestStartCell();
        Vector3 startPos = new Vector3(startCell.x * cellSize, 1f, startCell.y * cellSize);
        if (playerPrefab)
        {
            playerInstance = Instantiate(playerPrefab, startPos, Quaternion.identity);
            playerInstance.name = "Player";
            spawnedObjects.Add(playerInstance);
        }

        // ゴールは最も遠いfloorセルに置く（簡易）
        Vector2Int goalCell = FindFurthestCellFrom(startCell);
        if (goalPrefab != null)
        {
            Vector3 gpos = new Vector3(goalCell.x * cellSize, 1f, goalCell.y * cellSize);
            GameObject g = Instantiate(goalPrefab, gpos, Quaternion.identity);
            g.name = "Goal";
            spawnedObjects.Add(g);
        }

        // 敵・宝箱・トラップをランダムに配置（start周辺は避ける）
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        occupied.Add(startCell);
        occupied.Add(goalCell);

        // Shuffle floorCells for random placement
        Shuffle(floorCells);

        int placedEnemies = 0, placedTreasures = 0, placedTraps = 0;
        foreach (var cell in floorCells)
        {
            if (occupied.Contains(cell)) continue;
            // Skip if too close to start
            if (Vector2Int.Distance(cell, startCell) < 4f) continue;

            // Randomly place
            if (placedEnemies < enemyCount && Random.value < 0.08f)
            {
                SpawnAtCell(enemyPrefab, cell);
                placedEnemies++;
                occupied.Add(cell);
                continue;
            }
            if (placedTreasures < treasureCount && Random.value < 0.05f)
            {
                SpawnAtCell(treasurePrefab, cell);
                placedTreasures++;
                occupied.Add(cell);
                continue;
            }
            if (placedTraps < trapCount && Random.value < 0.04f)
            {
                SpawnAtCell(trapPrefab, cell);
                placedTraps++;
                occupied.Add(cell);
                continue;
            }

            if (placedEnemies >= enemyCount && placedTreasures >= treasureCount && placedTraps >= trapCount) break;
        }

        Debug.Log($"Dungeon built: rooms={rooms.Count}, floors={floorCells.Count}, enemies={placedEnemies}, treasures={placedTreasures}, traps={placedTraps}");
    }

    GameObject InstantiatePrefabSafe(GameObject prefab, PrimitiveType fallback, Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject go = null;
        if (prefab != null)
        {
            go = Instantiate(prefab, pos, rot, parent);
        }
        else
        {
            go = GameObject.CreatePrimitive(fallback);
            go.transform.position = pos;
            go.transform.rotation = rot;
            go.transform.SetParent(parent, true);
        }
        return go;
    }

    void SpawnAtCell(GameObject prefab, Vector2Int cell)
    {
        if (prefab == null) return;
        Vector3 pos = new Vector3(cell.x * cellSize, 1f, cell.y * cellSize);
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        obj.name = prefab.name;
        spawnedObjects.Add(obj);
    }

    Vector2Int FindBestStartCell()
    {
        // シンプル: 最初の部屋の中心にする（roomsが作られている前提）
        if (rooms.Count > 0)
        {
            return RoomCenter(rooms[0]);
        }
        // フォールバックで任意のfloorを返す
        if (floorCells.Count > 0) return floorCells[0];
        return new Vector2Int(mapWidth / 2, mapHeight / 2);
    }

    Vector2Int FindFurthestCellFrom(Vector2Int origin)
    {
        float bestDist = -1f;
        Vector2Int best = origin;
        foreach (var c in floorCells)
        {
            float d = Vector2Int.Distance(c, origin);
            if (d > bestDist)
            {
                bestDist = d;
                best = c;
            }
        }
        return best;
    }
    #endregion

    #region Minimap & Exploration
    void InitMinimap()
    {
        if (minimapUI == null)
        {
            Debug.LogWarning("minimapUI が未設定です。ミニマップは表示されません。");
            return;
        }

        int texW = mapWidth * minimapScale;
        int texH = mapHeight * minimapScale;
        minimapTexture = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        minimapTexture.filterMode = FilterMode.Point;

        // clear
        for (int y = 0; y < texH; y++)
            for (int x = 0; x < texW; x++)
                minimapTexture.SetPixel(x, y, new Color(0, 0, 0, 0f));

        minimapTexture.Apply();
        minimapUI.texture = minimapTexture;

        // reveal around start
        if (playerInstance != null)
        {
            Vector2Int pcell = WorldToCell(playerInstance.transform.position);
            RevealAround(pcell, revealRadius);
            RedrawMinimap();
        }
    }

    void Update()
    {
        if (playerInstance == null || minimapTexture == null) return;

        Vector2Int pcell = WorldToCell(playerInstance.transform.position);
        if (RevealAround(pcell, revealRadius))
        {
            RedrawMinimap();
        }

        // optional: draw player marker on top every frame
        DrawPlayerOnMinimap(pcell);
    }

    Vector2Int WorldToCell(Vector3 world)
    {
        int x = Mathf.RoundToInt(world.x / cellSize);
        int y = Mathf.RoundToInt(world.z / cellSize);
        x = Mathf.Clamp(x, 0, mapWidth - 1);
        y = Mathf.Clamp(y, 0, mapHeight - 1);
        return new Vector2Int(x, y);
    }

    bool RevealAround(Vector2Int center, int radius)
    {
        bool changed = false;
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int nx = center.x + dx;
                int ny = center.y + dy;
                if (nx < 0 || ny < 0 || nx >= mapWidth || ny >= mapHeight) continue;
                // 円形にしたければ下のチェックを有効に
                // if (dx*dx + dy*dy > radius*radius) continue;
                if (!explored[ny, nx])
                {
                    explored[ny, nx] = true;
                    changed = true;
                }
            }
        }
        return changed;
    }

    void RedrawMinimap()
    {
        int texW = minimapTexture.width;
        int texH = minimapTexture.height;

        // 基本の色
        Color floorCol = new Color(0.85f, 0.85f, 0.85f, 1f);
        Color wallCol  = new Color(0.18f, 0.18f, 0.18f, 1f);
        Color unseen   = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Color c = unseen;
                if (explored[y, x])
                {
                    c = (map[y, x] == 0) ? floorCol : wallCol;
                }
                // fill minimapScale block
                int baseX = x * minimapScale;
                int baseY = y * minimapScale;
                for (int yy = 0; yy < minimapScale; yy++)
                    for (int xx = 0; xx < minimapScale; xx++)
                        minimapTexture.SetPixel(baseX + xx, baseY + yy, c);
            }
        }

        // マーカー（敵・宝箱・トラップ・ゴール）を描く（探索済みセルのみ）
        foreach (var obj in spawnedObjects)
        {
            if (obj == null) continue;
            Vector2Int cell = WorldToCell(obj.transform.position);
            if (!explored[cell.y, cell.x]) continue;

            Color mark = Color.white;
            string n = obj.name.ToLower();
            if (n.Contains("enemy")) mark = new Color(0.85f, 0.2f, 0.2f, 1f);
            else if (n.Contains("treasure")) mark = new Color(0.95f, 0.85f, 0.1f, 1f);
            else if (n.Contains("trap")) mark = new Color(0.6f, 0.2f, 0.8f, 1f);
            else if (n.Contains("goal")) mark = new Color(0.2f, 0.9f, 0.6f, 1f);

            // マーカーは小さめの中央ブロック
            int baseX = cell.x * minimapScale;
            int baseY = cell.y * minimapScale;
            int size = Mathf.Max(1, minimapScale / 2);
            int offset = (minimapScale - size) / 2;
            for (int yy = 0; yy < size; yy++)
                for (int xx = 0; xx < size; xx++)
                    minimapTexture.SetPixel(baseX + offset + xx, baseY + offset + yy, mark);
        }

        minimapTexture.Apply();
        minimapUI.texture = minimapTexture;
    }

    void DrawPlayerOnMinimap(Vector2Int pcell)
    {
        // Redraw previously (to keep markers) then draw player marker
        RedrawMinimap();

        int baseX = pcell.x * minimapScale;
        int baseY = pcell.y * minimapScale;
        int size = Mathf.Max(1, minimapScale / 2);
        int offset = (minimapScale - size) / 2;
        Color pcol = new Color(0.2f, 0.6f, 1f, 1f);
        for (int yy = 0; yy < size; yy++)
            for (int xx = 0; xx < size; xx++)
                minimapTexture.SetPixel(baseX + offset + xx, baseY + offset + yy, pcol);

        minimapTexture.Apply();
        minimapUI.texture = minimapTexture;
    }
    #endregion

    #region Utilities
    #endregion
}
