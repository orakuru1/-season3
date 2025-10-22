using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyUnit : Unit
{
    public IEnumerator UseSkillAI(Unit target)
    {
        if (attackSkills.Count == 0) yield break;

        // とりあえずランダムにスキル選択
        var skill = attackSkills[Random.Range(0, attackSkills.Count)];
        Debug.Log($"{status.unitName} が {skill.skillName} を使おうとしている");

        List<Vector2Int> attackPositions = skill.attackPattern.GetPattern(gridPos);
        foreach (var pos in attackPositions)
        {
            GridBlock block = gridManager.GetBlock(pos);
            if (block != null && block.occupantUnit == target)
            {
                int damage = skill.power;
                target.status.currentHP -= damage;
                Debug.Log($"{target.status.unitName} に {damage} ダメージ！");
                yield break;
            }
        }
    }
}
