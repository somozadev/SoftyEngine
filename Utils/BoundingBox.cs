using SFML.Graphics;
using SFML.System;

namespace Softine.Utils;

public class BoundingBox
{
    public Vector2f Position { get; set; }
    public Vector2f Size { get; }

    public Vector2f Min => new(Position.X, Position.Y);

    public Vector2f Max => new(Position.X + Size.X, Position.Y + Size.Y);

    public Shape Shape { get; private set; }
    public BoundingBox(ref Vector2f position, Vector2f size)
    {
        Position = position;
        Size = size;
        var squareShape = new RectangleShape();
        // squareShape.Position = Position;
        // squareShape.Scale = new Vector2f(1, 1);
        // squareShape.OutlineColor = Color.Magenta;
        // squareShape.Origin = new Vector2f(size.X/2, size.Y/2);
        // squareShape.Size =  Size;
        Shape = squareShape;
        
    }

    public bool Intersects(BoundingBox other)
    {
        return !(Position.X + Size.X < other.Position.X ||
                 Position.X > other.Position.X + other.Size.X ||
                 Position.Y + Size.Y < other.Position.Y ||
                 Position.Y > other.Position.Y + other.Size.Y);
    }
}