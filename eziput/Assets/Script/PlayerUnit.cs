using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerUnit : Unit
{
    private void Update()
    {
        if (isMoving || animationController.isAttacking) return;

        // �e�X�L���ɐݒ肳�ꂽ�L�[���͂��`�F�b�N
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
        animationController.isAttacking = true;
        Debug.Log($"{status.unitName} �� {skill.skillName} ���g�p�I");

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
                Debug.Log($"{target.status.unitName} �� {damage} �_���[�W�I");
            }
        }

        yield return new WaitForSeconds(0.3f);
        animationController.isAttacking = false;
    }
}
