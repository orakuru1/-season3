using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerUnit : Unit
{

    private void Update()
    {
        if (isMoving || isAttacking) return;

        // 各スキルに設定されたキー入力をチェック
        foreach (var skill in attackSkills)
        {
            if (Input.GetKeyDown(skill.triggerKey))
            {
                StartCoroutine(UseSkill(skill));
                break;
            }
        }
    }

    private IEnumerator UseSkill(AttackSkill skill)
    {
        isAttacking = true;
        Debug.Log($"{status.unitName} が {skill.skillName} を使用！");

        List<Vector2Int> attackPositions = skill.attackPattern.GetPattern(gridPos);
        foreach (var pos in attackPositions)
        {
            GridBlock targetBlock = gridManager.GetBlock(pos);
            if (targetBlock != null && targetBlock.occupantUnit != null &&
                targetBlock.occupantUnit.team != this.team)
            {
                var target = targetBlock.occupantUnit;
                int damage = skill.power;
                target.status.currentHP -= damage;
                Debug.Log($"{target.status.unitName} に {damage} ダメージ！");
            }
        }

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }
}
