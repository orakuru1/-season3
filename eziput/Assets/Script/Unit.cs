// === File: Unit.cs ===
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public GridManager gridManager;

    
    [System.Serializable]
    public class UnitStatus
    {
        public string unitName = "Unit";
        public int exp = 0;
        public int level = 1;
        public int maxHP = 20;
        public int currentHP = 20;
        public int attack = 5;
        public int defense = 3;
        public int moveRange = 3;
        public int attackRange = 1;
        public float maxStepHeight = 0.5f;
        public int speed = 4;
        public int luck = 1;
        public List<float> levelUpMultipliers = new List<float> { 1f, 1.1f, 1.2f };
        public AttackPatternBase attackPattern;

        public List<Vector2Int> GetAttackRange(Vector2Int currentPos) => attackPattern != null ? attackPattern.GetPattern(currentPos) : new List<Vector2Int>();
    }

    public UnitStatus status = new UnitStatus();
    public enum Team { Player, Enemy }
    public Team team;

    public Vector2Int gridPos;
    public float moveSpeed = 4f; // interpolation speed
    public bool isMoving = false;
    private bool isAttacking = false;

    private Vector3 originalPosition;
    private Animator anim;

    private void Awake()
    {
        gridManager = GridManager.Instance;
        gridManager = FindObjectOfType<GridManager>();
    }

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        // ���[���h�ʒu����O���b�h���W�������Z�o
        gridPos = gridManager.WorldToGrid(transform.position);

        var block = gridManager.GetBlock(gridPos);
        if (block != null)
        {
            block.occupantUnit = this;
            transform.position = block.transform.position + Vector3.up * 1f;
        }

        anim = GetComponent<Animator>();
    }


    // 1�}�X�ړ��i�^�[�����̒��ڈړ��j
    public bool TryMove(Vector2Int dir)
    {
        if (isMoving) return false;
        Vector2Int next = gridPos + dir;
        var nextBlock = gridManager.GetBlock(next);
        if (nextBlock == null) return false;

        // �G������Ȃ�U��
        if (nextBlock.occupantUnit != null && nextBlock.occupantUnit.team != this.team)
        {
            StartCoroutine(Attack(nextBlock.occupantUnit));
            return true;
        }

        // �ʍs�\��
        if (!nextBlock.isWalkable || (nextBlock.occupantUnit != null)) return false;

        StartCoroutine(MoveToBlockCoroutine(nextBlock));
        return true;
    }

    public IEnumerator MoveAlongPath(List<GridBlock> path)
    {
        if (path == null || path.Count == 0) yield break;

        // �ړ��J�n�t���O
        isMoving = true;

        // ���݃u���b�N�̐�L����U����
        var curBlock = gridManager.GetBlock(gridPos);
        if (curBlock != null && curBlock.occupantUnit == this)
            curBlock.occupantUnit = null;

        // �e�X�e�b�v���Ԉړ�
        foreach (var block in path)
        {
            if (block == null) continue;

            Vector3 start = transform.position;
            Vector3 end = block.transform.position + Vector3.up * 1f; // �����I�t�Z�b�g�ɍ��킹��
            float elapsed = 0f;
            float duration = 1f / moveSpeed; // moveSpeed ����Œ�`����Ă���O��

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(start, end, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = end;

            // gridPos �� occupant ���X�V�i�r���� occupant ���K�v�Ȃ炱���Őݒ�j
            gridPos = block.gridPos;
            block.occupantUnit = this;

            // �����҂i�����ڂ̒����j
            yield return new WaitForSeconds(0.02f);
        }

        isMoving = false;

        // �I�����Ƀv���C���[�Ȃ�^�[���I���ʒm�i�o�H�ړ���1�񂾂��j
        if (team == Unit.Team.Player)
        {
            TurnManager.Instance.EndPlayerTurn();
        }
    }

    // coroutine�Ŋ��炩�Ɉړ�
    private IEnumerator MoveToBlockCoroutine(GridBlock block)
    {
        isMoving = true;

        // ���݃u���b�N���
        var cur = gridManager.GetBlock(gridPos);
        if (cur != null && cur.occupantUnit == this) cur.occupantUnit = null;

        Vector3 start = transform.position;
        Vector3 end = block.transform.position + Vector3.up * 1f;
        float elapsed = 0f;
        float duration = 1f / moveSpeed;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;

        // �ŏI�X�V
        gridPos = block.gridPos;
        block.occupantUnit = this;

        isMoving = false;

        // �v���C���[�Ȃ�^�[���I���ʒm
        if (team == Team.Player)
        {
            TurnManager.Instance.EndPlayerTurn();
        }
    }

    // �_�b�V���i�����}�X�j
    public IEnumerator Dash(Vector2Int dir, int distance)
    {
        for (int i = 0; i < distance; i++)
        {
            var next = gridPos + dir;
            var block = gridManager.GetBlock(next);
            if (block == null) break;
            if (block.occupantUnit != null)
            {
                // �G�ɐڐG������U�����Ď~�߂�
                if (block.occupantUnit.team != team) { StartCoroutine(Attack(block.occupantUnit)); break; }
                else break;
            }
            if (!block.isWalkable) break;

            yield return StartCoroutine(MoveToBlockCoroutine(block));
            // �ړ����Ɉꏬ�x�~
            while (isMoving) yield return null;
        }
    }

    public IEnumerator Attack(Unit target)
    {
        if (target == null) yield break;

        // �U����Ƀ^�[�����I���i�v���C���[�Ȃ�G�^�[���ցj
        if (isAttacking) yield break;

        if (team == Team.Player)
        {
            isAttacking = true;///現在は攻撃アニメーションがプレイヤーにしか入っていないため、プレイヤー限定にする。
        }

        if (anim != null)
        {
            // �����ύX�i�U�������������j
            Vector2 dir = (target.transform.position - transform.position).normalized;
            transform.forward = new Vector3(dir.x, 0, dir.y);

            yield return null;

            // �U������
            target.TakeDamage(1); // ����1�_���[�W

            Debug.Log($"{name} attacked {target.name}!");
            anim.SetInteger("Attack", 1);

        }
        
    }

    public void AttackEnd()
    {
        isAttacking = false;
        anim.SetInteger("Attack",0);
        // �U����Ƀ^�[�����I���i�v���C���[�Ȃ�G�^�[���ցj
        if (team == Team.Player)
        {
            TurnManager.Instance.NextTurn();
        }
    }


    public void TakeDamage(int damage)
    {
        status.currentHP -= damage;
        if (status.currentHP <= 0) Die();
    }

    public void LevelUp()
    {
        status.level++;
        float mul = status.levelUpMultipliers[Mathf.Clamp(status.level, 0, status.levelUpMultipliers.Count - 1)];
        status.maxHP = Mathf.RoundToInt(status.maxHP * mul);
        status.attack = Mathf.RoundToInt(status.attack * mul);
    }

    public void Die()
    {
        var block = gridManager.GetBlock(gridPos);
        if (block != null && block.occupantUnit == this) block.occupantUnit = null;
        TurnManager.Instance.RemoveUnit(this);
        Destroy(gameObject);
    }

    // AI�p�w���p�[
    public bool CanAttackNearby()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            var b = gridManager.GetBlock(gridPos + d);
            if (b != null && b.occupantUnit != null && b.occupantUnit.team != this.team) return true;
        }
        return false;
    }

    public void AttackNearestTarget()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            var b = gridManager.GetBlock(gridPos + d);
            if (b != null && b.occupantUnit != null && b.occupantUnit.team != this.team)
            {
                StartCoroutine(Attack(b.occupantUnit));
                return;
            }
        }
    }

    // �G���v���C���[�Ɉ�������߂Â��i�R���[�`���ŌĂׂ�j
    public IEnumerator MoveTowardNearestPlayerCoroutine()
    {
        var target = FindClosestEnemyUnit(team == Team.Player ? Team.Enemy : Team.Player);
        if (target == null) yield break;

        // A*�Ōo�H���擾
        var path = gridManager.FindPath(gridPos, target.gridPos);
        if (path == null || path.Count < 2) yield break;

        // ����1�}�X��
        var nextBlock = path[1];
        if (nextBlock.occupantUnit != null)
        {
            if (nextBlock.occupantUnit.team != this.team)
            {
                StartCoroutine(Attack(nextBlock.occupantUnit));
                yield break;
            }
            else yield break;
        }

        yield return StartCoroutine(MoveToBlockCoroutine(nextBlock));
    }





    private Unit FindClosestEnemyUnit(Team targetTeam)
    {
        Unit[] all = FindObjectsOfType<Unit>();
        Unit best = null; float min = float.MaxValue;
        foreach (var u in all)
        {
            if (u.team != targetTeam) continue;
            float d = Vector2Int.Distance(gridPos, u.gridPos);
            if (d < min) { min = d; best = u; }
        }
        return best;
    }

    // End turn (keeps compatibility with older SRPG code)
    public void EndTurn()
    {
        TurnManager.Instance.NextTurn();
    }
}