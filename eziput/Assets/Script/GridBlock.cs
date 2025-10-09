// === File: GridBlock.cs ===
using UnityEngine;

public class GridBlock : MonoBehaviour
{
    public Vector2Int gridPos; // �O���b�h���W
    public bool isWalkable = true;
    public Unit occupantUnit = null; // ��L���j�b�g
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

    public void Highlight(bool on, Color? color = null)
    {
        if (blockRenderer == null) return;
        blockRenderer.material.color = on ? (color ?? new Color(0, 0.5f, 1f, 0.6f)) : originalColor;
    }

    public void SetColor(Color c)
    {
        if (blockRenderer == null) return;
        blockRenderer.material.color = c;
    }

    public void ResetColor()
    {
        if (blockRenderer == null) return;
        blockRenderer.material.color = originalColor;
    }
}
