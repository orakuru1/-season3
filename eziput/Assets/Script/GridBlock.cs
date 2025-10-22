// === File: GridBlock.cs ===
using UnityEngine;

public class GridBlock : MonoBehaviour
{
    public Vector2Int gridPos; // グリッド座標
    public bool isWalkable = true;
    public Unit occupantUnit = null; // 占有ユニット
    public bool isRamp = false;

    private Renderer blockRenderer;
    private Color originalColor;

    private void Awake()
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

    public void SetColor(Color c)
    {
        if (blockRenderer == null) return;
        blockRenderer.material.color = c;
    }

    public void ResetHighlight()
    {
        GetComponent<Renderer>().material.color = originalColor;
    }
}
