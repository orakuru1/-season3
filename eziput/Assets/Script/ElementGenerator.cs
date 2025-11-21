using UnityEngine;
using System.Collections.Generic;

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

    private int mapWidth;
    private int mapHeight;
    private List<GameObject> spawned = new List<GameObject>();

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;
    public float CellSize => cellSize;

    public void GenerateFromMap(int[,] mapData, DungeonSettings settings, List<Room> rooms, Room startRoom, Room endRoom, IReadOnlyList<Vector2Int> floorList)
    {
        Debug.Log("[ElementGenerator] GenerateFromMap 実行開始");

        if (mapData == null)
        {
            Debug.LogError("[ElementGenerator] mapData が null です");
            return;
        }

        // --- 追加デバッグ ---
        Debug.Log($"[ElementGenerator] map size = {mapData.GetLength(1)} x {mapData.GetLength(0)}");
        Debug.Log($"[ElementGenerator] rooms count = {(rooms != null ? rooms.Count : 0)}");
        Debug.Log($"[ElementGenerator] startRoom = {startRoom?.Center}, endRoom = {endRoom?.Center}");
        Debug.Log($"[ElementGenerator] floorList count = {(floorList != null ? floorList.Count : 0)}");
        if (floorList != null)
        {
            int preview = Mathf.Min(10, floorList.Count);
            for (int i = 0; i < preview; i++)
                Debug.Log($"[ElementGenerator] floorList[{i}] = {floorList[i]}");
        }

        mapHeight = mapData.GetLength(0);
        mapWidth = mapData.GetLength(1);

        ClearSpawned();

        // --- Safe / Danger route 確認ログ ---
        RouteType route = RouteType.Safe;
        if (GameManager.Instance != null) route = GameManager.Instance.CurrentRoute;
        Debug.Log($"[ElementGenerator] CurrentRoute = {route}");

        // ==== tile placement ====
        Debug.Log("[ElementGenerator] Start placing tiles (floor/wall)...");

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

                    // --- 配置ログ ---
                    Debug.Log($"[ElementGenerator] Tile placed: {(mapData[y, x] == 0 ? "Floor" : "Wall")} at ({x},{y})");

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

        // ==== entrance / exit ====
        Debug.Log("[ElementGenerator] Placing entrance & exit...");

        if (startRoom != null && entrancePrefab != null)
        {
            Vector3 p = new Vector3(startRoom.Center.x * cellSize, 0.5f, startRoom.Center.y * cellSize);
            var e = Instantiate(entrancePrefab, p, Quaternion.identity, elementParent);
            e.name = "Entrance";
            spawned.Add(e);
            Debug.Log($"[ElementGenerator] Entrance placed at {startRoom.Center}");
        }

        if (endRoom != null && exitPrefab != null)
        {
            Vector3 p = new Vector3(endRoom.Center.x * cellSize, 0.5f, endRoom.Center.y * cellSize);
            var e = Instantiate(exitPrefab, p, Quaternion.identity, elementParent);
            e.name = "Exit";
            spawned.Add(e);
            Debug.Log($"[ElementGenerator] Exit placed at {endRoom.Center}");
        }

        // ==== room features ====
        Debug.Log("[ElementGenerator] Feature placement開始");

        if (rooms != null && rooms.Count > 0)
        {
            foreach (var r in rooms)
            {
                Debug.Log($"[ElementGenerator] Room: pos=({r.x},{r.y}) size=({r.width}x{r.height})");

                int minX = r.x + 1;
                int maxX = r.x + r.width - 2;
                int minY = r.y + 1;
                int maxY = r.y + r.height - 2;

                if (maxX < minX || maxY < minY)
                {
                    Debug.LogWarning("[ElementGenerator] Room interior is too small. Skipped.");
                    continue;
                }
            }
        }

        // ==== ランダム宝配置 ====
        Debug.Log("[ElementGenerator] Start Random Treasure Placement");

        int treasureRandomCount = 5;
        IReadOnlyList<Vector2Int> safeFloorList = floorList;

        if (safeFloorList == null || safeFloorList.Count == 0)
        {
            Debug.LogWarning("[ElementGenerator] floorList が空です。mapData から床リストを作ります。");

            List<Vector2Int> tmp = new List<Vector2Int>();
            for (int y = 0; y < mapData.GetLength(0); y++)
                for (int x = 0; x < mapData.GetLength(1); x++)
                    if (mapData[y, x] == 0)
                        tmp.Add(new Vector2Int(x, y));

            safeFloorList = tmp;
            Debug.Log($"[ElementGenerator] fallback floorList count = {safeFloorList.Count}");
        }

        if (safeFloorList.Count == 0)
        {
            Debug.LogError("[ElementGenerator] 有効な floorList がありません。宝箱配置をスキップ");
        }
        else
        {
            Debug.Log($"[ElementGenerator] Random Treasure: floor candidates = {safeFloorList.Count}");
            HashSet<Vector2Int> used = new HashSet<Vector2Int>();

            for (int i = 0; i < treasureRandomCount; i++)
            {
                int attempts = 0;
                Vector2Int cell;

                do
                {
                    int idx = Random.Range(0, safeFloorList.Count);
                    cell = safeFloorList[idx];
                    attempts++;

                    if (attempts > 50)
                    {
                        Debug.LogWarning("[ElementGenerator] 50回試して配置できなかったためスキップ");
                        break;
                    }
                }
                while (used.Contains(cell));

                used.Add(cell);

                Vector3 spawnPos = new Vector3(cell.x * cellSize, 1.0f, cell.y * cellSize);
                GameObject t = Instantiate(treasurePrefab, spawnPos, Quaternion.identity, elementParent);
                t.name = $"RandomTreasure_{cell.x}_{cell.y}";
                spawned.Add(t);

                Debug.Log($"[ElementGenerator] RandomTreasure placed at {cell}");
            }
        }

        Debug.Log("[ElementGenerator] GenerateFromMap 完了");
    }

    private void SpawnFeature(GameObject prefab, int x, int y, string baseName)
    {
        if (prefab == null) return;

        Vector3 p = new Vector3(x * cellSize, 0.5f, y * cellSize);
        GameObject o = Instantiate(prefab, p, Quaternion.identity, elementParent);
        o.name = $"{baseName}_{x}_{y}";
        spawned.Add(o);

        Debug.Log($"[ElementGenerator] Feature '{baseName}' spawned at ({x},{y})");
    }

    private void ClearSpawned()
    {
        Debug.Log("[ElementGenerator] ClearSpawned");
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null) DestroyImmediate(spawned[i]);
        }
        spawned.Clear();
    }
}
