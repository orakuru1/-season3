using System.Collections.Generic;
using UnityEngine;

public abstract class AttackPatternBase : ScriptableObject
{
    public string patternName = "New Pattern";
    public List<Vector2Int> relativePositions = new List<Vector2Int>() { Vector2Int.up };
    public bool isAreaAttack = false;

    public virtual List<Vector2Int> GetPattern(Vector2Int center)
    {
        var result = new List<Vector2Int>();
        foreach (var offset in relativePositions)
        {
            result.Add(center + offset);
        }
        return result;
    }
}
