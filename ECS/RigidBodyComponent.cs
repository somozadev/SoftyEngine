using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using SFML.Graphics;
using SFML.System;
using SoftBody;
using Softine.Utils;
using SoftyEngine.ECS;
using Transform = SoftyEngine.ECS.Transform;

namespace Softine
{
    [Serializable]
    public class RigidBodyComponent : PhysicsComponent
    {
        public RigidBodyType BodyType { get; private set; }
        public Shape Shape { get; private set; }
        public float Size { get; private set; }
        public float HalfSize { get; private set; }
        public float Radius { get; private set; }
        public Vector2f Center { get; private set; }

        private const float min = 1.05f;
        private const float max = 4.5f;

        public RigidBodyComponent(RigidBodyType bodyType)
        {
            PhysicsSystem.Register(this);
            RigidCollisionSystem.Register(this);
            Points = new List<PointMassComponent>();
            BodyType = bodyType;
            NumSize = 5;
        }

        public override void SetOwner(Entity entity)
        {
            base.SetOwner(entity);
            Entity.TryGetComponent<Transform>(out var trf);
            if (trf == null)
                throw new Exception("NO TRANSFORM COMPONENT ERROR");
            Random rnd = new Random();
            trf.Scale = new Vector2f(1, 1) * (min + rnd.NextSingle() * (max - min));
            Size = trf.Scale.X;
            HalfSize = Size / 2;
            Center = trf.Position;
            Radius = trf.Scale.X;
            var numSides = NumSize;
            switch (BodyType)
            {
                case RigidBodyType.CIRCLE:
                    var circleShape = new CircleShape();
                    circleShape.Position = Center;
                    circleShape.Origin = new Vector2f(Radius, Radius);
                    circleShape.Radius = Radius;
                    var cp = new PointMassComponent(1f, Center, true, Size);
                    cp.UpdateInertia("circle");
                    Points?.Add(cp);
                    Shape = circleShape;


                    break;
                case RigidBodyType.SQUARE:
                    var squareShape = new RectangleShape();
                    squareShape.Position = Center;
                    squareShape.Scale = new Vector2f(1, 1);
                    squareShape.OutlineColor = Color.Magenta;
                    squareShape.Size = new Vector2f(1, 1) * Size;
                    var sp = new PointMassComponent(1f, Center, true, Size);
                    sp.UpdateInertia("square");
                    Points?.Add(sp);
                    Shape = squareShape;

                    break;
                case RigidBodyType.TRIANGLE:
                    var shape = new ConvexShape(3)
                    {
                        FillColor = Color.Magenta,
                        Position = Center
                    };
                    shape.SetPoint(0, new Vector2f(Center.X, Center.Y - Size / 2));
                    shape.SetPoint(1, new Vector2f(Center.X - Size / 2, Center.Y + Size / 2));
                    shape.SetPoint(2, new Vector2f(Center.X + Size / 2, Center.Y + Size / 2));

                    Shape = shape;
                    var tp = new PointMassComponent(1f, Center, true, Size);
                    tp.UpdateInertia("triangle");
                    Points?.Add(tp);
                    break;
                case RigidBodyType.POLY:
                    var polyShape = new ConvexShape((uint)NumSize)
                    {
                        FillColor = Color.Magenta,
                        Position = Center
                    };
                    for (int i = 0; i < numSides; i++)
                    {
                        float angle = i * (360f / numSides);
                        float angleRad = angle * (float)Math.PI / 180f;

                        Vector2f position = new Vector2f(
                            Center.X + Radius * (float)Math.Cos(angleRad),
                            Center.Y + Radius * (float)Math.Sin(angleRad)
                        );
                        polyShape.SetPoint((uint)i, position);
                    }

                    Shape = polyShape;
                    var pp = new PointMassComponent(1f, Center, true, Size);
                    pp.UpdateInertia("polygon");
                    Points?.Add(pp);  
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Shape.OutlineColor = new Color(
                (byte)rnd.Next(256),
                (byte)rnd.Next(256),
                (byte)rnd.Next(256)
            );
            Shape.OutlineThickness = 1f;
            Shape.FillColor = GameState.windowColor;

            if (Points == null) return;
            foreach (var point in Points)
                entity.AddComponent(point);
        }
    }

    public enum RigidBodyType
    {
        CIRCLE,
        SQUARE,
        TRIANGLE,
        POLY
    }
}