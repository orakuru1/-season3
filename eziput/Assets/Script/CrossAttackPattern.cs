using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AttackPatterns/Cross Pattern", fileName = "CrossPattern")]
public class CrossAttackPattern : AttackPatternBase
{
    public override List<Vector2Int> GetPattern(Vector2Int center)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (var offset in relativePositions)
        {
            positions.Add(center + offset);
        }
        return positions;
    }
}
