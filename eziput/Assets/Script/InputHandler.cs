// === File: InputHandler.cs ===
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Camera mainCamera;
    private Unit currentPlayerUnit;

    private void OnEnable()
    {
        TurnManager.OnTurnStart += OnTurnStart;
    }
    private void OnDisable()
    {
        TurnManager.OnTurnStart -= OnTurnStart;
    }

    private void OnTurnStart(Unit unit)
    {
        if (unit.team == Unit.Team.Player)
            currentPlayerUnit = unit;
        else
            currentPlayerUnit = null;
    }

    private void Update()
    {
        if (currentPlayerUnit == null) return;

        // �L�[�{�[�h�ړ��i1�}�X�j
        if (Input.GetKeyDown(KeyCode.W)) { if (currentPlayerUnit.TryMove(Vector2Int.up)) { } }
        if (Input.GetKeyDown(KeyCode.S)) { if (currentPlayerUnit.TryMove(Vector2Int.down)) { } }
        if (Input.GetKeyDown(KeyCode.A)) { if (currentPlayerUnit.TryMove(Vector2Int.left)) { } }
        if (Input.GetKeyDown(KeyCode.D)) { if (currentPlayerUnit.TryMove(Vector2Int.right)) { } }

        // �_�b�V���iShift + dir�j
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.W)) StartCoroutine(currentPlayerUnit.Dash(Vector2Int.up, 7));
            if (Input.GetKeyDown(KeyCode.S)) StartCoroutine(currentPlayerUnit.Dash(Vector2Int.down, 7));
            if (Input.GetKeyDown(KeyCode.A)) StartCoroutine(currentPlayerUnit.Dash(Vector2Int.left, 7));
            if (Input.GetKeyDown(KeyCode.D)) StartCoroutine(currentPlayerUnit.Dash(Vector2Int.right, 7));
        }
        /*
        // �}�E�X�N���b�N�ړ��i�o�H�T�����p�j
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var block = hit.collider.GetComponent<GridBlock>();
                if (block != null)
                {
                    // �ړ��悪���݂��o�H�����邩
                    var start = gridPositionToVector2Int(currentPlayerUnit.gridPos);
                    var path = GridManager.Instance.FindPath(currentPlayerUnit.gridPos, block.gridPos);
                    if (path != null && path.Count > 0)
                    {
                        // path �� GridBlock ���X�g�Ƃ��ēn�� MoveAlongPath ���g��
                        StartCoroutine(currentPlayerUnit.MoveAlongPath(path));
                    }
                }
            }
        }*/
    }

    private Vector2Int gridPositionToVector2Int(Vector2Int v) => v;
}