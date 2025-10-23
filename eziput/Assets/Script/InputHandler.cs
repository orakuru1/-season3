// === File: InputHandler.cs ===
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Camera mainCamera;
    private Unit currentPlayerUnit;

    public enum Move
    {
        W,A,S,D
    }
    public Move move;
    private void OnEnable()
    {
        TurnManager.OnTurnStart += OnTurnStart;
    }
    private void OnDisable()
    {
        TurnManager.OnTurnStart -= OnTurnStart;
    }

    private void OnTurnStart(Unit unit)
    {
        if (unit.team == Unit.Team.Player)
            currentPlayerUnit = unit;
        else
            currentPlayerUnit = null;
    }

    private void Update()
    {
        if (TurnManager.Instance.currentTeam != Unit.Team.Player)
            return;
        if (currentPlayerUnit == null) return;

        // キーボード移動（1マス）
        if (Input.GetKeyDown(KeyCode.W)) { move = Move.W; }
        if (Input.GetKeyDown(KeyCode.S)) { move = Move.S; }
        if (Input.GetKeyDown(KeyCode.A)) { move = Move.A; }
        if (Input.GetKeyDown(KeyCode.D)) { move = Move.D; }
        if (Input.GetKeyDown(KeyCode.Return)) 
        {
            if(move == Move.W){currentPlayerUnit.TryMove(Vector2Int.up);}
            if(move == Move.A){currentPlayerUnit.TryMove(Vector2Int.left);}
            if(move == Move.S){currentPlayerUnit.TryMove(Vector2Int.down);}
            if(move == Move.D){currentPlayerUnit.TryMove(Vector2Int.right);}
        }
        /*
        if (currentPlayerUnit.attackSkills != null)
        {
            foreach (var skill in currentPlayerUnit.attackSkills)
            {
                if (Input.GetKeyDown(skill.triggerKey))
                {
                    currentPlayerUnit.status.attackPattern = skill.attackPattern;
                    currentPlayerUnit.ShowAttackRange();
                }
            }
        }

        // 攻撃キャンセルなどで範囲を消す場合
        if (Input.GetKeyDown(KeyCode.Escape))
            currentPlayerUnit.ClearAttackRange();
        /*
        // ダッシュ（Shift + dir）
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.W)) StartCoroutine(currentPlayerUnit.Dash(Vector2Int.up, 7));
            if (Input.GetKeyDown(KeyCode.S)) StartCoroutine(currentPlayerUnit.Dash(Vector2Int.down, 7));
            if (Input.GetKeyDown(KeyCode.A)) StartCoroutine(currentPlayerUnit.Dash(Vector2Int.left, 7));
            if (Input.GetKeyDown(KeyCode.D)) StartCoroutine(currentPlayerUnit.Dash(Vector2Int.right, 7));
        }
        /*
        // マウスクリック移動（経路探索利用）
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var block = hit.collider.GetComponent<GridBlock>();
                if (block != null)
                {
                    // 移動先が存在し経路があるか
                    var start = gridPositionToVector2Int(currentPlayerUnit.gridPos);
                    var path = GridManager.Instance.FindPath(currentPlayerUnit.gridPos, block.gridPos);
                    if (path != null && path.Count > 0)
                    {
                        // path を GridBlock リストとして渡す MoveAlongPath を使う
                        StartCoroutine(currentPlayerUnit.MoveAlongPath(path));
                    }
                }
            }
        }*/
    }

    private Vector2Int gridPositionToVector2Int(Vector2Int v) => v;
}