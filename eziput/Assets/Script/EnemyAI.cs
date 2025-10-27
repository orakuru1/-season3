using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public IEnumerator ExecuteEnemyTurn()
    {
        // プレイヤーを探す
        var players = FindObjectsOfType<Unit>()
            .Where(u => u.team == Unit.Team.Player)
            .ToList();

        if (players.Count == 0) yield break;

        // 一番近いプレイヤーを見つける
        Unit target = players
            .OrderBy(p => Vector2Int.Distance(unit.gridPos, p.gridPos))
            .FirstOrDefault();

        float distance = Vector2Int.Distance(unit.gridPos, target.gridPos);

        // 🔹 距離チェック
        if (distance <= 5f) // 5マス以内なら行動
        {
            if (distance <= unit.status.attackRange)
            {

                // 攻撃（仮）
                Debug.Log($"{unit.name} が {target.name} を攻撃！");
                // 攻撃アニメーションとか入れるならここ
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                // プレイヤーに近づく
                var path = GridManager.Instance.FindPath(unit.gridPos, target.gridPos);
                if (path != null && path.Count > 1)
                {
                    var next = path[1]; // 次のマス
                    yield return StartCoroutine(unit.MoveTowardNearestPlayerCoroutine());
                }
            }
        }
        else
        {
            yield return StartCoroutine(RandomWander());
        }
    }
    private IEnumerator RandomWander()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        dirs = dirs.OrderBy(x => Random.value).ToArray();

        foreach (var dir in dirs)
        {
            Vector2Int candidate = unit.gridPos + dir;

            // マップ内かチェック
            var block = GridManager.Instance.GetBlock(candidate);
            if (block == null) continue;

            // 通行可能かつ空いているマスを確認
            if (!block.isWalkable || block.occupantUnit != null)
                continue;

            // 経路探索
            var path = GridManager.Instance.FindPath(unit.gridPos, candidate);
            if (path == null || path.Count < 2)
                continue;

            var nextBlock = path[1];

            // ✅ ここで最終確認：移動直前に誰かがそのマスを取っていないかチェック
            if (nextBlock.occupantUnit != null)
            {
                Debug.Log($"{unit.name} は目的地が塞がれたため再抽選します。");
                yield return new WaitForSeconds(0.1f);
                continue; // 別方向を試す
            }

            // 🔹 マスを一時的に「予約」扱いにして他の敵に取られないようにする
            nextBlock.occupantUnit = unit;

            // 実際に移動（アニメーション付き）
            yield return StartCoroutine(unit.MoveTowardNearestCoroutine(candidate));

            yield break;
        }

        // どこにも行けない場合
        Debug.Log($"{unit.name} は動けないので待機。");
        yield return new WaitForSeconds(0.2f);
    }

    // 攻撃できるかチェック
    public bool CanAttackPlayer()
    {
        var players = FindObjectsOfType<Unit>()
            .Where(u => u.team == Unit.Team.Player)
            .ToList();
        if (players.Count == 0) return false;

        Unit target = players
            .OrderBy(p => Vector2Int.Distance(unit.gridPos, p.gridPos))
            .FirstOrDefault();

        float distance = Vector2Int.Distance(unit.gridPos, target.gridPos);
        return distance <= unit.status.attackRange;
    }

    // 攻撃実行（攻撃アニメーションなど入れる場所）
    public IEnumerator AttackNearestPlayer()
    {
        var players = FindObjectsOfType<Unit>()
            .Where(u => u.team == Unit.Team.Player)
            .ToList();
        if (players.Count == 0) yield break;

        Unit target = players
            .OrderBy(p => Vector2Int.Distance(unit.gridPos, p.gridPos))
            .FirstOrDefault();
        yield return StartCoroutine(unit.AttackNearestTarget()); 

        Debug.Log($"{unit.name} が {target.name} を攻撃！");
        // ここにアニメーション処理を入れる
        //yield return new WaitForSeconds(2.5f);
    }

}
