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
        public float Area { get; private set; }

        private const float min = 60.05f;
        private const float max = 60.5f;

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
                    cp.ColliderArea = Radius;
                    Points?.Add(cp);
                    Shape = circleShape;
                    Area = (float)(Math.PI * Radius * Radius); //pi * r * r

                    break;
                case RigidBodyType.SQUARE:
                    var squareShape = new RectangleShape();
                    squareShape.Position = Center;
                    squareShape.Scale = new Vector2f(1, 1);
                    squareShape.OutlineColor = Color.Magenta;
                    squareShape.Origin = new Vector2f(HalfSize, HalfSize);
                    squareShape.Size = new Vector2f(1, 1) * Size;
                    var sp = new PointMassComponent(1f, Center, true, Size);
                    sp.UpdateInertia("square");
                    sp.ColliderArea = HalfSize;
                    Points?.Add(sp);
                    Shape = squareShape;
                    Area = (Size * Size); // l * l // b * h 
                    break;
                case RigidBodyType.TRIANGLE:
                    var shape = new ConvexShape(3)
                    {
                        FillColor = Color.Magenta,
                        Position = Center
                    };
                    Vector2f p1 = new Vector2f(Center.X, Center.Y - Size / 2);
                    Vector2f p2 = new Vector2f(Center.X - Size / 2, Center.Y + Size / 2);
                    Vector2f p3 = new Vector2f(Center.X + Size / 2, Center.Y + Size / 2);
                    shape.SetPoint(0, p1);
                    shape.SetPoint(1, p2);
                    shape.SetPoint(2, p3);

                    Vector2f triangleCenter = new Vector2f(
                        (p1.X + p2.X + p3.X) / 3f,
                        (p1.Y + p2.Y + p3.Y) / 3f
                    );
                    shape.Origin = triangleCenter;
                    shape.Position = Center;
                    Shape = shape;
                    var tp = new PointMassComponent(1f, Center, true, Size);
                    tp.ColliderArea = HalfSize;
                    tp.UpdateInertia("triangle");
                    Points?.Add(tp);

                    Area = 0.5f * Math.Abs(
                        p1.X * (p2.Y - p3.Y) +
                        p2.X * (p3.Y - p1.Y) +
                        p3.X * (p1.Y - p2.Y)
                    );
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

                    var polygonCenter = new Vector2f(0f, 0f);
                    for (int i = 0; i < numSides; i++)
                    {
                        polygonCenter += polyShape.GetPoint((uint)i);
                    }

                    polygonCenter /= numSides;
                    polyShape.Origin = polygonCenter;
                    polyShape.Position = Center;
                    Shape = polyShape;
                    var pp = new PointMassComponent(1f, Center, true, Size);
                    pp.UpdateInertia("polygon");
                    Points?.Add(pp);
                    float area = 0f;
                    for (int i = 0; i < numSides; i++)
                    {
                        Vector2f po1 = polyShape.GetPoint((uint)i);
                        Vector2f po2 = polyShape.GetPoint((uint)((i + 1) % numSides));

                        area += (po1.X * po2.Y) - (po2.X * po1.Y);
                    }

                    area = Math.Abs(area) * 0.5f;

                    Area = area;
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

   
        public override void Destroy()
        {
            PhysicsSystem.UnRegister(this);
            RigidCollisionSystem.UnRegister(this);
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