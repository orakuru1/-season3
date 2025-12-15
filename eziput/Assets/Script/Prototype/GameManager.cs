using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static event Action<GameObject> OnPlayerSpawned;

    // =======================
    // ステージ & ルート
    // =======================
    public int CurrentStage { get; private set; } = 1;
    public const int MaxStage = 4;
    public RouteType CurrentRoute { get; private set; } = RouteType.Safe;

    // =======================
    // ステージ条件
    // =======================
    public bool IsBossDefeated { get; set; }
    public bool IsItemCrafted { get; set; }

    // =======================
    // プレイヤー生成
    // =======================
    public GameObject playerPrefab;
    public ElementGenerator eg;

    // =======================
    // HP UI
    // =======================
    public Slider hpSlider;
    public Text hptext;

    // =======================
    // Game Clear UI（Stage1）
    // =======================
    [Header("Game Clear UI")]
    public CanvasGroup gameClearCanvasGroup;
    public float gameClearFadeTime = 1.5f;
    public AudioClip stage1ClearSE;

    // =======================
    // Game Over UI
    // =======================
    [Header("Game Over UI")]
    public CanvasGroup gameOverCanvasGroup;
    public float gameOverFadeTime = 1.5f;
    public AudioClip gameOverSE;

    // =======================
    // BGM
    // =======================
    [Header("BGM")]
    public AudioSource bgmSource;

    [Header("Game Over BGM Settings")]
    public AudioClip gameOverBGM;
    public float gameOverBgmTargetVolume = 0.4f;
    public float gameOverBgmFadeInTime = 2.0f;
    public float silenceDurationBeforeGameOverBgm = 0.5f;

    // =======================
    // 初期化
    // =======================
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitCanvasGroup(gameClearCanvasGroup);
        InitCanvasGroup(gameOverCanvasGroup);

        StartCoroutine(SpawnPlayerAfterGenerate());
    }

    private void InitCanvasGroup(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.gameObject.SetActive(false);
    }

    // =======================
    // ルート選択
    // =======================
    public void SelectSafeRoute()
    {
        CurrentRoute = RouteType.Safe;
        SceneManager.LoadScene("Tougou2");
    }

    public void SelectDangerRoute()
    {
        CurrentRoute = RouteType.Danger;
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
        if (!IsBossDefeated || !IsItemCrafted)
            return;

        if (CurrentStage == 1)
            ShowStage1GameClear();
        else
            GameClear();
    }

    // =======================
    // Stage1 Game Clear
    // =======================
    private void ShowStage1GameClear()
    {
        DisableInput();
        StopBgm();

        if (stage1ClearSE != null)
            AudioSource.PlayClipAtPoint(stage1ClearSE, Vector3.zero);

        StartCoroutine(FadeInCanvas(gameClearCanvasGroup, gameClearFadeTime));
    }

    public void OnNextStageButton()
    {
        ProceedToNextStage();
    }

    // =======================
    // 通常 Game Clear
    // =======================
    private void GameClear()
    {
        StartCoroutine(GameClearRoutine());
    }

    private IEnumerator GameClearRoutine()
    {
        DisableInput();
        yield return new WaitForSeconds(1f);
        ProceedToNextStage();
    }

    // =======================
    // Game Over
    // =======================
    public void GameOver()
    {
        DisableInput();
        StopBgm();

        if (gameOverSE != null)
            AudioSource.PlayClipAtPoint(gameOverSE, Vector3.zero);

        StartCoroutine(GameOverBgmRoutine());
        StartCoroutine(FadeInCanvas(gameOverCanvasGroup, gameOverFadeTime));
    }

    public void GameOvedUI()
    {
        gameOverCanvasGroup.alpha = 0f;
    }

    private IEnumerator GameOverBgmRoutine()
    {
        yield return new WaitForSeconds(silenceDurationBeforeGameOverBgm);

        if (bgmSource == null || gameOverBGM == null)
            yield break;

        bgmSource.clip = gameOverBGM;
        bgmSource.loop = true;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float t = 0f;
        while (t < gameOverBgmFadeInTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, gameOverBgmTargetVolume, t / gameOverBgmFadeInTime);
            yield return null;
        }

        bgmSource.volume = gameOverBgmTargetVolume;
    }

    public void OnRetryButton()
    {
        StopBgm();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnReturnToTitle()
    {
        StopBgm();
        SceneManager.LoadScene("TitleScene");
    }

    // =======================
    // ステージ遷移
    // =======================
    private void ProceedToNextStage()
    {
        CurrentStage++;

        IsBossDefeated = false;
        IsItemCrafted = false;

        if (CurrentStage > MaxStage)
        {
            SceneManager.LoadScene("GameCompleteScene");
        }
        else if (CurrentStage == 4)
        {
            SceneManager.LoadScene("FinalStage");
        }
        else
        {
            SceneManager.LoadScene("Scene_Select");
        }
    }

    // =======================
    // 共通処理
    // =======================
    private void DisableInput()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.enabled = false;
    }

    private void StopBgm()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    private IEnumerator FadeInCanvas(CanvasGroup cg, float duration)
    {
        if (cg == null) yield break;

        cg.gameObject.SetActive(true);
        cg.alpha = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    // =======================
    // プレイヤー生成
    // =======================
    private IEnumerator SpawnPlayerAfterGenerate()
    {
        yield return new WaitUntil(() => eg != null && eg.EntranceFound);
        SpawnPlayer(eg.EntranceWorldPos);
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
            unit.hpSlider = hpSlider;
            unit.hptext = hptext;
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
