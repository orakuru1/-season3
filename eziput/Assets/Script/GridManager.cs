// === File: GridManager.cs ===
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid settings")]
    public int width = 20;
    public int height = 12;
    public GameObject blockPrefab; // 別チームがランダム生成するときはこのプレハブを使ってもらう

    // 外部ランダム生成チーム向け: 生成済みのBlockを登録する仕組みを残す
    private Dictionary<Vector2Int, GridBlock> gridMap = new Dictionary<Vector2Int, GridBlock>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 自動生成オプション: blockPrefab が指定されている場合シンプルな平面を作る。
        if (blockPrefab != null && gridMap.Count == 0)
        {
            GenerateFlatGrid();
        }

        Unit[] allUnits = FindObjectsOfType<Unit>();

    foreach (var unit in allUnits)
    {
        Vector2Int pos = WorldToGrid(unit.transform.position);
        unit.gridPos = pos;

        var block = GetBlock(pos);
        if (block != null)
        {
            if (block.occupantUnit != null)
            {
                Debug.LogWarning($"位置 {pos} は {block.occupantUnit.name} がすでに占有しています。{unit.name} はスキップ。");
                continue;
            }

            block.occupantUnit = unit;
            unit.transform.position = block.transform.position + Vector3.up * 1f;
        }
    }
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / 1);
        int y = Mathf.RoundToInt(worldPos.z / 1); // ZをY軸扱い
        return new Vector2Int(x, y);
    }


    public void GenerateFlatGrid()
    {
        gridMap.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, 0f, y);
                var obj = Instantiate(blockPrefab, pos, Quaternion.identity, transform);
                obj.name = $"Block_{x}_{y}";
                var block = obj.GetComponent<GridBlock>();
                if (block == null) block = obj.AddComponent<GridBlock>();
                block.gridPos = new Vector2Int(x, y);
                gridMap[new Vector2Int(x, y)] = block;
            }
        }
    }

    // 外部（ランダム生成チーム）が個別にブロックを生成して登録する場合はこちらを呼ぶ
    public void RegisterBlock(GridBlock block)
    {
        if (block == null) return;
        gridMap[block.gridPos] = block;
    }

    public GridBlock GetBlock(Vector2Int gridPos)
    {
        gridMap.TryGetValue(gridPos, out var block);
        return block;
    }

    // ワールド座標からグリッド座標へ（SRPG側の既存呼び出し互換）
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.z));
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        var b = GetBlock(gridPos);
        if (b != null) return b.transform.position;
        return new Vector3(gridPos.x, 0f, gridPos.y);
    }

    public List<GridBlock> GetAllBlocks()
    {
        return gridMap.Values.ToList();
    }

    // SRPGで使っていたGetMovableBlocksを互換的に残す（ローグライクでも参照可）
    public void GetMovableBlocks(Vector2Int startPos, int moveRange, out List<GridBlock> walkable, out List<GridBlock> unwalkable)
    {
        walkable = new List<GridBlock>();
        unwalkable = new List<GridBlock>();

        if (!gridMap.TryGetValue(startPos, out var startBlock)) return;

        var unit = TurnManager.Instance != null ? TurnManager.Instance.CurrentUnit : null;

        var queue = new Queue<(GridBlock block, int cost)>();
        var visited = new HashSet<GridBlock>();

        queue.Enqueue((startBlock, 0));
        visited.Add(startBlock);

        while (queue.Count > 0)
        {
            var (current, cost) = queue.Dequeue();
            walkable.Add(current);
            if (cost >= moveRange) continue;

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var d in dirs)
            {
                var np = current.gridPos + d;
                if (!gridMap.TryGetValue(np, out var nb)) continue;
                if (visited.Contains(nb)) continue;

                // 占有・通行・段差チェック
                if (nb.occupantUnit != null && nb.occupantUnit != unit)
                {
                    unwalkable.Add(nb);
                    visited.Add(nb);
                    continue;
                }

                float heightDiff = Mathf.Abs(nb.transform.position.y - current.transform.position.y);
                bool reachable = unit == null || heightDiff <= unit.status.maxStepHeight;

                if (reachable && nb.isWalkable)
                {
                    queue.Enqueue((nb, cost + 1));
                    visited.Add(nb);
                }
                else
                {
                    unwalkable.Add(nb);
                    visited.Add(nb);
                }
            }
        }

        // 見た目用 unwalkable 追加（省略可）
        foreach (var b in gridMap.Values)
        {
            if (b.isWalkable && !walkable.Contains(b)) unwalkable.Add(b);
        }
    }



    // 簡易 A*（SRPGで使われていたFindPathを復元）
    public List<GridBlock> FindPath(Vector2Int start, Vector2Int goal)
    {
        var open = new List<GridBlock>();
        var closed = new HashSet<GridBlock>();
        var cameFrom = new Dictionary<GridBlock, GridBlock>();
        var gScore = new Dictionary<GridBlock, int>();

        var startBlock = GetBlock(start);
        var goalBlock = GetBlock(goal);
        if (startBlock == null || goalBlock == null)
            return null;

        open.Add(startBlock);
        gScore[startBlock] = 0;

        while (open.Count > 0)
        {
            open.Sort((a, b) => (gScore[a] + Heuristic(a, goalBlock)).CompareTo(gScore[b] + Heuristic(b, goalBlock)));
            var current = open[0];

            if (current == goalBlock)
                return ReconstructPath(cameFrom, current);

            open.RemoveAt(0);
            closed.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!neighbor.isWalkable || closed.Contains(neighbor))
                    continue;

                // 他のユニットがいるマスはスキップ
                if (neighbor.occupantUnit != null && neighbor != goalBlock)
                    continue;

                int tentativeG = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;

                    if (!open.Contains(neighbor))
                        open.Add(neighbor);
                }
            }
        }

        return null;
    }

    private List<GridBlock> ReconstructPath(Dictionary<GridBlock, GridBlock> cameFrom, GridBlock current)
    {
        var path = new List<GridBlock> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private int Heuristic(GridBlock a, GridBlock b)
    {
        return Mathf.Abs(a.gridPos.x - b.gridPos.x) + Mathf.Abs(a.gridPos.y - b.gridPos.y);
    }

    // 上下左右のみ
    private List<GridBlock> GetNeighbors(GridBlock block)
    {
        var neighbors = new List<GridBlock>();
        Vector2Int[] dirs = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        foreach (var d in dirs)
        {
            var n = GetBlock(block.gridPos + d);
            if (n != null)
                neighbors.Add(n);
        }
        return neighbors;
    }


    private int GetManhattanDistance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    public bool CanMoveBetween(GridBlock from, GridBlock to, Unit unit)
    {
        if (to == null || !to.isWalkable) return false;
        float heightDelta = to.transform.position.y - from.transform.position.y;
        return Mathf.Abs(heightDelta) <= (unit != null ? unit.status.maxStepHeight : 999f);
    }

    // シンプル優先度キュー
    public class PriorityQueue<T>
    {
        private List<(T item, int priority)> elements = new List<(T, int)>();
        public int Count => elements.Count;
        public void Enqueue(T item, int priority) => elements.Add((item, priority));
        public T Dequeue()
        {
            int best = 0;
            for (int i = 1; i < elements.Count; i++) if (elements[i].priority < elements[best].priority) best = i;
            var it = elements[best].item;
            elements.RemoveAt(best);
            return it;
        }
    }
}