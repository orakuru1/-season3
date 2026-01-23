using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{
    private SkillData selectedSkill;
    public bool isUsingSkill = false;

    private BossScript bossScript;

    public int expReward = 5;
    //近づいたら、Word Spaceで表示させるようにしたい。(雑魚的)
    //ボスは、Screen Space - Cameraで表示させる。

    /// <summary>
    /// 攻撃範囲内にプレイヤーがいるスキルを自動選択
    /// </summary>
    public bool TrySelectSkill()
    {
        if (attackSkills == null || attackSkills.Count == 0)
        {
            //Debug.Log($"{name} はスキルを持っていません。");
            return false;
        }

        var gm = GridManager.Instance;
        var players = FindObjectsOfType<Unit>().Where(u => u.team == Team.Player).ToList();
        if (players.Count == 0) return false;

        foreach (var skill in attackSkills)
        {
            if (skill.attackPattern == null) continue;

            var attackPositions = skill.attackPattern.GetPattern(gridPos, facingDir);
            foreach (var pos in attackPositions)
            {
                var block = gm.GetBlock(pos);
                if (block != null && block.occupantUnit != null && block.occupantUnit.team == Team.Player && HasLineOfSight(gridPos, pos))
                {
                    selectedSkill = skill;
                    Debug.Log($"{name} がスキル {skill.skillName} を選択しました。");
                    return true;
                }
            }
        }

        selectedSkill = null;
        return false;
    }

    /// <summary>
    /// 自動でスキルを発動（EnemyUnit 専用処理）
    /// </summary>
    public IEnumerator UseSelectedSkill()
    {
        if (isUsingSkill) yield break;
        if (selectedSkill == null)
        {
            Debug.LogWarning($"{name} に選択されたスキルがありません。");
            yield break;
        }

        isUsingSkill = true;

        Debug.Log($"{name} が {selectedSkill.skillName} を使用！");

        var gm = GridManager.Instance;
        var attackPositions = selectedSkill.attackPattern.GetPattern(gridPos, facingDir);
        List<Unit> targets = new List<Unit>();

        foreach (var pos in attackPositions)
        {
            var block = gm.GetBlock(pos);
            if (block != null && block.occupantUnit != null &&
                block.occupantUnit.team == Team.Player)
            {
                targets.Add(block.occupantUnit);
            }
        }

        if (targets.Count == 0)
        {
            Debug.Log("範囲内にプレイヤーがいません。");
            isUsingSkill = false;
            yield break;
        }
        Unit nearestTarget = targets.OrderBy(t => Vector2Int.Distance(gridPos, t.gridPos)).First();
        Vector3 dir3D = (nearestTarget.transform.position - transform.position).normalized;
        dir3D.y = 0;
        if (dir3D.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir3D);
        }
        // 攻撃アニメーション開始（簡易例）
        animationController.animationState.isAttacking = true;

        int TA = CalculateDamage(status.attack, equippedWeaponType);
        int TrueAttack = TA + selectedSkill.power + equidpAttackBonus;

        // 単体 or 複数ターゲット対応
        if (targets.Count == 1)
        {
            animationController.Initialize(targets[0], TrueAttack);
            animationController.AttackAnimation(selectedSkill.animationID);
        }
        else
        {
            animationController.Initialize(targets, TrueAttack);
            animationController.AttackAnimation(selectedSkill.animationID);
        }

        // 攻撃が終わるまで待機
        yield return new WaitUntil(() => !animationController.animationState.isAttacking);

        animationController.animationState.isAttacking = false;
        isUsingSkill = false;
        selectedSkill = null;
    }

    bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        var line = GridLineUtility.GetLine(from, to);

        foreach (var pos in line)
        {
            // 到達点（プレイヤー位置）は許可
            if (pos == to)
                return true;

            var block = GridManager.Instance.GetBlock(pos);

            // 壁 or マップ外があれば遮断
            if (block == null || !block.isWalkable)
                return false;
        }

        return true;
    }

}
