using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameObject> OnPlayerSpawned;

    // 現在選択されているルート
    public RouteType CurrentRoute { get; private set; } = RouteType.Safe;

    public GameObject playerPrefab;

    public ElementGenerator eg;

    void Start()
    {
        StartCoroutine(SpawnPlayerAfterGenerate());
    }

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    public void SelectSafeRoute()
    {
        CurrentRoute = RouteType.Safe;
        Debug.Log("Safeルートを選択しました。");
        SceneManager.LoadScene("Tougou2");
    }

    public void SelectDangerRoute()
    {
        CurrentRoute = RouteType.Danger;
        Debug.Log("Dangerルートを選択しました。");
        SceneManager.LoadScene("Tougou2");
    }

    public void LoadSelectScene()
    {
        SceneManager.LoadScene("Scene_Select");
    }

    private IEnumerator SpawnPlayerAfterGenerate()
    {
        // ElementGenerator が生成完了するまで待つ
        yield return new WaitUntil(() => ElementGeneratorFinished());

        if (eg != null && eg.EntranceFound)
        {
            StartCoroutine(SpawnPlayerLater(eg.EntranceWorldPos));
        }
    }

    private bool ElementGeneratorFinished()
    {
        // ElementGenerator がシーンに存在して、入口を生成し終わっている場合 true
        return (eg != null && eg.EntranceFound);
    }
    private IEnumerator SpawnPlayerLater(Vector3 worldPos)
    {
        SpawnPlayer(worldPos);
        // ★ GridBlock.Start() が走るまで 1 フレーム待つ
        yield return null;

        if (InputHandler.Instance != null)
            InputHandler.Instance.ClearHighlight();

    }

    private void SpawnPlayer(Vector3 worldPos)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab が設定されていません！");
            return;
        }
        GameObject playerObj = Instantiate(playerPrefab, worldPos, Quaternion.identity);
        OnPlayerSpawned?.Invoke(playerObj);
        Unit unit = playerObj.GetComponent<Unit>();
        if (unit != null)
        {
            unit.gridPos = new Vector2Int(
                Mathf.RoundToInt(worldPos.x / 2),
                Mathf.RoundToInt(worldPos.z / 2)
            );

            var block = GridManager.Instance.GetBlock(unit.gridPos);
            if (block != null)
                block.occupantUnit = unit;

            if (!TurnManager.Instance.allUnits.Contains(unit))
                TurnManager.Instance.allUnits.Insert(0, unit);
        }
        // ★ カメラのターゲットにする
        if (CameraFollowAdvanced.Instance != null)
        {
            CameraFollowAdvanced.Instance.SetTarget(playerObj.transform);
        }

        Debug.Log("Player Spawned at Entrance");

}


}
