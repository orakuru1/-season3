using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ElementGenerator : MonoBehaviour
{
    [Header("プレハブ")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject ceilingPrefab;

    [Header("天井設定")]
    public float ceilingHeight = 2.5f;      // 床から天井裏面まで
    public float ceilingThickness = 0.3f;

    public GameObject enemyPrefab;
    public GameObject treasurePrefab;
    public int treasureRandomCount = 0;
    public GameObject trapPrefab;
    public GameObject entrancePrefab;
    public GameObject exitPrefab;

    // ← ここにワーププレファブを追加
    [Header("ワープ(ミニゲームへ遷移)")]
    public GameObject warpPrefab;        // インスペクターでワーププレファブを設定
    public int warpTotalCount = 1;          // マップ全体に置く数（通常1）。0にすると配置されない。

    [Header("中ボス")]
    public GameObject midBossPrefab;
    public int midBossSearchRadius = 3; // 出口から何マス以内か


    [Header("マテリアル（Safe / Danger）")]
    public Material safeFloorMaterial;
    public Material dangerFloorMaterial;
    public Material safeWallMaterial;
    public Material dangerWallMaterial;
    public Material safeCeilingMaterial;
    public Material dangerCeilingMaterial;

    [Header("親オブジェクト")]
    public Transform elementParent;

    [Header("マップ設定")]
    public float cellSize = 2.0f;

    private int mapWidth;
    private int mapHeight;
    private List<GameObject> spawned = new List<GameObject>();

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;
    public float CellSize => cellSize;
    public Vector3 EntranceWorldPos { get; private set; }
    public bool EntranceFound { get; private set; } = false;

    public void GenerateFromMap(
        int[,] mapData,
        DungeonSettings settings,
        List<Room> rooms,
        Room startRoom,
        Room endRoom,
        IReadOnlyList<Vector2Int> floorList)
    {
        Debug.Log("[ElementGenerator] GenerateFromMap 実行開始");

        if (mapData == null)
        {
            Debug.LogError("[ElementGenerator] mapData が null です");
            return;
        }

        mapHeight = mapData.GetLength(0);
        mapWidth = mapData.GetLength(1);

        ClearSpawned();

        RouteType route = (GameManager.Instance != null)
            ? GameManager.Instance.CurrentRoute
            : RouteType.Safe;

        Debug.Log($"[ElementGenerator] CurrentRoute = {route}");

        // ==== 床と壁の生成 ====
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                GameObject prefab = (mapData[y, x] == 0) ? floorPrefab : wallPrefab;

                if (prefab != null)
                {
                    Vector3 pos = new Vector3(x * cellSize, 0f, y * cellSize);
                    GameObject go = Instantiate(prefab, pos, Quaternion.identity, elementParent);
                    go.name = (mapData[y, x] == 0) ? $"Floor_{x}_{y}" : $"Wall_{x}_{y}";
                    spawned.Add(go);

                    GridBlock block = go.GetComponent<GridBlock>();
                    if (block != null)
                    {
                        block.gridPos = new Vector2Int(x, y);
                        block.isWalkable = (mapData[y, x] == 0); // 0=床、1=壁

                        // GridManager に登録
                        if (GridManager.Instance != null)
                        {
                            GridManager.Instance.RegisterBlock(block);
                        }
                    }

                    Renderer rend = go.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        if (mapData[y, x] == 0 && route == RouteType.Safe && safeFloorMaterial != null)
                            rend.sharedMaterial = safeFloorMaterial;
                        else if (mapData[y, x] == 0 && route == RouteType.Danger && dangerFloorMaterial != null)
                            rend.sharedMaterial = dangerFloorMaterial;
                        else if (mapData[y, x] == 1 && route == RouteType.Safe && safeWallMaterial != null)
                            rend.sharedMaterial = safeWallMaterial;
                        else if (mapData[y, x] == 1 && route == RouteType.Danger && dangerWallMaterial != null)
                            rend.sharedMaterial = dangerWallMaterial;
                    }
                }
            }
        }

        // =====================================================================
        // 天井生成（固定高さ・自動計算なし）
        // =====================================================================
        float ceilingCenterY = ceilingHeight + ceilingThickness * 0.5f;

        float mapW = mapWidth * cellSize;
        float mapH = mapHeight * cellSize;

        Vector3 ceilingPos = new Vector3(
            mapW / 2f - cellSize / 2f,
            ceilingCenterY,
            mapH / 2f - cellSize / 2f
        );

        GameObject ceiling = Instantiate(ceilingPrefab, ceilingPos, Quaternion.identity, elementParent);
        ceiling.name = "Ceiling_All";
        ceiling.transform.localScale = new Vector3(mapW, ceilingThickness, mapH);

        var cr = ceiling.GetComponent<MeshRenderer>() ?? ceiling.GetComponentInChildren<MeshRenderer>();
        if (cr != null)
            cr.sharedMaterial = (route == RouteType.Danger) ? dangerCeilingMaterial : safeCeilingMaterial;

        spawned.Add(ceiling);

        // Place entrance & exit
        // Place entrance
        if (startRoom != null && entrancePrefab != null)
        {
            Vector3 p = new Vector3(startRoom.Center.x * cellSize, 0.5f, startRoom.Center.y * cellSize);
            var e = Instantiate(entrancePrefab, p, Quaternion.identity, elementParent);
            e.name = "Entrance";
            spawned.Add(e);

            // ★入口座標を保存する
            EntranceWorldPos = p;
            EntranceFound = true;
        }

        GameObject exitObject = null; // ★ 追加（クラス内 or メソッド先頭）
        if (endRoom != null && exitPrefab != null)
        {
            Vector3 p = new Vector3(endRoom.Center.x * cellSize, 0.5f, endRoom.Center.y * cellSize);
            exitObject = Instantiate(exitPrefab, p, Quaternion.identity, elementParent);
            exitObject.name = "Exit";
            spawned.Add(exitObject);
        }

        // =======================================================================
        // ★ 中ボスを出口付近に配置（最終完成形）
        // =======================================================================
        if (midBossPrefab != null && exitObject != null)
        {
            Vector3 exitPos = exitObject.transform.position;

            int exitX = Mathf.RoundToInt(exitPos.x / cellSize);
            int exitY = Mathf.RoundToInt(exitPos.z / cellSize);

            List<Vector2Int> candidates = new List<Vector2Int>();

            for (int dy = -midBossSearchRadius; dy <= midBossSearchRadius; dy++)
            {
                for (int dx = -midBossSearchRadius; dx <= midBossSearchRadius; dx++)
                {
                    int x = exitX + dx;
                    int y = exitY + dy;

                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                        continue;

                    // 出口そのものは除外
                    if (x == exitX && y == exitY)
                        continue;

                    // 床のみ
                    if (mapData[y, x] == 0)
                        candidates.Add(new Vector2Int(x, y));
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("中ボス配置候補が見つかりませんでした");
                return;
            }

            Vector2Int spawnCell = candidates[Random.Range(0, candidates.Count)];

            SpawnFeature(
                midBossPrefab,
                spawnCell,
                "MidBoss"
            );

            // ★ 中ボスだけ移動制限を設定
            var bossObj = spawned.Last();
            var ai = bossObj.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.useMoveLimit = true;
                ai.moveCenter = spawnCell;
                ai.moveRadius = 3; // ← 好きな範囲
            }

            Debug.Log($"[MidBoss] Spawned at {spawnCell}");
        }


        // Place features per room (use settings if available)
        if (rooms != null && rooms.Count > 0)
        {
            // determine counts or ratios
            int enemyPerRoom = 1;
            int treasurePerRoom = 0;
            int trapPerRoom = 0;

            // If settings has integer counts, use them (best-effort)
            if (settings != null)
            {
                // many versions of DungeonSettings exist; try common fields
                try
                {
                    // if settings has enemyCount/treasureCount/trapCount fields, distribute them
                    int totalEnemies = (int)typeof(DungeonSettings).GetField("enemyCount")?.GetValue(settings);
                    int totalTreasures = (int)typeof(DungeonSettings).GetField("treasureCount")?.GetValue(settings);
                    int totalTraps = (int)typeof(DungeonSettings).GetField("trapCount")?.GetValue(settings);

                    enemyPerRoom = Mathf.Max(1, totalEnemies / Mathf.Max(1, rooms.Count));
                    treasurePerRoom = Mathf.Max(0, totalTreasures / Mathf.Max(1, rooms.Count));
                    trapPerRoom = Mathf.Max(0, totalTraps / Mathf.Max(1, rooms.Count));
                }
                catch
                {
                    // fallback: use rates if available
                    try
                    {
                        float tr = (float)typeof(DungeonSettings).GetField("treasureSpawnRate")?.GetValue(settings);
                        float er = (float)typeof(DungeonSettings).GetField("enemySpawnRate")?.GetValue(settings);
                        float pr = (float)typeof(DungeonSettings).GetField("trapSpawnRate")?.GetValue(settings);
                        // approximate counts per room
                        enemyPerRoom = Mathf.Max(1, Mathf.RoundToInt(er * 5f));
                        treasurePerRoom = Mathf.RoundToInt(tr * 3f);
                        trapPerRoom = Mathf.RoundToInt(pr * 3f);
                    }
                    catch
                    {
                        // totally fallback defaults
                        enemyPerRoom = 1;
                        treasurePerRoom = 0;
                        trapPerRoom = 0;
                    }
                }
            }

            // For each room, place features in random interior positions
            foreach (var r in rooms)
            {
                // ensure interior bounds
                int minX = r.x + 1;
                int maxX = r.x + r.width - 2;
                int minY = r.y + 1;
                int maxY = r.y + r.height - 2;
                if (maxX < minX || maxY < minY) continue;

                // place enemies
                for (int i = 0; i < enemyPerRoom; i++)
                {
                    int rx = Random.Range(minX, maxX + 1);
                    int ry = Random.Range(minY, maxY + 1);
                    SpawnFeature(enemyPrefab, new Vector2Int(rx,ry), "Enemy");
                }
                // treasures
                for (int i = 0; i < treasurePerRoom; i++)
                {
                    int rx = Random.Range(minX, maxX + 1);
                    int ry = Random.Range(minY, maxY + 1);
                    SpawnFeature(treasurePrefab, new Vector2Int(rx, ry), "Treasure");
                }
                // traps
                for (int i = 0; i < trapPerRoom; i++)
                {
                    int rx = Random.Range(minX, maxX + 1);
                    int ry = Random.Range(minY, maxY + 1);
                    SpawnFeature(trapPrefab, new Vector2Int(rx, ry), "Trap");
                }

            } // foreach rooms

            // ====================================================================
            // ★ マップ全体で warpTotalCount 個のワープを配置する
            // ====================================================================
            if (warpPrefab != null && warpTotalCount > 0 && rooms != null && rooms.Count > 0)
            {
                int placedWarpCount = 0;
                int globalAttempts = 0;

                while (placedWarpCount < warpTotalCount && globalAttempts < warpTotalCount * 30)
                {
                    globalAttempts++;

                    // ① ランダムな部屋を選ぶ（スタート・ゴール除外）
                    Room r = rooms[Random.Range(0, rooms.Count)];
                    if (r == startRoom || r == endRoom) continue;

                    int minX = r.x + 1;
                    int maxX = r.x + r.width - 2;
                    int minY = r.y + 1;
                    int maxY = r.y + r.height - 2;
                    if (maxX < minX || maxY < minY) continue;

                    // ② ランダムセル
                    int wx = Random.Range(minX, maxX + 1);
                    int wy = Random.Range(minY, maxY + 1);
                    Vector2Int cell = new Vector2Int(wx, wy);

                    // 床チェック
                    if (mapData[wy, wx] != 0) continue;

                    // スタート／ゴールと被らない
                    if (startRoom != null && cell == startRoom.Center) continue;
                    if (endRoom != null && cell == endRoom.Center) continue;

                    // occupant チェック
                    GridBlock gb = GridManager.Instance.GetBlock(cell);
                    if (gb != null && gb.occupantUnit != null) continue;

                    // ③ 衝突チェック（セル基準）
                    Vector3 spawnPos = new Vector3(
                        cell.x * cellSize,
                        0.5f,
                        cell.y * cellSize
                    );

                    float checkRadius = Mathf.Max(0.3f, cellSize * 0.4f);
                    Collider[] cols = Physics.OverlapSphere(spawnPos, checkRadius);
                    if (cols.Any(c => c != null && c.CompareTag("Wall")))
                        continue;

                    // ④ 生成
                    Quaternion rot = warpPrefab.transform.rotation;
                    GameObject warp = Instantiate(warpPrefab, spawnPos, rot, elementParent);
                    warp.name = $"Warp_{cell.x}_{cell.y}";
                    spawned.Add(warp);

                    placedWarpCount++;
                }

                if (placedWarpCount < warpTotalCount)
                {
                    Debug.LogWarning($"[ElementGenerator] ワープ配置数が不足: {placedWarpCount}/{warpTotalCount}");
                }
            }
        }
        else
        {
            // fallback: scatter features on open floor
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (mapData[y, x] == 0)
                    {
                        float val = Random.value;
                        if (val < 0.005f) SpawnFeature(enemyPrefab, new Vector2Int(x, y), "Enemy");
                        else if (val < 0.007f) SpawnFeature(treasurePrefab, new Vector2Int(x, y), "Treasure");
                        else if (val < 0.009f) SpawnFeature(trapPrefab, new Vector2Int(x, y), "Trap");
                    }
                }
            }
        }

        Debug.Log("[ElementGenerator] 要素の配置が完了しました。");

        // =======================================================================
        // ★ ランダム宝箱配置（絶対に壁と干渉せず、必ず床に接地する最終版）
        // =======================================================================
        if (treasurePrefab != null && treasureRandomCount > 0)
        {
            List<Vector2Int> candidates = floorList
                .Where(c =>
                {
                    GridBlock b = GridManager.Instance.GetBlock(c);
                    return b != null && b.isWalkable &&
                           b.occupantUnit == null;
                })
                .ToList();

            for (int i = 0; i < treasureRandomCount && candidates.Count > 0; i++)
            {
                int idx = Random.Range(0, candidates.Count);
                Vector2Int cell = candidates[idx];
                candidates.RemoveAt(idx);

                // ★ 宝箱だけ180°回転
                SpawnFeature(
                    treasurePrefab,
                    cell,
                    "Treasure",
                    Quaternion.Euler(180f, 0f, 0f)
                );
            }
        }
    }

    // -------------------------------------------------------------------------
    //    Feature spawn
    // -------------------------------------------------------------------------
    // spawn helpers
    private Vector3 GridToWorld(Vector2Int cell)
    {
        return new Vector3(
            cell.x * cellSize + cellSize * 0.5f,
            0,
            cell.y * cellSize + cellSize * 0.5f
        );
    }

    private void SpawnFeature(GameObject prefab, Vector2Int cell, string baseName, Quaternion? rotation = null
)
    {
        if (prefab == null) return;
        Quaternion rot = rotation ?? Quaternion.identity;

        Vector3 pos = GridToWorld(cell);
        GameObject o = Instantiate(prefab, pos, rot, elementParent);
        o.name = $"{baseName}_{cell.x}_{cell.y}";
        spawned.Add(o);

        GridBlock block = GridManager.Instance.GetBlock(cell);
        if (block == null) return;

        // Unit（敵・ボス）
        Unit unit = o.GetComponent<Unit>();
        if (unit != null)
        {
            unit.gridPos = cell;
            block.occupantUnit = unit;

            if (TurnManager.Instance != null &&
                !TurnManager.Instance.allUnits.Contains(unit))
            {
                TurnManager.Instance.allUnits.Add(unit);
            }
            return;
        }

        // 宝箱・ワープ・罠など
    }

    private void ClearSpawned()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null) DestroyImmediate(spawned[i]);
        }
        spawned.Clear();
    }
}
