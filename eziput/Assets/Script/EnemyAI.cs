// === File: EnemyAI.cs ===
using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    // TurnManager ����Ăԁi�R���[�`�����g���j
    public IEnumerator ExecuteEnemyTurn()
    {
        if (unit == null) yield break;

        // �s���O�̃E�F�C�g�i���o�j
        yield return new WaitForSeconds(0.15f);

        // �U���\�Ȃ�U��
        if (unit.CanAttackNearby())
        {
            unit.AttackNearestTarget();
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            // �߂Â��i1���j
            yield return unit.MoveTowardNearestPlayerCoroutine();
        }

        // �s���I��
        yield return new WaitForSeconds(0.05f);
    }
}