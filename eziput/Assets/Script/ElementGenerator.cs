using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// DungeonGenerator から受け取った mapData / rooms / start/end に応じて
/// - 床・壁（Prefab）を配置（または既に配置されている場合は色のみ変更）
/// - 各部屋に対して敵／宝／罠を配置（settings の割合 or 個数を参照）
/// - 入口・出口を配置
/// - Safe / Danger による見た目差（マテリアル）を適用
/// </summary>
public class ElementGenerator : MonoBehaviour
{
    [Header("プレハブ")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject enemyPrefab;
    public GameObject treasurePrefab;
    public GameObject trapPrefab;
    public GameObject entrancePrefab;
    public GameObject exitPrefab;

    [Header("マテリアル（Safe / Danger）")]
    public Material safeFloorMaterial;
    public Material dangerFloorMaterial;
    public Material safeWallMaterial;
    public Material dangerWallMaterial;

    [Header("親オブジェクト")]
    public Transform elementParent;

    [Header("マップ設定")]
    public float cellSize = 1.0f;

    // internal track
    private int mapWidth;
    private int mapHeight;
    private List<GameObject> spawned = new List<GameObject>();

    // CameraSetup 用ゲッタ
    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;
    public float CellSize => cellSize;

    /// <summary>
    /// DungeonGenerator から呼ばれるエントリーポイント
    /// </summary>
    public void GenerateFromMap(int[,] mapData, DungeonSettings settings, List<Room> rooms, Room startRoom, Room endRoom)
    {
        if (mapData == null)
        {
            Debug.LogError("[ElementGenerator] mapData が null です");
            return;
        }

        mapHeight = mapData.GetLength(0);
        mapWidth = mapData.GetLength(1);

        // clear previous
        ClearSpawned();

        // determine current route
        RouteType route = RouteType.Safe;
        if (GameManager.Instance != null) route = GameManager.Instance.CurrentRoute; // assuming GameManager has 'CurrentRoute' field

        // place tiles (floor and walls) — if prefabs provided
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

                    // apply material based on route
                    Renderer rend = go.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        if (mapData[y, x] == 0 && (route == RouteType.Safe) && safeFloorMaterial != null) rend.sharedMaterial = safeFloorMaterial;
                        else if (mapData[y, x] == 0 && (route == RouteType.Danger) && dangerFloorMaterial != null) rend.sharedMaterial = dangerFloorMaterial;
                        else if (mapData[y, x] == 1 && (route == RouteType.Safe) && safeWallMaterial != null) rend.sharedMaterial = safeWallMaterial;
                        else if (mapData[y, x] == 1 && (route == RouteType.Danger) && dangerWallMaterial != null) rend.sharedMaterial = dangerWallMaterial;
                    }
                }
            }
        }

        // Place entrance & exit
        if (startRoom != null && entrancePrefab != null)
        {
            Vector3 p = new Vector3(startRoom.Center.x * cellSize, 0.5f, startRoom.Center.y * cellSize);
            var e = Instantiate(entrancePrefab, p, Quaternion.identity, elementParent);
            e.name = "Entrance";
            spawned.Add(e);
        }
        if (endRoom != null && exitPrefab != null)
        {
            Vector3 p = new Vector3(endRoom.Center.x * cellSize, 0.5f, endRoom.Center.y * cellSize);
            var e = Instantiate(exitPrefab, p, Quaternion.identity, elementParent);
            e.name = "Exit";
            spawned.Add(e);
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
                    SpawnFeature(enemyPrefab, rx, ry, "Enemy");
                }
                // treasures
                for (int i = 0; i < treasurePerRoom; i++)
                {
                    int rx = Random.Range(minX, maxX + 1);
                    int ry = Random.Range(minY, maxY + 1);
                    SpawnFeature(treasurePrefab, rx, ry, "Treasure");
                }
                // traps
                for (int i = 0; i < trapPerRoom; i++)
                {
                    int rx = Random.Range(minX, maxX + 1);
                    int ry = Random.Range(minY, maxY + 1);
                    SpawnFeature(trapPrefab, rx, ry, "Trap");
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
                        if (val < 0.005f) SpawnFeature(enemyPrefab, x, y, "Enemy");
                        else if (val < 0.007f) SpawnFeature(treasurePrefab, x, y, "Treasure");
                        else if (val < 0.009f) SpawnFeature(trapPrefab, x, y, "Trap");
                    }
                }
            }
        }

        Debug.Log("[ElementGenerator] 要素の配置が完了しました。");
    }

    // spawn helpers
    private void SpawnFeature(GameObject prefab, int x, int y, string baseName)
    {
        if (prefab == null) return;
        Vector3 p = new Vector3(x * cellSize, 0.5f, y * cellSize);
        GameObject o = Instantiate(prefab, p, Quaternion.identity, elementParent);
        o.name = $"{baseName}_{x}_{y}";
        spawned.Add(o);
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
