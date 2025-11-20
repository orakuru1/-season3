using System.Collections.Generic;
using UnityEngine;

public static class GridLineUtility
{
    // Bresenham's line algorithm
    public static List<Vector2Int> GetLine(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> line = new List<Vector2Int>();

        int x = start.x;
        int y = start.y;

        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);

        int sx = start.x < end.x ? 1 : -1;
        int sy = start.y < end.y ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            line.Add(new Vector2Int(x, y));

            if (x == end.x && y == end.y)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        return line;
    }
}
