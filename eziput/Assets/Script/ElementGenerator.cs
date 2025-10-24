using UnityEngine;

/// <summary>
/// DungeonGenerator からマップ情報を受け取り、
/// 敵・罠・宝箱・ゴールなどを配置する
/// </summary>
public class ElementGenerator : MonoBehaviour
{
    public Transform stageParent;
    public GameObject enemyPrefab;
    public GameObject treasurePrefab;
    public GameObject trapPrefab;
    public GameObject goalLightPrefab;
    public float cellSize = 2.0f;

    private int[,] mapData;

    /// <summary>
    /// DungeonGenerator から呼ばれる生成関数
    /// </summary>
    public void GenerateFromMap(int[,] mapData, DungeonSettings settings)
    {
        this.mapData = mapData;
        ClearStage();

        // 床の生成
        for (int y = 0; y < mapData.GetLength(0); y++)
        {
            for (int x = 0; x < mapData.GetLength(1); x++)
            {
                if (mapData[y, x] == 0)
                {
                    Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    floor.transform.position = pos;
                    floor.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                    floor.transform.SetParent(stageParent);
                    floor.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
                }
            }
        }

        // 敵・宝箱・罠・ゴール配置
        for (int i = 0; i < settings.enemyCount; i++) SpawnAtRandom(enemyPrefab, "Enemy");
        for (int i = 0; i < settings.treasureCount; i++) SpawnAtRandom(treasurePrefab, "Treasure");
        for (int i = 0; i < settings.trapCount; i++) SpawnAtRandom(trapPrefab, "Trap");

        // ゴールは1つだけ
        SpawnAtRandom(goalLightPrefab, "Goal");

        Debug.Log("✅ オブジェクト配置完了（ルート設定反映）");
    }

    /// <summary>
    /// ステージの既存オブジェクトを削除
    /// </summary>
    void ClearStage()
    {
        if (stageParent != null)
        {
            foreach (Transform child in stageParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 通行可能な床の上にランダムで配置
    /// </summary>
    void SpawnAtRandom(GameObject prefab, string label)
    {
        if (prefab == null) return;

        int tries = 100;
        while (tries-- > 0)
        {
            int x = Random.Range(1, mapData.GetLength(1) - 1);
            int y = Random.Range(1, mapData.GetLength(0) - 1);
            if (mapData[y, x] == 0)
            {
                Vector3 pos = new Vector3(x * cellSize, 0.5f, y * cellSize);
                GameObject obj = Instantiate(prefab, pos, Quaternion.identity, stageParent);
                obj.name = $"{label}_{x}_{y}";
                break;
            }
        }
    }

    // --- 以下を ElementGenerator.cs の末尾に追加 ---
    public int GetMapWidth()
    {
        return mapData != null ? mapData.GetLength(1) : 0;
    }

    public int GetMapHeight()
    {
        return mapData != null ? mapData.GetLength(0) : 0;
    }
}
