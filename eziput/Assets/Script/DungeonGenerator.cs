using UnityEngine;

/// <summary>
/// Safe／Dangerルートに応じてダンジョン全体の構造を生成する
/// </summary>
[System.Serializable]
public class DungeonSettings
{
    public int mapWidth = 50;
    public int mapHeight = 50;
    public int roomCount = 8;
    public int enemyCount = 5;
    public int treasureCount = 3;
    public int trapCount = 2;
}

public class DungeonGenerator : MonoBehaviour
{
    public DungeonSettings safeSettings = new DungeonSettings
    {
        mapWidth = 40,
        mapHeight = 40,
        roomCount = 6,
        enemyCount = 3,
        treasureCount = 5,
        trapCount = 1
    };

    public DungeonSettings dangerSettings = new DungeonSettings
    {
        mapWidth = 60,
        mapHeight = 60,
        roomCount = 10,
        enemyCount = 10,
        treasureCount = 2,
        trapCount = 5
    };

    private DungeonSettings activeSettings;
    private int[,] mapData;

    void Start()
    {
        // Safe/Dangerの切り替え
        var route = GameManager.Instance.currentRoute;
        activeSettings = route == RouteType.Safe ? safeSettings : dangerSettings;

        Debug.Log(route == RouteType.Safe ? "🔵 Safeルート用ダンジョンを生成中..." : "🔴 Dangerルート用ダンジョンを生成中...");

        GenerateDungeon();
        NotifyElementGenerator();
    }

    void GenerateDungeon()
    {
        mapData = new int[activeSettings.mapHeight, activeSettings.mapWidth];

        // 初期化（全て壁）
        for (int y = 0; y < activeSettings.mapHeight; y++)
        {
            for (int x = 0; x < activeSettings.mapWidth; x++)
            {
                mapData[y, x] = 1;
            }
        }

        // ランダム部屋生成
        for (int i = 0; i < activeSettings.roomCount; i++)
        {
            int roomW = Random.Range(4, 8);
            int roomH = Random.Range(4, 8);
            int roomX = Random.Range(1, activeSettings.mapWidth - roomW - 1);
            int roomY = Random.Range(1, activeSettings.mapHeight - roomH - 1);

            for (int y = roomY; y < roomY + roomH; y++)
            {
                for (int x = roomX; x < roomX + roomW; x++)
                {
                    mapData[y, x] = 0; // 床
                }
            }
        }

        // 部屋間の通路を掘る
        for (int i = 0; i < activeSettings.roomCount - 1; i++)
        {
            int x1 = Random.Range(1, activeSettings.mapWidth - 1);
            int y1 = Random.Range(1, activeSettings.mapHeight - 1);
            int x2 = Random.Range(1, activeSettings.mapWidth - 1);
            int y2 = Random.Range(1, activeSettings.mapHeight - 1);

            DigCorridor(x1, y1, x2, y2);
        }

        Debug.Log($"✅ マップ生成完了 ({activeSettings.mapWidth}x{activeSettings.mapHeight})");
    }

    void DigCorridor(int x1, int y1, int x2, int y2)
    {
        while (x1 != x2)
        {
            mapData[y1, x1] = 0;
            x1 += x1 < x2 ? 1 : -1;
        }

        while (y1 != y2)
        {
            mapData[y1, x1] = 0;
            y1 += y1 < y2 ? 1 : -1;
        }
    }

    void NotifyElementGenerator()
    {
        ElementGenerator eg = FindObjectOfType<ElementGenerator>();
        if (eg != null)
        {
            eg.GenerateFromMap(mapData, activeSettings);
            Debug.Log("📡 ElementGenerator にマップデータを送信しました");
        }
        else
        {
            Debug.LogWarning("⚠️ ElementGenerator がシーンに存在しません");
        }
    }
}
