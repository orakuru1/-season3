// === File: TurnManager.cs ===
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public delegate void OnTurnStartDelegate(Unit unit);
    public static event OnTurnStartDelegate OnTurnStart;

    private List<Unit> allUnits = new List<Unit>();
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

        Debug.Log($"Turn start: {unit.name} ({unit.team})");
        OnTurnStart?.Invoke(unit);

        // �v���C���[�̏ꍇ InputHandler ��������󂯕t����
        if (unit.team == Unit.Team.Player)
        {
            // nothing here: player's input will call TurnManager.EndPlayerTurn()
        }
        else
        {
            // �G�Ȃ�R���[�`���Ŏ��s
            StartCoroutine(ExecuteEnemy(unit));
        }
    }

    private IEnumerator ExecuteEnemy(Unit enemy)
    {
        var ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            yield return StartCoroutine(ai.ExecuteEnemyTurn());
        }

        // �G�̃^�[���I������
        //NextTurn();
    }

    // �v���C���[���s�����I�����Ƃ��ɌĂԁiEndPlayerTurn�j
    public void EndPlayerTurn()
    {
        NextTurn();
    }

    // SRPG�݊�: ���̃��j�b�g��
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
}