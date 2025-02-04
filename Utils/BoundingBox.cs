using SFML.System;

namespace Softine.Utils;

public class BoundingBox
{
    public Vector2f Position { get; }
    public Vector2f Size { get; }

    public BoundingBox(Vector2f position, Vector2f size)
    {
        Position = position;
        Size = size;
    }

    public bool Intersects(BoundingBox other)
    {
        return !(Position.X + Size.X < other.Position.X ||
                 Position.X > other.Position.X + other.Size.X ||
                 Position.Y + Size.Y < other.Position.Y ||
                 Position.Y > other.Position.Y + other.Size.Y);
    }
}
