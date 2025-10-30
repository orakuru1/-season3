﻿using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private Unit currentPlayerUnit;
    private GridBlock highlightedBlock;
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

        var player = currentPlayerUnit as PlayerUnit;
        if (player == null) return;

        // === 移動・攻撃中は方向転換禁止 ===
        // === 攻撃・移動中は方向転換だけ禁止 ===
        bool canRotate = !(player.isMoving || player.animationController.animationState.isAttacking);

        // 攻撃範囲表示中などで方向転換を許可したい場合
        if (canRotate && player.isShowingAttackRange)
        {
            HandleDirectionChange(player);
        }
        else if (canRotate)
        {
            HandleDirectionChange(player);
        }


        // === Enterで移動 or スキル発動 ===
        if (Input.GetKeyDown(KeyCode.Return))
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
        if (player.attackSkills != null)
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
        bool dirChanged = false;
        if (Input.GetKeyDown(KeyCode.W)) { player.facingDir = Vector2Int.up; dirChanged = true; }
        if (Input.GetKeyDown(KeyCode.S)) { player.facingDir = Vector2Int.down; dirChanged = true; }
        if (Input.GetKeyDown(KeyCode.A)) { player.facingDir = Vector2Int.left; dirChanged = true; }
        if (Input.GetKeyDown(KeyCode.D)) { player.facingDir = Vector2Int.right; dirChanged = true; }

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

    public void UpdateHighlight()
    {
        ClearHighlight();
        if (currentPlayerUnit == null || GridManager.Instance == null)
            return;

        Vector2Int targetPos = currentPlayerUnit.gridPos + currentPlayerUnit.facingDir;
        GridBlock block = GridManager.Instance.GetBlock(targetPos);
        if (block != null)
        {
            block.SetHighlight(Color.blue);
            highlightedBlock = block;
        }
    }

    void ClearHighlight()
    {
        if (highlightedBlock != null)
        {
            highlightedBlock.ResetHighlight();
            highlightedBlock = null;
        }
    }
}
