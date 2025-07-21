using SFML.System;
using SoftBody;
using Softine.Utils;
using SoftyEngine.ECS;

namespace Softine
{
    [Serializable]
    public class SoftBodyComponent : PhysicsComponent
    {
        public List<SpringBaseComponent>? Springs { get; protected init; }
        public List<PointMassComponent> Frame { get; private set; }
        public SoftBodyType BodyType { get; private set; }

        public BoundingBox BoundingBox { get; set; }

        public SoftBodyComponent(SoftBodyType bodyType)
        {
            PhysicsSystem.Register(this);
            CollisionSystem.Register(this);
            Points = new List<PointMassComponent>();
            Springs = new List<SpringBaseComponent>();
            Frame = new List<PointMassComponent>();
            BodyType = bodyType;
            NumSize = 5;
        }

        public override void SetOwner(Entity entity)
        {
            base.SetOwner(entity);
            Entity.TryGetComponent<Transform>(out var trf);
            if (trf == null)
                throw new Exception("NO TRANSFORM COMPONENT ERROR");
            float size = trf.Scale.X;
            float halfSize = size / 2;
            var componentPosition = trf.Position;
            var center = componentPosition;
            var radius = trf.Scale.X;
            var numSides = NumSize;

            switch (BodyType)
            {
                case SoftBodyType.CIRCLE:
                    var numPoints = 8;
                    for (var i = 0; i < numPoints; i++)
                    {
                        var angle = i * (360f / numPoints);
                        var angleRad = angle * (float)Math.PI / 180f;

                        Vector2f position = new Vector2f(
                            center.X + radius * (float)Math.Cos(angleRad),
                            center.Y + radius * (float)Math.Sin(angleRad)
                        );

                        Points.Add(new PointMassComponent(1f, position, true));
                    }

                    for (var i = 0; i < numPoints; i++)
                        Springs.Add(new SpringComponent(Points[i], Points[(i + 1) % numPoints], 150f, 1f));
                    for (var i = 0; i < numPoints; i++)
                    {
                        for (var j = 1; j <= 3; j++)
                        {
                            var oppositeIndex = (i + j * (numPoints / 3)) % numPoints;
                            Springs.Add(new SpringComponent(Points[i], Points[oppositeIndex], 150f, 1f));
                        }
                    }

                    // for (int i = 0; i < numPoints; i++)
                    //     for (int j = i + 1; j < numPoints; j++) 
                    //         Springs.Add(new SpringComponent(Points[i], Points[j], 10f, 1f));
                    break;
                case SoftBodyType.SQUARE:
                    Points.Add(new PointMassComponent(1f, new Vector2f(center.X - halfSize, center.Y - halfSize),
                        true));
                    Points.Add(new PointMassComponent(1f, new Vector2f(center.X + halfSize, center.Y - halfSize),
                        true));
                    Points.Add(new PointMassComponent(1f, new Vector2f(center.X - halfSize, center.Y + halfSize),
                        true));
                    Points.Add(new PointMassComponent(1f, new Vector2f(center.X + halfSize, center.Y + halfSize),
                        true));
                    Frame.Add(new PointMassComponent(1f, new Vector2f(center.X - halfSize, center.Y - halfSize),
                        true));
                    Frame.Add(new PointMassComponent(1f, new Vector2f(center.X + halfSize, center.Y - halfSize),
                        true));
                    Frame.Add(new PointMassComponent(1f, new Vector2f(center.X - halfSize, center.Y + halfSize),
                        true));
                    Frame.Add(new PointMassComponent(1f, new Vector2f(center.X + halfSize, center.Y + halfSize),
                        true));

                    Springs.Add(new SpringComponent(Points[0], Points[1], 125f, 2.5f));
                    Springs.Add(new SpringComponent(Points[1], Points[3], 125f, 2.5f));
                    Springs.Add(new SpringComponent(Points[3], Points[2], 125f, 2.5f));
                    Springs.Add(new SpringComponent(Points[3], Points[0], 125f, 2.5f));
                    Springs.Add(new SpringComponent(Points[0], Points[2], 125f, 2.5f));
                    Springs.Add(new SpringComponent(Points[2], Points[1], 125f, 2.5f));

                    break;
                case SoftBodyType.TRIANGLE:
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = i * (360f / 3);
                        float angleRad = angle * (float)Math.PI / 180f;

                        Vector2f position = new Vector2f(
                            center.X + radius * (float)Math.Cos(angleRad),
                            center.Y + radius * (float)Math.Sin(angleRad)
                        );

                        Points.Add(new PointMassComponent(1f, position, true));
                    }

                    for (int i = 0; i < 3; i++)
                        Springs.Add(new SpringComponent(Points[i], Points[(i + 1) % 3], 110f, 1f));

                    break;
                case SoftBodyType.POLY:
                    for (int i = 0; i < numSides; i++)
                    {
                        float angle = i * (360f / numSides);
                        float angleRad = angle * (float)Math.PI / 180f;

                        Vector2f position = new Vector2f(
                            center.X + radius * (float)Math.Cos(angleRad),
                            center.Y + radius * (float)Math.Sin(angleRad)
                        );

                        Points.Add(new PointMassComponent(1f, position, true));
                    }

                    for (int i = 0; i < numSides; i++)
                    {
                        Springs.Add(new SpringComponent(Points[i], Points[(i + 1) % numSides], 110f, 1f));
                    }

                    for (int i = 0; i < numSides; i++)
                    {
                        for (int j = 1; j <= 6; j++)
                        {
                            int step = (numSides / 6 == 0) ? 1 : numSides / 6;
                            int targetIndex = (i + j * step) % numSides;
                            if (i != targetIndex)
                            {
                                Springs.Add(new SpringComponent(Points[i], Points[targetIndex], 110f, 1f));
                            }
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Points == null) return;
            foreach (var point in Points)
                entity.AddComponent(point);
            if (Springs == null) return;
            foreach (var spring in Springs)
                entity.AddComponent(spring);


            BoundingBox = new BoundingBox(ref componentPosition, trf.Scale);
            RendererComponent boundingBoxRender = new RendererComponent(BoundingBox.Shape);
            Entity.AddComponent(boundingBoxRender);
        }

        public Vector2f GetPosition()
        {
            if (Points.Count == 0)
                return new Vector2f(0, 0);
            var sum = new Vector2f(0, 0);
            foreach (var point in Points)
                sum += point.Position;
            var center = sum / Points.Count;
            return center;
        }

        public float GetRotation()
        {
            var center = GetPosition();
            var totalAngle = 0.0f;
            var count = 0;

            foreach (var point in Points)
            {
                foreach (var fpoint in Frame)
                {
                    var currentAngle = GetAngleBetweenPoints(point.Position, fpoint.Position, center);
                    totalAngle += currentAngle;
                    count++;
                }
            }

            var averageAngle = totalAngle / count;
            return averageAngle;
        }

        private float GetAngleBetweenPoints(Vector2f A, Vector2f B, Vector2f C)
        {
            Vector2f CA = A - C;
            Vector2f CB = B - C;
            var dotProduct = Vector2Utils.Dot(CA, CB);
            var magnitudeCA = Vector2Utils.Magnitude(CA);
            var magnitudeCB = Vector2Utils.Magnitude(CB);
            var cosineAngle = Math.Clamp(dotProduct / (magnitudeCA * magnitudeCB), -1f, 1f);
            var angle = (float)Math.Acos(cosineAngle);
            return angle;
        }

   
        public override void Destroy()
        {
            PhysicsSystem.UnRegister(this);
            CollisionSystem.UnRegister(this);
        }
    }

    public interface IPhysicsComponent
    {
    }

    public enum SoftBodyType
    {
        CIRCLE,
        SQUARE,
        TRIANGLE,
        POLY
    }
}