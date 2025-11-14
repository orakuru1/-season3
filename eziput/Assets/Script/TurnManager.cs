// === File: TurnManager.cs ===
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public Unit.Team currentTeam { get; private set; }  // 現在行動中のチーム

    public delegate void OnTurnStartDelegate(Unit unit);
    public static event OnTurnStartDelegate OnTurnStart;

    public List<Unit> allUnits = new List<Unit>();
    private int currentIndex = 0;

    public Unit CurrentUnit => allUnits.Count > 0 ? allUnits[currentIndex] : null;

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    private void Start()
    {
        allUnits = FindObjectsOfType<Unit>().OrderByDescending(u => u.status.speed).ToList();
        if (allUnits.Count == 0) return;
        StartCoroutine(StartTurnsRoutine());
    }

    private IEnumerator StartTurnsRoutine()
    {
        yield return null;
        StartNextTurn();
    }

    public void StartNextTurn()
    {
        if (allUnits.Count == 0) return;
        currentIndex = currentIndex % allUnits.Count;
        var unit = allUnits[currentIndex];
        currentTeam = unit.team;  //  現在のチームを記録

        //Debug.Log($"Turn start: {unit.name} ({unit.team})");
        OnTurnStart?.Invoke(unit);
        // プレイヤーターン → 敵全員ターン の交互にする
        bool isPlayerTurn = allUnits[currentIndex].team == Unit.Team.Player;

        if (unit.team == Unit.Team.Player)
        {
            var playerUnit = allUnits.FirstOrDefault(u => u.team == Unit.Team.Player);
            if (playerUnit == null) return;

            //Debug.Log("Player Turn Start");
            OnTurnStart?.Invoke(playerUnit);
            // プレイヤーの行動は InputHandler 側で EndPlayerTurn() を呼ぶ
        }
        else
        {
            //Debug.Log("Enemy Turn Start");
            StartCoroutine(ExecuteAllEnemies());
        }
    }

    private IEnumerator ExecuteAllEnemies()
    {
        // 敵全員取得
        var enemies = allUnits.Where(u => u.team == Unit.Team.Enemy).ToList();

        // --- 攻撃できる敵を先に実行 ---
        foreach (var enemy in enemies)
        {
            var eUnit = enemy.GetComponent<EnemyUnit>();
            if (eUnit != null && eUnit.TrySelectSkill())
            {
                yield return StartCoroutine(eUnit.UseSelectedSkill());
                continue; // 攻撃済みの敵はここでスキップ
            }

            var ai = enemy.GetComponent<EnemyAI>();
            if (ai != null && ai.CanAttackPlayer())
            {
                yield return StartCoroutine(ai.AttackNearestPlayer());
            }
        }


        // --- 残り（攻撃できなかった敵）を一斉移動 ---
        List<Coroutine> moveCoroutines = new List<Coroutine>();
        foreach (var enemy in enemies)
        {
            var ai = enemy.GetComponent<EnemyAI>();
            if (ai != null && !ai.CanAttackPlayer())
            {
                moveCoroutines.Add(StartCoroutine(ai.ExecuteEnemyTurn()));
            }
        }

        // 全員の移動完了を待つ
        foreach (var c in moveCoroutines)
        {
            yield return c;
        }

        Debug.Log("Enemy Turn End → Player Turn Start");
        EndEnemyTurn();
    }

    public void EndPlayerTurn()
    {
        Debug.Log("Player Turn End → Enemy Turn Start");
        currentIndex++;
        StartNextTurn();
    }

    private void EndEnemyTurn()
    {
        currentIndex = 0; // プレイヤーに戻す
        StartNextTurn();
    }
    // SRPG互換: 次のユニットへ
    public void NextTurn()
    {
        if (allUnits.Count == 0) return;
        currentIndex = (currentIndex + 1) % allUnits.Count;
        StartNextTurn();
    }
    public void RemoveUnit(Unit unit)
    {
        if (allUnits.Contains(unit))
        {
            int idx = allUnits.IndexOf(unit);
            allUnits.Remove(unit);
            if (idx <= currentIndex && currentIndex > 0) currentIndex--;
        }
    }

    public List<Unit> GetAllUnits()
    {
        return allUnits;
    }

}