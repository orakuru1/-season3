using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private Unit currentPlayerUnit;
    private GridBlock highlightedBlock;
    private bool st;
    [SerializeField] float longPressThreshold = 0.35f;
    [SerializeField] float autoMoveInterval = 0.15f;

    float enterPressTime = 0f;
    bool isAutoMoving = false;
    Coroutine autoMoveCoroutine;

    public static InputHandler Instance { get; internal set; }

    private void OnEnable() => TurnManager.OnTurnStart += OnTurnStart;
    private void OnDisable() => TurnManager.OnTurnStart -= OnTurnStart;

    private void OnTurnStart(Unit unit)
    {
        currentPlayerUnit = unit.team == Unit.Team.Player ? unit : null;
        UpdateHighlight();
    }
    private void Update()
    {

        if (TurnManager.Instance.currentTeam != Unit.Team.Player) return;
        if (currentPlayerUnit == null) return;
        if (TurnManager.Instance.gameState != TurnManager.GameState.Playing) return;

        var player = currentPlayerUnit as PlayerUnit;
        if (player == null) return;
        bool isBusy =
        player.isMoving ||
        player.animationController.animationState.isAttacking ||
        player.animationController.animationState.isBuffing ||
        player.animationController.animationState.isDebuffing ||
        player.animationController.animationState.isHiling ||
        player.isEvent;

        // === 移動・攻撃中は方向転換禁止 ===
        // === 攻撃・移動中は方向転換だけ禁止 ===
        bool canRotate = !(player.isMoving || player.animationController.animationState.isAttacking || player.animationController.animationState.isBuffing || player.animationController.animationState.isDebuffing || player.animationController.animationState.isHiling || GodManeger.Instance.isGodDescrip || player.isEvent);

        // 攻撃範囲表示中などで方向転換を許可したい場合
        if (canRotate && player.isShowingAttackRange)
        {
            HandleDirectionChange(player);
        }
        else if (canRotate)
        {
            HandleDirectionChange(player);
        }

        // === Enter押下開始 ===
        if (Input.GetKeyDown(KeyCode.Return) && !player.isEvent)
        {
            enterPressTime = Time.time;
        }

        // === Enter押し続け ===
        if (Input.GetKey(KeyCode.Return) && !player.isEvent)
        {
            if (!isAutoMoving &&
                Time.time - enterPressTime >= longPressThreshold &&
                !IsEnemyAhead(player))
            {   // 長押し開始
                autoMoveCoroutine = StartCoroutine(AutoMove(player));
            }
        }

        // === Enter離した ===
        if (Input.GetKeyUp(KeyCode.Return))
        {
            StopAutoMove();
        }


        // === Enterで移動 or スキル発動 ===
        if (Input.GetKeyDown(KeyCode.Return) && !player.isEvent)
        {
            if (player == null) return;

            if (player.selectedSkill != null && player.isShowingAttackRange)
            {
                // スキル発動
                Debug.Log("スキル発動: " + player.selectedSkill.skillName);
                StartCoroutine(player.UseSkill(player.selectedSkill));
            }
            else
            {
                // 通常移動
                Debug.Log("通常移動を試みます");
                DoMove(player);
            }

            return;
        }



        // === スキル選択 ===
        if (!isBusy && player.attackSkills != null)
        {
            foreach (var skill in player.attackSkills)
            {
                if (Input.GetKeyDown(skill.triggerKey))
                {
                    player.selectedSkill = skill;
                    player.status.attackPattern = skill.attackPattern;
                    ClearHighlight();
                    player.ShowAttackRange();
                    return;
                }
            }
        }

        // === ESCでキャンセル ===
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            player.ClearAttackRange();
            player.selectedSkill = null;
            ClearHighlight();
        }
    }

    private void HandleDirectionChange(PlayerUnit player)
    {   
        st = true;
        bool dirChanged = false;
        if (Input.GetKeyDown(KeyCode.W)) { player.facingDir = Vector2Int.up; dirChanged = true; }
        if(CameraSwitcher.Instance.isTopView)
        {
            if (Input.GetKeyDown(KeyCode.A)) { player.facingDir = Vector2Int.left; dirChanged = true; }
            if (Input.GetKeyDown(KeyCode.S)) { player.facingDir = Vector2Int.down; dirChanged = true; }
            if (Input.GetKeyDown(KeyCode.D)) { player.facingDir = Vector2Int.right; dirChanged = true; }
        }
        else
        {
            //１人称の時と天カメの時で操作方法を変えたほうが良いかもしれない。めっちゃやりにくい。
        
            if (Input.GetKeyDown(KeyCode.S))
            {
                // 180度反転
                player.facingDir = -player.facingDir;
                dirChanged = true;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                // 左回転（90度）
                player.facingDir = new Vector2Int(
                    -player.facingDir.y,
                    player.facingDir.x
                );
                dirChanged = true;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                // 右回転（90度）
                player.facingDir = new Vector2Int(
                    player.facingDir.y,
                    -player.facingDir.x
                );
                dirChanged = true;
            }
        }

        if (!dirChanged) return;

        Vector3 dir3D = new Vector3(player.facingDir.x, 0, player.facingDir.y);
        player.transform.rotation = Quaternion.LookRotation(dir3D);
        UpdateHighlight();

        if (player.isShowingAttackRange)
            player.ShowAttackRange();
    }

    private void DoMove(PlayerUnit player)
    {
        ClearHighlight();
        if (player.facingDir == Vector2Int.up) player.TryMove(Vector2Int.up);
        if (player.facingDir == Vector2Int.down) player.TryMove(Vector2Int.down);
        if (player.facingDir == Vector2Int.left) player.TryMove(Vector2Int.left);
        if (player.facingDir == Vector2Int.right) player.TryMove(Vector2Int.right);
    }

    List<Vector2Int> GetForwardCheckCells(
    Vector2Int origin,
    Vector2Int dir,
    int forward = 2,
    int halfWidth = 1
)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        Vector2Int right = new Vector2Int(dir.y, -dir.x); // 90度回転

        for (int f = 1; f <= forward; f++)
        {
            Vector2Int center = origin + dir * f;

            for (int w = -halfWidth; w <= halfWidth; w++)
            {
                cells.Add(center + right * w);
            }
        }

        return cells;
    }

    bool IsEnemyAhead(PlayerUnit player)
    {
        var gm = GridManager.Instance;
        var cells = GetForwardCheckCells(
            player.gridPos,
            player.facingDir,
            forward: 5,
            halfWidth: 1
        );

        foreach (var c in cells)
        {
            var block = gm.GetBlock(c);
            if (block == null) continue;

            if (block.occupantUnit != null &&
                block.occupantUnit.team == Unit.Team.Enemy)
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator AutoMove(PlayerUnit player)
    {
        isAutoMoving = true;

        while (Input.GetKey(KeyCode.Return))
        {
            DoMove(player);
            yield return new WaitForSeconds(autoMoveInterval);

            // 敵が近づいたら即停止
            if (IsEnemyAhead(player))
                break;
        }

        StopAutoMove();
    }


    void StopAutoMove()
    {
        if (autoMoveCoroutine != null)
        {
            StopCoroutine(autoMoveCoroutine);
            autoMoveCoroutine = null;
        }
        isAutoMoving = false;
    }

    public void UpdateHighlight()
    {
        if (st)
        {
            ClearHighlight();
            if (currentPlayerUnit == null || GridManager.Instance == null)
                return;

            Vector2Int targetPos = currentPlayerUnit.gridPos + currentPlayerUnit.facingDir;
            GridBlock block = GridManager.Instance.GetBlock(targetPos);
            if (block != null && GridManager.Instance.IsWalkable(targetPos))
            {
                block.SetHighlight(Color.blue);
                highlightedBlock = block;
            }

            if (block != null && block.trapType != TrapType.None && DamageTextManager.Instance.isView == false && LogManager.Instance != null)
            {
                LogManager.Instance.AddLog("……床に違和感がある");
            }
            
        }
    }

    public void ClearHighlight()
    {
        if (highlightedBlock != null)
        {
            highlightedBlock.ResetHighlight();
            highlightedBlock = null;
        }
    }
}
