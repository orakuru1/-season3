// === File: AttackPatternBase.cs ===
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackPatternBase : ScriptableObject
{
    public string patternName = "New Pattern";
    public List<Vector2Int> relativePositions = new List<Vector2Int>() { Vector2Int.up };
    public bool isAreaAttack = false;

    public abstract List<Vector2Int> GetPattern(Vector2Int center);
}