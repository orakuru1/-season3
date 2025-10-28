using System.Collections.Generic;
using UnityEngine;

public abstract class AttackPatternBase : ScriptableObject
{
    public string patternName = "New Pattern";
    public List<Vector2Int> relativePositions = new List<Vector2Int>() { Vector2Int.up };
    public bool isAreaAttack = false;

    /// <summary>
    /// 向きを考慮して攻撃範囲を返す
    /// </summary>
    public virtual List<Vector2Int> GetPattern(Vector2Int center, Vector2Int facingDir)
    {
        var result = new List<Vector2Int>();
        foreach (var offset in relativePositions)
        {
            Vector2Int rotated = RotateByFacing(offset, facingDir);
            result.Add(center + rotated);
        }
        return result;
    }

    /// <summary>
    /// 方向に応じて相対座標を回転させる
    /// </summary>
    protected Vector2Int RotateByFacing(Vector2Int offset, Vector2Int facingDir)
    {
        if (facingDir == Vector2Int.up)
            return offset;
        else if (facingDir == Vector2Int.right)
            return new Vector2Int(offset.y, -offset.x);
        else if (facingDir == Vector2Int.down)
            return new Vector2Int(-offset.x, -offset.y);
        else if (facingDir == Vector2Int.left)
            return new Vector2Int(-offset.y, offset.x);
        return offset;
    }
}
