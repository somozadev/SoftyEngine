using SFML.System;

namespace Softine.Utils;

public class SpatialHashGrid<T>
{
    private readonly Dictionary<Vector2i, List<T>> _cells = new();
    private readonly float _cellSize;

    public SpatialHashGrid(float cellSize)
    {
        _cellSize = cellSize;
    }

    public void Clear()
    {
        _cells.Clear();
    }

    public void Insert(T item, BoundingBox bounds)
    {
        Vector2i minCell = WorldToGrid(bounds.Position);
        Vector2i maxCell = WorldToGrid(bounds.Position + bounds.Size);

        for (int x = minCell.X; x <= maxCell.X; x++)
        {
            for (int y = minCell.Y; y <= maxCell.Y; y++)
            {
                Vector2i cell = new Vector2i(x, y);
                if (!_cells.ContainsKey(cell))
                {
                    _cells[cell] = new List<T>();
                }
                _cells[cell].Add(item);
            }
        }
    }

    public List<T> Query(BoundingBox bounds)
    {
        List<T> results = new();
        Vector2i minCell = WorldToGrid(bounds.Position);
        Vector2i maxCell = WorldToGrid(bounds.Position + bounds.Size);

        for (int x = minCell.X; x <= maxCell.X; x++)
        {
            for (int y = minCell.Y; y <= maxCell.Y; y++)
            {
                Vector2i cell = new Vector2i(x, y);
                if (_cells.TryGetValue(cell, out var items))
                {
                    results.AddRange(items);
                }
            }
        }
        return results;
    }

    private Vector2i WorldToGrid(Vector2f position)
    {
        return new Vector2i((int)(position.X / _cellSize), (int)(position.Y / _cellSize));
    }
}
