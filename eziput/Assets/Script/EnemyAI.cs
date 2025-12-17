using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Unit unit;
    private EnemyUnit enemyUnit;

    // === 中ボス用移動制限 ===
    public bool useMoveLimit = false;
    public Vector2Int moveCenter;   // 基準マス
    public int moveRadius = 0;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        enemyUnit = GetComponent<EnemyUnit>();
    }

    public IEnumerator ExecuteEnemyTurn()
    {
        if (IsOutsideMoveRange())
        {
            yield return StartCoroutine(ReturnToCenter());
            yield break;
        }
        var players = FindObjectsOfType<Unit>()
            .Where(u => u.team == Unit.Team.Player)
            .ToList();

        if (players.Count == 0) yield break;

        Unit target = players
            .OrderBy(p => Vector2Int.Distance(unit.gridPos, p.gridPos))
            .FirstOrDefault();

        float distance = Vector2Int.Distance(unit.gridPos, target.gridPos);

        //  視界チェック
        bool canSee = CanSeePlayer();

        // 視界内にプレイヤーがいる場合
        if (canSee)
        {
            if (distance <= 5f)   // 5マス以内なら行動
            {
                //  近づく or 攻撃
                if (distance <= unit.status.attackRange)
                {
                    Debug.Log($"{unit.name} が {target.name} を攻撃！");
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    var path = GridManager.Instance.FindPath(unit.gridPos, target.gridPos);
                    if (path != null && path.Count > 1)
                    {
                        yield return StartCoroutine(unit.MoveTowardNearestPlayerCoroutine());
                    }
                }

                //  追いかけて攻撃可能になった時の向き変更
                if (CanAttackPlayer())
                {
                    Vector3 dir3D = (target.transform.position - transform.position).normalized;
                    dir3D.y = 0;
                    if (dir3D.sqrMagnitude > 0.001f)
                    {
                        transform.rotation = Quaternion.LookRotation(dir3D);
                    }
                }

                yield break;
            }
        }

        // 視界にいない / 5マスより遠い → 索敵行動
        yield return StartCoroutine(RandomWander());
        yield return null;
    }

    private IEnumerator RandomWander()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        dirs = dirs.OrderBy(x => Random.value).ToArray();

        foreach (var dir in dirs)
        {
            Vector2Int candidate = unit.gridPos + dir;

            // ★ 中ボス移動制限チェック
            if (!IsWithinMoveRange(candidate))
                continue;

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
        Vector2Int dir = target.gridPos - unit.gridPos;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            unit.facingDir = new Vector2Int((int)Mathf.Sign(dir.x), 0);
        else
            unit.facingDir = new Vector2Int(0, (int)Mathf.Sign(dir.y));

        // 🔹 まず通常攻撃範囲内にいる場合
        if (distance <= unit.status.attackRange)
            return true;

        // 🔹 次にスキル使用可能か（EnemyUnitならTrySelectSkillを使う）
        if (enemyUnit != null && enemyUnit.TrySelectSkill())
        {
            // スキルが選択されたら true を返す（後で UseSelectedSkill で使用）
            return true;
        }

        return false;
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

        if (target != null)
            Debug.Log($"{unit.name} が {target.name} を攻撃！");
        else
            Debug.Log($"{unit.name} が 消滅した対象を攻撃しようとしました（既に倒れている）");

        // ここにアニメーション処理を入れる
        //yield return new WaitForSeconds(2.5f);
    }
    public bool CanSeePlayer()
    {
        var players = FindObjectsOfType<Unit>().Where(u => u.team == Unit.Team.Player);

        foreach (var p in players)
        {
            var line = GridLineUtility.GetLine(unit.gridPos, p.gridPos);

            foreach (var pos in line)
            {
                var block = GridManager.Instance.GetBlock(pos);

                // プレイヤー位置はスルー
                if (pos == p.gridPos)
                    return true;

                // 障害物があったら見えない
                if (block == null || !block.isWalkable)
                    return false;
            }
        }

        return false;
    }

    bool IsWithinMoveRange(Vector2Int target)
    {
        if (!useMoveLimit) return true;

        int dist = Mathf.Abs(target.x - moveCenter.x)
                 + Mathf.Abs(target.y - moveCenter.y);

        return dist <= moveRadius;
    }
    bool IsOutsideMoveRange()
    {
        if (!useMoveLimit) return false;
        return !IsWithinMoveRange(unit.gridPos);
    }

    IEnumerator ReturnToCenter()
    {
        Debug.Log($"{unit.name} はテリトリー外！初期位置へ戻る");

        var path = GridManager.Instance.FindPath(unit.gridPos, moveCenter);

        if (path == null || path.Count < 2)
        {
            Debug.LogWarning("戻る経路が見つからない");
            yield break;
        }

        // ★ 1マスだけ戻る（重要）
        Vector2Int next = path[1].gridPos;

        // 念のため再チェック
        if (!IsWithinMoveRange(next))
            yield break;

        yield return StartCoroutine(unit.MoveTowardNearestCoroutine(next));
    }


}
