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

    // TurnManager から呼ぶ（コルーチンを使う）
    public IEnumerator ExecuteEnemyTurn()
    {
        if (unit == null) yield break;

        // 行動前のウェイト（演出）
        yield return new WaitForSeconds(0.15f);

        // 攻撃可能なら攻撃
        if (unit.CanAttackNearby())
        {
            unit.AttackNearestTarget();
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            // 近づく（1歩）
            yield return unit.MoveTowardNearestPlayerCoroutine();
        }

        // 行動終了
        yield return new WaitForSeconds(0.05f);
    }
}