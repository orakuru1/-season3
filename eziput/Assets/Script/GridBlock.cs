// === File: GridBlock.cs ===
using UnityEngine;

public class GridBlock : MonoBehaviour
{
    public Vector2Int gridPos; // グリッド座標
    public bool isWalkable = true;
    public Unit occupantUnit = null; // 占有ユニット
    public bool isRamp = false;
    public bool hasEvent = false;
    public string eventID = "";

    private Renderer blockRenderer;
    private Color originalColor;

    // Trap関連
    public TrapType trapType = TrapType.None;
    public int trapValue = 0;
    public bool trapConsumed = false;
    public AudioClip ArrowTrapSE;

    private void Start()
    {
        blockRenderer = GetComponent<Renderer>();
        if (blockRenderer != null) originalColor = blockRenderer.material.color;
    }

    public void SetOccupied(Unit unit)
    {
        occupantUnit = unit;
    }

    public void ClearOccupied()
    {
        if (occupantUnit != null) occupantUnit = null;
    }

    public void Highlight(Color color)
    {
        if (blockRenderer != null)
            blockRenderer.material.color = color;
    }
    public void SetHighlight(Color color)
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = color;
    }

    public void SetColor(Color c)
    {
        if (blockRenderer == null) return;
        blockRenderer.material.color = c;
    }

    public void ResetHighlight()
    {
        GetComponent<Renderer>().material.color = originalColor;
    }

    public void OnDrawGizmos()//デバッグ用
    {
        if (trapType == TrapType.None) return;

        Gizmos.color = trapType == TrapType.Damage ? Color.red : Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);
    }


}
