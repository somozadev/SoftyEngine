using SFML.System;
using Softine;

namespace SoftyEngine.ECS;

public class Transform : Component
{
    public Transform()
    {
        TransformSystem.Register(this);
    }

    public Transform(Vector2f position, float scale = 1)
    {
        Position = position;
        ScaleBy(scale);
        TransformSystem.Register(this);
    }

    public Vector2f Position { get; set; } = new Vector2f(0, 0);
    public float Rotation { get; set; } = 0f;
    public Vector2f Scale { get; set; } = new Vector2f(1, 1);
    public Transform Parent { get; set; }


    public Vector2f GlobalPosition => Parent.GlobalPosition + Position;
    public float GlobalRotation => Parent.GlobalRotation + Rotation;
    public Vector2f GlobalScale => new(Parent.GlobalScale.X * Scale.X, Parent.GlobalScale.Y * Scale.Y);

    public void Translate(Vector2f translation)
    {
        Position += translation;
    }

    public void Rotate(float angle)
    {
        Rotation += angle;
    }

    public void SetScale(Vector2f scale)
    {
        Scale = scale;
    }

    public void ScaleBy(float scaleFactor)
    {
        Scale = new Vector2f(Scale.X * scaleFactor, Scale.Y * scaleFactor);
    }
}