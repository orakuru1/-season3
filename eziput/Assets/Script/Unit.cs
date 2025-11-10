// === File: Unit.cs ===
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public GridManager gridManager;


    [System.Serializable]
    public class UnitStatus
    {
        public string unitName = "Unit";
        public int exp = 0;
        public int level = 1;
        public int maxHP = 20;
        public int currentHP = 20;
        public int attack = 5;
        public int defense = 3;
        public int moveRange = 3;
        public int attackRange = 1;
        public float maxStepHeight = 0.5f;
        public int speed = 4;
        public int luck = 1;
        public List<float> levelUpMultipliers = new List<float> { 1f, 1.1f, 1.2f };
        public AttackPatternBase attackPattern;

        //public List<Vector2Int> GetAttackRange(Vector2Int currentPos) => attackPattern != null ? attackPattern.GetPattern(currentPos,facingDir) : new List<Vector2Int>();
    }

    [System.Serializable]
    public class AttackSkill
    {
        public string skillName;
        public KeyCode triggerKey; // プレイヤー用（敵AIでは無視してOK）
        public AttackPatternBase attackPattern;
        public int power = 10;
        public int animationID = 1;

    }

    public UnitStatus status = new UnitStatus();
    public enum Team { Player, Enemy }
    public Team team;

    public Vector2Int gridPos;
    public float moveSpeed = 4f; // interpolation speed
    public bool isMoving = false;
    public bool isShowingAttackRange;

    public Vector2Int facingDir;
    public List<AttackSkill> attackSkills;

    private Vector3 originalPosition;
    public Animator anim;
    public AnimationController animationController;
    private List<GridBlock> highlightedBlocks = new List<GridBlock>();

    //装備による補正値
    public int equidpAttackBonus = 0;
    public int equipDefenseBonus = 0;

    //現在の総合ステータス計算
    public int TotalAttack => status.attack + equidpAttackBonus;
    public int Totaldefense => status.defense + equipDefenseBonus;



    private void Awake()
    {
        gridManager = GridManager.Instance;
        gridManager = FindObjectOfType<GridManager>();
        animationController = GetComponent<AnimationController>();
        if (team == Team.Player)
        {
            status.speed = 10;
        }
        else if (team == Team.Enemy)
        {
            status.speed = 4;
        }
    }

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        // ワールド位置からグリッド座標を自動算出
        gridPos = gridManager.WorldToGrid(transform.position);

        var block = gridManager.GetBlock(gridPos);
        if (block != null)
        {
            block.occupantUnit = this;
            transform.position = block.transform.position + Vector3.up * 1f;
        }

        anim = GetComponent<Animator>();
        animationController = GetComponent<AnimationController>();

        if(animationController != null)
        {
            animationController.onAnimationEnd = () =>
            {
                // 攻撃終了後ターン進行
                if (team == Team.Player)
                {
                    TurnManager.Instance.NextTurn();
                }
            };
        }

        


    }

    public IEnumerator MoveTo(Vector2Int targetGridPos)
    {
        if (isMoving) yield break;

        isMoving = true;
        Vector3 targetWorldPos = new Vector3(targetGridPos.x, transform.position.y, targetGridPos.y);

        while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, Time.deltaTime * moveSpeed);
            yield return null;
        }

        transform.position = targetWorldPos;
        gridPos = targetGridPos;
        isMoving = false;
    }

    // 1マス移動（ターン中の直接移動）
    public bool TryMove(Vector2Int dir)
    {
        if (isMoving || animationController.animationState.isAttacking) return false; Vector2Int next = gridPos + dir;
        var nextBlock = gridManager.GetBlock(next);
        if (nextBlock == null) return false;

        // 敵がいるなら攻撃
        if (nextBlock.occupantUnit != null && nextBlock.occupantUnit.team != this.team)
        {
            StartCoroutine(Attack(nextBlock.occupantUnit));
            return true;
        }

        // 通行可能か
        if (!nextBlock.isWalkable || (nextBlock.occupantUnit != null)) return false;

        StartCoroutine(MoveToBlockCoroutine(nextBlock));
        return true;
    }

    public IEnumerator MoveAlongPath(List<GridBlock> path)
    {
        if (path == null || path.Count == 0) yield break;

        // 移動開始フラグ
        isMoving = true;

        // 現在ブロックの占有を一旦解除
        var curBlock = gridManager.GetBlock(gridPos);
        if (curBlock != null && curBlock.occupantUnit == this)
            curBlock.occupantUnit = null;

        // 各ステップを補間移動
        foreach (var block in path)
        {
            if (block == null) continue;

            Vector3 start = transform.position;
            Vector3 end = block.transform.position + Vector3.up * 1f; // 高さオフセットに合わせる
            float elapsed = 0f;
            float duration = 1f / moveSpeed; // moveSpeed が上で定義されている前提

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(start, end, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = end;

            // gridPos と occupant を更新（途中で occupant が必要ならここで設定）
            gridPos = block.gridPos;
            block.occupantUnit = this;

            // 少し待つ（見た目の調整）
            yield return new WaitForSeconds(0.02f);
        }

        isMoving = false;

        // 終了時にプレイヤーならターン終了通知（経路移動は1回だけ）
        if (team == Unit.Team.Player)
        {
            TurnManager.Instance.EndPlayerTurn();
        }
    }

    // coroutineで滑らかに移動
    private IEnumerator MoveToBlockCoroutine(GridBlock block)
    {
        isMoving = true;

        //    ݃u   b N   
        var cur = gridManager.GetBlock(gridPos);
        if (cur != null && cur.occupantUnit == this) cur.occupantUnit = null;

        Vector3 start = transform.position;
        Vector3 end = block.transform.position + Vector3.up * 1f;
        float elapsed = 0f;
        float duration = 1f / moveSpeed;

        Vector3 dir = (end - start).normalized;
        dir.y = 0f; // 水平方向だけを考慮
        if (dir != Vector3.zero)
            transform.forward = dir;

        if (animationController != null) animationController.StartRunAnimation();
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;

        if (animationController != null) animationController.StopRunAnimation();
        // === ここでoccupant更新 ===
        if (cur != null && cur.occupantUnit == this)
            cur.occupantUnit = null;

        gridPos = block.gridPos;
        block.occupantUnit = this;

        isMoving = false;

        // プレイヤーならターン終了通知
        if (team == Team.Player)
        {
            TurnManager.Instance.EndPlayerTurn();
        }
    }

    // ダッシュ（複数マス）
    public IEnumerator Dash(Vector2Int dir, int distance)
    {
        for (int i = 0; i < distance; i++)
        {
            var next = gridPos + dir;
            var block = gridManager.GetBlock(next);
            if (block == null) break;
            if (block.occupantUnit != null)
            {
                // 敵に接触したら攻撃して止める
                if (block.occupantUnit.team != team) { StartCoroutine(Attack(block.occupantUnit)); break; }
                else break;
            }
            if (!block.isWalkable) break;

            yield return StartCoroutine(MoveToBlockCoroutine(block));
            // 移動毎に一小休止
            while (isMoving) yield return null;
        }

    }
    public IEnumerator Attack(Unit target)
    {
        if (target == null || isMoving || animationController.animationState.isAttacking)
            yield break;

        animationController.Initialize(target);
        animationController.animationState.isAttacking = true;

        // 向き変更（相手の方向を向く）
        Vector3 dir3D = (target.transform.position - transform.position).normalized;
        dir3D.y = 0;
        if (dir3D.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir3D);
        }

        // ===== アニメーションあり =====
        if (anim != null)
        {
            Debug.Log($"{name} attacks {target.name} (with animation)");

            // 攻撃アニメーション開始
            animationController.AttackAnimation();
            // 攻撃アニメーション → 相手のヒットアニメーションの流れは
            // Animator のイベント経由で OnAttackAnimationEnd() / OnHitAnimationEnd() へ
            yield return new WaitUntil(() => !animationController.animationState.isAttacking);
            target.TakeDamage(TotalAttack);

        }
        // ===== アニメーションなし =====
        else
        {
            Debug.Log($"{name} attacks {target.name} (no animator)");
            yield return null;

            target.TakeDamage(1);
            Debug.Log($"{name} attacked {target.name}!");

            animationController.animationState.isAttacking = false;

            // 攻撃終了後ターン進行
            if (team == Team.Player)
            {
                TurnManager.Instance.NextTurn();
            }
        }
    }

    /*
    public IEnumerator Attack(Unit target)
    {
        if (target == null || isMoving || isAttacking) yield break;

        isAttacking = true;
        Target = target;

        // 攻撃方向を向く
        Vector3 dir3D = (target.transform.position - transform.position).normalized;
        dir3D.y = 0;
        if (dir3D.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir3D);
        }

        Debug.Log($"{name} attacked {target.name}!");

        // アニメーターがある場合 → 攻撃アニメーション
        if (anim != null)
        {
            anim.SetInteger("Attack", 1); // トリガーを設定（パラメータ名はプロジェクトに合わせて）
                                          // 攻撃アニメーション中の一部で実際にダメージを与える（例：0.3秒後）
            yield return new WaitForSeconds(0.3f);
            target.TakeDamage(1); // 仮のダメージ値
        }
        else
        {
            // アニメーションが無い場合は即時攻撃
            target.TakeDamage(1);
            yield return null;
        }

        // 攻撃終了処理
        isAttacking = false;

        // 攻撃後にターンを終了（プレイヤーなら敵ターンへ）
        if (team == Team.Player)
        {
            TurnManager.Instance.NextTurn();
        }

    }
    */

    public void zangekiEffect()
    {
        EffectManager.Instance.PlayEffect(EffectManager.EffectType.Zangeki, transform.position + Vector3.up * 1f, transform, transform.rotation);
    }


    public void TakeDamage(int damage)
    {
        status.currentHP -= damage;
        if (status.currentHP <= 0) Die();
    }

    public void LevelUp()
    {
        status.level++;
        float mul = status.levelUpMultipliers[Mathf.Clamp(status.level, 0, status.levelUpMultipliers.Count - 1)];
        status.maxHP = Mathf.RoundToInt(status.maxHP * mul);
        status.attack = Mathf.RoundToInt(status.attack * mul);
    }

    public void Die()
    {
        var block = gridManager.GetBlock(gridPos);
        if (block != null && block.occupantUnit == this) block.occupantUnit = null;
        TurnManager.Instance.RemoveUnit(this);
        Debug.Log($"{name} は倒れた！");
        // 死亡時のアニメーションを再生したいならここで
        // anim.SetTrigger("Die");

        // 攻撃者がいた場合は攻撃完了扱いにする
        if (animationController.attacker != null)
        {
            animationController.attacker.animationState.isHitAnimation = true;
            animationController.attacker.AnimationEnd();
        }

        // このユニットを削除
        Destroy(gameObject);
    }

    // AI用ヘルパー
    public bool CanAttackNearby()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            var b = gridManager.GetBlock(gridPos + d);
            if (b != null && b.occupantUnit != null && b.occupantUnit.team != this.team) return true;
        }
        return false;
    }

    public IEnumerator AttackNearestTarget()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            var b = gridManager.GetBlock(gridPos + d);
            if (b != null && b.occupantUnit != null && b.occupantUnit.team != this.team)
            {
                yield return StartCoroutine(Attack(b.occupantUnit));
                yield break;
            }
        }
    }

    // 敵がプレイヤーに一歩だけ近づく（コルーチンで呼べる）
    public IEnumerator MoveTowardNearestPlayerCoroutine()
    {
        var target = FindClosestEnemyUnit(team == Team.Player ? Team.Enemy : Team.Player);
        if (target == null) yield break;

        // A*で経路を取得
        var path = gridManager.FindPath(gridPos, target.gridPos);
        if (path == null || path.Count < 2) yield break;

        // 次の1マスへ
        var nextBlock = path[1];

        // もし誰かいるなら動かない
        if (nextBlock.occupantUnit != null)
        {
            if (nextBlock.occupantUnit.team != this.team)
            {
                StartCoroutine(Attack(nextBlock.occupantUnit));
                yield break;
            }
            else yield break;
        }

        // ここでブロックを仮確保しておく
        gridManager.GetBlock(gridPos).occupantUnit = null;
        nextBlock.occupantUnit = this;

        yield return StartCoroutine(MoveToBlockCoroutine(nextBlock));

        // 最後に正しい位置更新
        gridPos = nextBlock.gridPos;
    }
    public IEnumerator MoveTowardNearestCoroutine(Vector2Int? targetOverride = null)
    {
        Vector2Int targetPos;

        if (targetOverride.HasValue)
        {
            targetPos = targetOverride.Value;
        }
        else
        {
            // 既存の追尾ターゲット探索ロジック
            var players = FindObjectsOfType<Unit>()
                .Where(u => u.team == Team.Player)
                .ToList();

            if (players.Count == 0) yield break;

            Unit nearest = players.OrderBy(p => Vector2Int.Distance(gridPos, p.gridPos)).First();
            targetPos = nearest.gridPos;
        }

        var path = GridManager.Instance.FindPath(gridPos, targetPos);
        if (path != null && path.Count > 1)
        {
            var next = path[1];
            yield return StartCoroutine(MoveToBlockCoroutine(next));
        }
    }


    public void ShowAttackRange()
    {
        ClearAttackRange();
        if (status.attackPattern == null || gridManager == null) return;

        isShowingAttackRange = true;

        // 向きを考慮した攻撃範囲を取得
        List<Vector2Int> pattern = status.attackPattern.GetPattern(gridPos, facingDir);

        foreach (var pos in pattern)
        {
            GridBlock block = gridManager.GetBlock(pos);
            if (block != null)
            {
                block.SetHighlight(Color.red);
                highlightedBlocks.Add(block);
            }
        }
    }



    // 攻撃範囲の表示を消す
    public void ClearAttackRange()
    {
        foreach (var b in highlightedBlocks)
            b.ResetHighlight();
        highlightedBlocks.Clear();
        isShowingAttackRange = false;
    }
    // 向きによって攻撃パターンを回転
    private Vector2Int RotateByFacing(Vector2Int offset)
    {
        if (facingDir == Vector2Int.up)
            return offset;
        else if (facingDir == Vector2Int.right)
            return new Vector2Int(offset.y, -offset.x);
        else if (facingDir == Vector2Int.down)
            return new Vector2Int(-offset.x, -offset.y);
        else if (facingDir == Vector2Int.left)
            return new Vector2Int(-offset.y, offset.x);
        return offset;
    }
    private Unit FindClosestEnemyUnit(Team targetTeam)
    {
        Unit[] all = FindObjectsOfType<Unit>();
        Unit best = null; float min = float.MaxValue;
        foreach (var u in all)
        {
            if (u.team != targetTeam) continue;
            float d = Vector2Int.Distance(gridPos, u.gridPos);
            if (d < min) { min = d; best = u; }
        }
        return best;
    }

    // End turn (keeps compatibility with older SRPG code)
    public void EndTurn()
    {
        TurnManager.Instance.NextTurn();
    }
}