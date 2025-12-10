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

    // ← ここにワーププレファブを追加
    [Header("ワープ(ミニゲームへ遷移)")]
    public GameObject warpPrefab;        // インスペクターでワーププレファブを設定
    public int warpTotalCount = 1;          // マップ全体に置く数（通常1）。0にすると配置されない。

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

                // ----------------------------
                // ★ ここからワープ配置ロジック（追加）
                // ----------------------------

                // スタート部屋／ゴール部屋には置かない（部屋そのものを比較）
                if (warpPrefab != null && warpTotalCount > 0)
                {
                    bool skipThisRoom = false;
                    if (startRoom != null && ReferenceEquals(r, startRoom)) skipThisRoom = true;
                    if (endRoom != null && ReferenceEquals(r, endRoom)) skipThisRoom = true;

                    // もし Room 型が同一オブジェクトでなくても、中の座標で判断したい場合は下のコメント解除：
                    // if (!skipThisRoom && startRoom != null &&
                    //     (r.Center.x == startRoom.Center.x && r.Center.y == startRoom.Center.y)) skipThisRoom = true;
                    // if (!skipThisRoom && endRoom != null &&
                    //     (r.Center.x == endRoom.Center.x && r.Center.y == endRoom.Center.y)) skipThisRoom = true;

                    if (!skipThisRoom)
                    {
                        for (int w = 0; w < warpTotalCount; w++)
                        {
                            bool placedWarp = false;
                            int warpAttempts = 0;
                            // try to find a valid grid cell inside the room
                            while (!placedWarp && warpAttempts < 40)
                            {
                                warpAttempts++;
                                int wx = Random.Range(minX, maxX + 1);
                                int wy = Random.Range(minY, maxY + 1);

                                // 1) 必ず床であること
                                if (wx < 0 || wy < 0 || wy >= mapHeight || wx >= mapWidth) continue;
                                if (mapData[wy, wx] != 0) continue; // 床以外は不可

                                // 2) スタート/ゴールのセンターセルと被らないようにする
                                bool conflictWithStartOrEnd = false;
                                if (startRoom != null)
                                {
                                    if (wx == startRoom.Center.x && wy == startRoom.Center.y) conflictWithStartOrEnd = true;
                                }
                                if (endRoom != null)
                                {
                                    if (wx == endRoom.Center.x && wy == endRoom.Center.y) conflictWithStartOrEnd = true;
                                }
                                if (conflictWithStartOrEnd) continue;

                                // 3) グリッドに occupant が居ないこと（Unit がいる場合は不可）
                                GridBlock gb = null;
                                if (GridManager.Instance != null)
                                {
                                    gb = GridManager.Instance.GetBlock(new Vector2Int(wx, wy));
                                    if (gb != null && gb.occupantUnit != null) continue;
                                }

                                // 4) 物理的にめり込まないか軽くチェック（OverlapSphere）
                                Vector3 spawnPos = new Vector3(wx * cellSize + cellSize * 0.5f, 0.5f, wy * cellSize + cellSize * 0.5f);
                                float checkRadius = Mathf.Max(0.3f, cellSize * 0.4f);
                                Collider[] cols = Physics.OverlapSphere(spawnPos, checkRadius);
                                bool collision = false;
                                foreach (var c in cols)
                                {
                                    if (c == null) continue;
                                    if (c.CompareTag("Wall")) { collision = true; break; }
                                    // 壁レイヤーが定義されている場合の追加チェックはここに入れられます
                                }
                                if (collision) continue;

                                // 5) 置く（Y高さは0.5で仮置き。必要なら調整してください）
                                GameObject warp = Instantiate(warpPrefab, spawnPos, Quaternion.identity, elementParent);
                                warp.name = $"Warp_{wx}_{wy}";
                                spawned.Add(warp);

                                // もし warp に特別な初期化が必要ならここで行う
                                placedWarp = true;
                            } // while attempts
                            if (!placedWarp)
                            {
                                Debug.LogWarning($"[ElementGenerator] Room ({r.x},{r.y}) にワープを配置できませんでした（試行数上限）");
                            }
                        } // for warpTotalCount
                    } // if not skip room
                } // if warpPrefab
                // ----------------------------
                // ★ ワープ配置ここまで
                // ----------------------------
            } // foreach rooms
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




        // =======================================================================
        // ★ ランダム宝箱配置（絶対に壁と干渉せず、必ず床に接地する最終版）
        // =======================================================================

        Debug.Log("[ElementGenerator] Start Random Treasure Placement");

        int treasureRandomCount = 5;
        IReadOnlyList<Vector2Int> safeFloorList = floorList;

        if (safeFloorList == null || safeFloorList.Count == 0)
        {
            List<Vector2Int> tmp = new List<Vector2Int>();
            for (int y = 0; y < mapData.GetLength(0); y++)
                for (int x = 0; x < mapData.GetLength(1); x++)
                    if (mapData[y, x] == 0)
                        tmp.Add(new Vector2Int(x, y));
            safeFloorList = tmp;
        }

        if (safeFloorList.Count == 0)
        {
            Debug.LogError("[ElementGenerator] floorList に有効な床がありません。");
        }
        else
        {
            HashSet<Vector2Int> used = new HashSet<Vector2Int>();
            Quaternion rotation = Quaternion.Euler(180, 0, 0);

            // レイヤー／マスク
            int wallLayer = LayerMask.NameToLayer("Wall");           // -1 の場合はレイヤー未定義
            int floorLayer = LayerMask.NameToLayer("Floor");         // -1 の場合はレイヤー未定義
            int floorMask = (floorLayer >= 0) ? (1 << floorLayer) : 0;

            for (int i = 0; i < treasureRandomCount; i++)
            {
                int attempts = 0;
                Vector2Int cell;

                // 未使用セルを選択（タイムアウトあり）
                do
                {
                    int idx = Random.Range(0, safeFloorList.Count);
                    cell = safeFloorList[idx];
                    attempts++;
                    if (attempts > 200) break;
                }
                while (used.Contains(cell));

                if (attempts > 200)
                {
                    Debug.LogWarning("[ElementGenerator] 適切なセルが見つかりませんでした（タイムアウト）");
                    continue;
                }

                int positionAttempts = 0;
                bool placed = false;

                while (!placed && positionAttempts < 120)
                {
                    positionAttempts++;

                    // --- セルの中心位置から高めにスタートして落とす ---
                    Vector3 spawnPos = new Vector3(
                        cell.x * cellSize + cellSize * 0.5f,
                        5.0f, // 高めから下向きに判定
                        cell.y * cellSize + cellSize * 0.5f
                    );

                    // 物理同期（重要）
                    Physics.SyncTransforms();

                    // 1) 厳密な安全位置探索（ComputePenetration 組み込み版）
                    Vector3 safePos;
                    bool valid = TryFindValidTreasurePosition_Strict_ComputePenetration(
                        treasurePrefab, spawnPos, rotation, out safePos, wallLayer);

                    if (!valid)
                    {
                        // 別の近傍セルへ変更して再試行
                        int idx = Random.Range(0, safeFloorList.Count);
                        cell = safeFloorList[idx];
                        continue;
                    }

                    // 2) 床への Raycast（必ず Floor レイヤーに当たることを要求）
                    Vector3 rayStart = safePos + Vector3.up * 6f; // 十分高くから落とす
                    RaycastHit floorHit;
                    bool floorHitOk = false;

                    if (floorLayer >= 0)
                    {
                        // Floor レイヤーが存在すればそれに限定して探す
                        floorHitOk = Physics.Raycast(rayStart, Vector3.down, out floorHit, 20f, floorMask);
                    }
                    else
                    {
                        // Floor レイヤーが無ければ通常の Raycast（床かどうかは後で判定）
                        floorHitOk = Physics.Raycast(rayStart, Vector3.down, out floorHit, 20f);
                    }

                    if (!floorHitOk)
                    {
                        // 床が見つからなければその場所は不適
                        int idx = Random.Range(0, safeFloorList.Count);
                        cell = safeFloorList[idx];
                        continue;
                    }

                    // 3) 生成（safePos 高さに生成してから下で調整）
                    GameObject t = Instantiate(treasurePrefab, safePos, rotation, elementParent);
                    // 物理同期してから最終当たり判定
                    Physics.SyncTransforms();

                    // 4) 最終判定（OverlapBox大 + Sphere）
                    bool finalCollision = false;

                    Collider placedCollider = t.GetComponentInChildren<Collider>();
                    Bounds placedBounds;
                    if (placedCollider != null)
                        placedBounds = placedCollider.bounds;
                    else
                    {
                        Renderer rr = t.GetComponentInChildren<Renderer>();
                        if (rr != null) placedBounds = rr.bounds;
                        else placedBounds = new Bounds(t.transform.position, Vector3.one * 0.5f);
                    }

                    Vector3 placedCenter = placedBounds.center;
                    Vector3 placedHalf = placedBounds.extents;

                    // OverlapBox（大きめ）
                    Collider[] finalCols = Physics.OverlapBox(placedCenter, placedHalf * 1.12f, t.transform.rotation);
                    foreach (var c in finalCols)
                    {
                        if (c == null) continue;
                        if (c.transform.IsChildOf(t.transform)) continue;
                        if (c.CompareTag("Wall") || (wallLayer >= 0 && c.gameObject.layer == wallLayer))
                        {
                            finalCollision = true;
                            break;
                        }
                    }

                    // Sphereチェック（角のめり込み対策）
                    if (!finalCollision)
                    {
                        float sphereR = Mathf.Max(placedHalf.x, placedHalf.z) * 1.08f;
                        Collider[] hits = Physics.OverlapSphere(placedCenter, sphereR);
                        foreach (var c in hits)
                        {
                            if (c == null) continue;
                            if (c.transform.IsChildOf(t.transform)) continue;
                            if (c.CompareTag("Wall") || (wallLayer >= 0 && c.gameObject.layer == wallLayer))
                            {
                                finalCollision = true;
                                break;
                            }
                        }
                    }

                    if (finalCollision)
                    {
                        Destroy(t);
                        continue;
                    }

                    // 5) 床にピッタリ接地（floorLayer が有効なら floorHit を使う）
                    RaycastHit finalHit;
                    Vector3 finalRayStart = t.transform.position + Vector3.up * 6f;
                    bool finalFloorOk = false;

                    if (floorLayer >= 0)
                    {
                        finalFloorOk = Physics.Raycast(finalRayStart, Vector3.down, out finalHit, 30f, floorMask);
                    }
                    else
                    {
                        finalFloorOk = Physics.Raycast(finalRayStart, Vector3.down, out finalHit, 30f);
                    }

                    if (!finalFloorOk)
                    {
                        // 床が無ければ破棄して再試行
                        Destroy(t);
                        continue;
                    }

                    // 高さを床に合わせる（少し浮きやめり込みがある場合は補正）
                    float bottomOffset = placedBounds.extents.y; // おおよその高さ補正（中心から底まで）
                    Vector3 groundedPos = new Vector3(t.transform.position.x, finalHit.point.y + bottomOffset, t.transform.position.z);
                    t.transform.position = groundedPos;

                    // 最終再チェック（念のため）
                    Physics.SyncTransforms();
                    Collider[] recheck = Physics.OverlapBox(t.transform.position + (placedBounds.center - t.transform.position), placedHalf * 1.06f, t.transform.rotation);
                    bool reCollision = false;
                    foreach (var c in recheck)
                    {
                        if (c == null) continue;
                        if (c.transform.IsChildOf(t.transform)) continue;
                        if (c.CompareTag("Wall") || (wallLayer >= 0 && c.gameObject.layer == wallLayer))
                        {
                            reCollision = true;
                            break;
                        }
                    }
                    if (reCollision)
                    {
                        Destroy(t);
                        continue;
                    }

                    // 成功
                    t.name = $"RandomTreasure_{cell.x}_{cell.y}";
                    spawned.Add(t);
                    used.Add(cell);
                    placed = true;
                } // while placed attempts

                if (!placed)
                {
                    Debug.LogWarning($"[ElementGenerator] cell={cell} で宝箱を配置できませんでした（複数試行の末）");
                }
            } // for each treasure
        } // else safeFloorList

        Debug.Log("[ElementGenerator] GenerateFromMap 完了");
    }


    // =========================================================================
    // ComputePenetration を使ってめり込みを押し戻す厳格版（親子Collider対応）
    // =========================================================================
    private bool TryFindValidTreasurePosition_Strict_ComputePenetration(
        GameObject prefab,
        Vector3 basePos,
        Quaternion rot,
        out Vector3 safePos,
        int wallLayer = -1)
    {
        safePos = basePos;

        // 仮生成してコライダーとレンダラから合成Boundsを取得
        GameObject preview = Instantiate(prefab, basePos, rot);
        preview.SetActive(false);

        Physics.SyncTransforms();

        Collider[] previewColliders = preview.GetComponentsInChildren<Collider>();
        Renderer[] previewRenderers = preview.GetComponentsInChildren<Renderer>();

        if ((previewColliders == null || previewColliders.Length == 0) &&
            (previewRenderers == null || previewRenderers.Length == 0))
        {
            Destroy(preview);
            return false;
        }

        // 合成 bounds（ワールド座標）
        Bounds combinedBounds;
        bool hasBounds = false;

        if (previewColliders != null && previewColliders.Length > 0)
        {
            combinedBounds = previewColliders[0].bounds;
            for (int i = 1; i < previewColliders.Length; i++) combinedBounds.Encapsulate(previewColliders[i].bounds);
            hasBounds = true;
        }
        else
        {
            combinedBounds = previewRenderers[0].bounds;
            for (int i = 1; i < previewRenderers.Length; i++) combinedBounds.Encapsulate(previewRenderers[i].bounds);
            hasBounds = true;
        }

        if (!hasBounds)
        {
            Destroy(preview);
            return false;
        }

        Vector3 centerOffset = combinedBounds.center - preview.transform.position; // world center offset relative to transform pos
        Vector3 halfExtents = combinedBounds.extents;

        // 探索オフセット（周辺をチェック）
        Vector3[] offsets = new Vector3[]
        {
            Vector3.zero,
            new Vector3(0.08f, 0, 0),
            new Vector3(-0.08f, 0, 0),
            new Vector3(0, 0, 0.08f),
            new Vector3(0, 0, -0.08f),
            new Vector3(0.12f, 0, 0.12f),
            new Vector3(-0.12f, 0, 0.12f),
            new Vector3(0.12f, 0, -0.12f),
            new Vector3(-0.12f, 0, -0.12f),
        };

        float innerMargin = 0.96f;
        float outerMargin = 1.14f;

        int wallLayerIndex = wallLayer;
        int wallMask = (wallLayerIndex >= 0) ? (1 << wallLayerIndex) : ~0;

        const int maxPenetrationResolveIters = 6;

        Physics.SyncTransforms();

        foreach (var off in offsets)
        {
            Vector3 testTransformPos = basePos + off;
            preview.transform.position = testTransformPos;
            preview.transform.rotation = rot;
            Physics.SyncTransforms();

            Vector3 overlapCenter = testTransformPos + centerOffset;

            // 1) 内側 Overlap（ほぼ本体サイズ）
            Collider[] colsInner = Physics.OverlapBox(overlapCenter, halfExtents * innerMargin, rot);
            bool innerOk = true;
            foreach (var hit in colsInner)
            {
                if (hit == null) continue;
                if (hit.transform.IsChildOf(preview.transform)) continue;

                if (hit.CompareTag("Wall") || (wallLayerIndex >= 0 && hit.gameObject.layer == wallLayerIndex))
                {
                    innerOk = false;
                    break;
                }
            }
            if (!innerOk) continue;

            // 2) 外側 Overlap（広め）
            Collider[] colsOuter = Physics.OverlapBox(overlapCenter, halfExtents * outerMargin, rot);
            bool outerOk = true;
            foreach (var hit in colsOuter)
            {
                if (hit == null) continue;
                if (hit.transform.IsChildOf(preview.transform)) continue;

                if (hit.CompareTag("Wall") || (wallLayerIndex >= 0 && hit.gameObject.layer == wallLayerIndex))
                {
                    outerOk = false;
                    break;
                }
            }
            if (!outerOk) continue;

            // 3) ComputePenetration による押し出し試行
            List<Collider> overlappedWalls = new List<Collider>();
            foreach (var h in colsOuter)
            {
                if (h == null) continue;
                if (h.transform.IsChildOf(preview.transform)) continue;
                if (h.CompareTag("Wall") || (wallLayerIndex >= 0 && h.gameObject.layer == wallLayerIndex))
                    overlappedWalls.Add(h);
            }

            bool penetrationResolved = true;

            if (overlappedWalls.Count > 0 && previewColliders != null && previewColliders.Length > 0)
            {
                int iter = 0;
                while (iter < maxPenetrationResolveIters && overlappedWalls.Count > 0)
                {
                    iter++;
                    Vector3 totalMove = Vector3.zero;

                    foreach (var pcol in previewColliders)
                    {
                        if (pcol == null) continue;
                        foreach (var wcol in overlappedWalls)
                        {
                            if (wcol == null) continue;
                            if (wcol.isTrigger) continue;
                            if (pcol.isTrigger) continue;

                            Vector3 direction;
                            float distance;
                            bool ok = Physics.ComputePenetration(
                                pcol, pcol.transform.position, pcol.transform.rotation,
                                wcol, wcol.transform.position, wcol.transform.rotation,
                                out direction, out distance);

                            if (ok && distance > 0.0001f)
                            {
                                totalMove += direction * distance;
                            }
                        }
                    }

                    if (totalMove.sqrMagnitude < 1e-6f) break;

                    // 少しスケールダウンして適用（安定化）
                    Vector3 moveThisStep = totalMove * 0.9f;
                    preview.transform.position += moveThisStep;
                    Physics.SyncTransforms();

                    // 再計測
                    overlappedWalls.Clear();
                    Collider[] newOuter = Physics.OverlapBox(preview.transform.position + centerOffset, halfExtents * outerMargin, preview.transform.rotation);
                    foreach (var h2 in newOuter)
                    {
                        if (h2 == null) continue;
                        if (h2.transform.IsChildOf(preview.transform)) continue;
                        if (h2.CompareTag("Wall") || (wallLayerIndex >= 0 && h2.gameObject.layer == wallLayerIndex))
                            overlappedWalls.Add(h2);
                    }

                    if (overlappedWalls.Count == 0) break;
                }

                if (overlappedWalls.Count > 0)
                    penetrationResolved = false;
            }

            if (!penetrationResolved) continue;

            // 4) 微小オフセットチェック（1cm刻み）
            bool microOk = true;
            for (int dx = -1; dx <= 1 && microOk; dx++)
            {
                for (int dz = -1; dz <= 1 && microOk; dz++)
                {
                    Vector3 microCenter = preview.transform.position + centerOffset + new Vector3(dx * 0.01f, 0, dz * 0.01f);
                    Collider[] microCols = Physics.OverlapBox(microCenter, halfExtents * innerMargin, rot);
                    foreach (var hit in microCols)
                    {
                        if (hit == null) continue;
                        if (hit.transform.IsChildOf(preview.transform)) continue;
                        if (hit.CompareTag("Wall") || (wallLayerIndex >= 0 && hit.gameObject.layer == wallLayerIndex))
                        {
                            microOk = false;
                            break;
                        }
                    }
                }
            }
            if (!microOk) continue;

            // すべて OK
            safePos = preview.transform.position;
            Destroy(preview);
            return true;
        }

        Destroy(preview);
        return false;
    }


    // -------------------------------------------------------------------------
    //    Feature spawn
    // -------------------------------------------------------------------------
    // spawn helpers
    private void SpawnFeature(GameObject prefab, int x, int y, string baseName)
    {
        if (prefab == null) return;
        Vector3 p = new Vector3(x * cellSize, 0.5f, y * cellSize);
        GameObject o = Instantiate(prefab, p, Quaternion.identity, elementParent);
        o.name = $"{baseName}_{x}_{y}";
        spawned.Add(o);


        Unit unit = o.GetComponent<Unit>();
        if (unit != null)
        {
            // グリッド位置を設定
            unit.gridPos = new Vector2Int(x, y);

            // occupant登録
            GridBlock block = GridManager.Instance.GetBlock(unit.gridPos);
            if (block != null)
            {
                block.occupantUnit = unit;
            }

            // TurnManagerに登録（重複チェック付き）
            if (TurnManager.Instance != null && !TurnManager.Instance.allUnits.Contains(unit))
            {
                TurnManager.Instance.allUnits.Add(unit);
            }
        }
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
