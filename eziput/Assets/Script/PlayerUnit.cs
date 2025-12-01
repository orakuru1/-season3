using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class PlayerUnit : Unit
{
    public AttackSkill selectedSkill = null; // InputHandlerから指定
    public bool isUsingSkill = false; // スキル実行中フラグ        
    
    // スキル使用
    public IEnumerator UseSkill(AttackSkill skill)
    {
        if (isUsingSkill) yield break; // 二重起動防止
        isUsingSkill = true;
        if (skill == null || skill.attackPattern == null)
        {
            Debug.LogWarning("スキルまたは攻撃パターンが未設定です。");
            yield break;
        }

        animationController.animationState.isAttacking = true;
        Debug.Log($"{status.unitName} が {skill.skillName} を使用しようとしています");

        // 攻撃対象候補を取得
        List<Vector2Int> attackPositions = skill.attackPattern.GetPattern(gridPos, facingDir);
        List<Unit> targets = new List<Unit>();

        foreach (var pos in attackPositions)
        {
            GridBlock targetBlock = gridManager.GetBlock(pos);
            if (targetBlock != null && targetBlock.occupantUnit != null &&
                targetBlock.occupantUnit.team != this.team)
            {
                targets.Add(targetBlock.occupantUnit);
            }
        }

        // 対象がいなければキャンセル
        if (targets.Count == 0)
        {
            Debug.Log("敵が範囲内にいないため、スキルは発動しません。");
            animationController.animationState.isAttacking = false;
            selectedSkill = null;
            isUsingSkill = false; // 終了時に解除
            ClearAttackRange();
            yield break;
        }

        Debug.Log($"{status.unitName} が {skill.skillName} を使用！ 対象数: {targets.Count}");

        if (anim != null)
        {
            if (targets.Count == 1)
            {
                animationController.Initialize(targets[0], skill.power);
                animationController.AttackAnimation(skill.animationID);
            }
            else
            {
                animationController.Initialize(targets, skill.power);
                animationController.AttackAnimation(skill.animationID);
            }

        }

        ClearAttackRange();

        // ヒットアニメーションが見える時間だけ待機
        yield return new WaitForSeconds(0.3f);

        // プレイヤーの攻撃アニメーション終了を待つ
        if (anim != null)
            yield return new WaitUntil(() => !animationController.animationState.isAttacking);
        else
            yield return null;

        //攻撃の後に神の力をゲットできるか毎回しよう
        yield return StartCoroutine(GodManeger.Instance.GrantGodToPlayer());

        //攻撃する時共通,攻撃時の神の力発動
        yield return StartCoroutine(GodManeger.Instance.TriggerAbilities(this.gameObject, AbilityTrigger.Passive_OnAttack));
        yield return null;

        yield return StartCoroutine(GodManeger.Instance.GrantGodToPlayer());


        // 終了処理
        TurnManager.Instance.NextTurn();
        animationController.animationState.isAttacking = false;
        selectedSkill = null;
        isUsingSkill = false; // 終了時に解除

        // ターン進行は自然に敵が動くように管理


    }

    public void Heal(int amount)
    {
        //Hpが満タンかチェック
        if(status.currentHP >= status.maxHP)
        {
            Debug.Log($"{status.unitName}のHpはすでに満タンです。回復アイテムを使う必要はない！！");
            return;
        }

        int oldHP = status.currentHP;
        status.currentHP = Mathf.Min(status.currentHP + amount, status.maxHP); //maxを超えない

        int healedAmount = status.currentHP - oldHP;  //実際に回復した量
        Debug.Log($"{status.unitName}のHPが{amount}回復！");

    }

    // 敵ヒット用コルーチン
    private IEnumerator HitTarget(Unit target, int damage)
    {
        // 向き変更（プレイヤーが敵方向を向く）


        // ヒットアニメーション開始（ダメージはまだ）
        if (target.anim != null)
        {
            //target.Target = this;
            target.anim.SetInteger("Hit", 1);
        }

        yield return null; // 即時終了、同時に再生される
    }
}
