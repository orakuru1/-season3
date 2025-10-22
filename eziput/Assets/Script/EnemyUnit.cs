using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyUnit : Unit
{
    public IEnumerator UseSkillAI(Unit target)
    {
        if (attackSkills.Count == 0) yield break;

        // �Ƃ肠���������_���ɃX�L���I��
        var skill = attackSkills[Random.Range(0, attackSkills.Count)];
        Debug.Log($"{status.unitName} �� {skill.skillName} ���g�����Ƃ��Ă���");

        List<Vector2Int> attackPositions = skill.attackPattern.GetPattern(gridPos);
        foreach (var pos in attackPositions)
        {
            GridBlock block = gridManager.GetBlock(pos);
            if (block != null && block.occupantUnit == target)
            {
                int damage = skill.power;
                target.status.currentHP -= damage;
                Debug.Log($"{target.status.unitName} �� {damage} �_���[�W�I");
                yield break;
            }
        }
    }
}
