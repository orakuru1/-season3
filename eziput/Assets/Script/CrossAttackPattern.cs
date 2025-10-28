using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AttackPatterns/Cross Pattern", fileName = "CrossPattern")]
public class CrossAttackPattern : AttackPatternBase
{
    public override List<Vector2Int> GetPattern(Vector2Int center, Vector2Int facingDir)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (var offset in relativePositions)
        {
            Vector2Int rotated = RotateByFacing(offset, facingDir);
            positions.Add(center + rotated);
        }
        return positions;
    }
}
