using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameObject> OnPlayerSpawned;

    // -----------------------
    // ステージ & ルート管理
    // -----------------------
    public int CurrentStage { get; private set; } = 1;
    public const int MaxStage = 4;  // 最終ステージ（4）

    public RouteType CurrentRoute { get; private set; } = RouteType.Safe;

    // -----------------------
    // ステージクリア条件
    // -----------------------
    public bool IsBossDefeated { get; set; } = false;
    public bool IsItemCrafted { get; set; } = false;

    // -----------------------
    // プレイヤー & 入口関連
    // -----------------------
    public GameObject playerPrefab;
    public ElementGenerator eg;

    // =======================
    // 初期化
    // =======================
    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(SpawnPlayerAfterGenerate());
    }

    // =======================
    // ルート選択
    // =======================
    public void SelectSafeRoute()
    {
        CurrentRoute = RouteType.Safe;
        Debug.Log($"【Stage{CurrentStage}】Safeルートを選択");
        SceneManager.LoadScene("Tougou2");
    }

    public void SelectDangerRoute()
    {
        CurrentRoute = RouteType.Danger;
        Debug.Log($"【Stage{CurrentStage}】Dangerルートを選択");
        SceneManager.LoadScene("Tougou2");
    }

    public void LoadSelectScene()
    {
        SceneManager.LoadScene("Scene_Select");
    }

    // =======================
    // ステージクリア判定
    // =======================
    public void TryStageClear()
    {
        if (!IsBossDefeated)
        {
            Debug.Log("ボスを倒していません！");
            return;
        }

        if (!IsItemCrafted)
        {
            Debug.Log("必要なアイテムが完成していません！");
            return;
        }

        Debug.Log($"ステージ {CurrentStage} クリア！");
        GameClear();
    }

    // =======================
    // ゲームクリア処理
    // =======================
    public void GameClear()
    {
        StartCoroutine(GameClearRoutine());
    }

    private IEnumerator GameClearRoutine()
    {
        Debug.Log("クリア演出開始");

        // 操作停止（必要なら）
        if (InputHandler.Instance != null)
            InputHandler.Instance.enabled = false;

        // UIフェードやSEなどを挟むならここ
        yield return new WaitForSeconds(1.0f);

        // 次ステージへ
        ProceedToNextStage();
    }

    // =======================
    // 次のステージへ進む
    // =======================
    public void ProceedToNextStage()
    {
        CurrentStage++;

        if (CurrentStage > MaxStage)
        {
            Debug.Log("全ステージクリア！");
            SceneManager.LoadScene("GameCompleteScene");
            return;
        }

        // 条件リセット
        IsBossDefeated = false;
        IsItemCrafted = false;

        if (CurrentStage == 4)
        {
            // 最終ステージは単独マップ
            Debug.Log("最終ステージへ移行");
            SceneManager.LoadScene("FinalStage");
        }
        else
        {
            // 次のステージの Safe / Danger を選択
            Debug.Log($"ステージ{CurrentStage}へ → ルート選択へ戻る");
            SceneManager.LoadScene("Scene_Select");
        }
    }

    // =======================
    // プレイヤー生成関係
    // =======================
    private IEnumerator SpawnPlayerAfterGenerate()
    {
        // ElementGeneratorが入口を生成するまで待つ
        yield return new WaitUntil(() => ElementGeneratorFinished());

        if (eg != null && eg.EntranceFound)
        {
            StartCoroutine(SpawnPlayerLater(eg.EntranceWorldPos));
        }
    }

    private bool ElementGeneratorFinished()
    {
        return (eg != null && eg.EntranceFound);
    }

    private IEnumerator SpawnPlayerLater(Vector3 worldPos)
    {
        SpawnPlayer(worldPos);

        // GridBlock.Start()が走るまで1フレーム待つ
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

        // カメラのターゲットに設定
        if (CameraFollowAdvanced.Instance != null)
        {
            CameraFollowAdvanced.Instance.SetTarget(playerObj.transform);
        }

        Debug.Log("Player Spawned at Entrance");
    }
}
