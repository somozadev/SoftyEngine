using SFML.Graphics;
using SFML.System;
using SFML.Window;
using SoftBody;
using Softine;
using Softine.Utils;

namespace SoftyEngine.ECS;

public abstract class System<T> where T : Component
{
    private static List<T> Components = new List<T>();

    public static void Register(T component)
    {
        Components.Add(component);
    }

    public static List<T> GetAllComponents()
    {
        return Components;
    }

    public virtual void Update(float deltaTime)
    {
        foreach (var component in Components)
        {
            ProcessUpdate(component, deltaTime);
        }
    }

    protected abstract void ProcessUpdate(T component, float deltaTime);

    public virtual void FixedUpdate(float fixedDeltaTime)
    {
        foreach (var component in Components)
        {
            ProcessFixedUpdate(component, fixedDeltaTime);
        }
    }

    protected abstract void ProcessFixedUpdate(T component, float fixedDeltaTime);
}

public class TransformSystem : System<Transform>, ISystem
{
    protected override void ProcessUpdate(Transform component, float deltaTime)
    {
        return;
    }

    protected override void ProcessFixedUpdate(Transform component, float fixedDeltaTime)
    {
        return;
    }
}

public class RenderSystem : System<RendererComponent>, ISystem
{
    private RenderWindow _window;

    public RenderSystem(RenderWindow window)
    {
        _window = window;
    }

    protected override void ProcessUpdate(RendererComponent component, float deltaTime)
    {
        // if (component.IsShape)
        //     _window.Draw(component.Drawable);
        // else
        //     _window.Draw(component.VDrawable);
    }

    protected override void ProcessFixedUpdate(RendererComponent component, float fixedDeltaTime)
    {
        return;
    }
}

public class ComplexRenderSystem : System<ComplexRendererComponent>, ISystem
{
    private RenderWindow _window;

    public ComplexRenderSystem(RenderWindow window)
    {
        _window = window;
    }

    protected override void ProcessUpdate(ComplexRendererComponent component, float deltaTime)
    {
        component.Entity.TryGetComponent<SoftBodyComponent>(out var softBody);
        var points = component.Entity.GetComponent<SoftBodyComponent>().Points;
        var springs = component.Entity.GetComponent<SoftBodyComponent>().Springs;
        var pointsL = softBody.Points.Count;
        var springsL = softBody.Springs.Count;

        if (component.RendererComponents.Count != (pointsL + springsL))
        {
            component.RendererComponents = new List<RendererComponent>();
            foreach (var point in points)
            {
                component.RendererComponents.Add(
                    new RendererComponent(Shapering.CreateCircle(10f, null, point.Position)));
            }

            foreach (var spring in springs)
            {
                component.RendererComponents.Add(
                    new RendererComponent(Shapering.CreateLine(spring.PointA.Position, spring.PointB.Position)));
            }

            foreach (var c in component.RendererComponents)
                component.Entity.AddComponent(c);
        }

        foreach (var renderer in component.RendererComponents)
        {
            if (renderer.IsShape)
            {
                foreach (var point in softBody.Points)
                {
                    renderer.Drawable.Position = point.Position;
                    _window.Draw(renderer.Drawable);
                }
            }

            if (!renderer.IsShape)
            {
                foreach (var spring in softBody.Springs)
                {
                    renderer.VDrawable = Shapering.CreateLine(spring.PointA.Position, spring.PointB.Position);
                    _window.Draw(renderer.VDrawable);
                }
            }
        }
    }

    protected override void ProcessFixedUpdate(ComplexRendererComponent component, float fixedDeltaTime)
    {
        return;
    }
}

public class CollisionSystem : System<SoftBodyComponent>, IPhysicsSystem
{
    protected override void ProcessUpdate(SoftBodyComponent component, float deltaTime)
    {
    }

    protected override void ProcessFixedUpdate(SoftBodyComponent component, float fixedDeltaTime)
    {
        DetectAndResolveCollisions(component);
    }

    private void DetectAndResolveCollisions(SoftBodyComponent currentBody)
    {
        var allBodies = GetAllComponents();

        foreach (var otherBody in allBodies)
        {
            if (currentBody == otherBody) continue;
            DetectPointToPointCollisions(currentBody, otherBody);
            DetectPointToEdgeCollisions(currentBody, otherBody);
            DetectEdgeToEdgeCollisions(currentBody, otherBody);
        }
    }

    private void DetectPointToPointCollisions(SoftBodyComponent bodyA, SoftBodyComponent bodyB)
    {
        foreach (var pointA in bodyA.Points)
        {
            foreach (var pointB in bodyB.Points)
            {
                Vector2f direction = pointB.Position - pointA.Position;
                float distance = Vector2Utils.Magnitude(direction);
                float minDistance = 10 + 10; //pointA.Radius + pointB.Radius

                if (distance < minDistance && distance > 0.0001f)
                {
                    Vector2f normal = direction / distance;
                    float overlap = minDistance - distance;

                    pointA.Position -= normal * (overlap / 2f);
                    pointB.Position += normal * (overlap / 2f);

                    ApplyCollisionResponse(pointA, pointB, normal);
                }
            }
        }
    }

    private void DetectPointToEdgeCollisions(SoftBodyComponent bodyA, SoftBodyComponent bodyB)
    {
        foreach (var point in bodyA.Points)
        {
            foreach (var spring in bodyB.Springs)
            {
                Vector2f closestPoint =
                    ClosestPointOnSegment(point.Position, spring.PointA.Position, spring.PointB.Position);
                Vector2f direction = point.Position - closestPoint;
                float distance = Vector2Utils.Magnitude(direction);

                float minDistance = 10; //point.Radius;
                if (distance < minDistance && distance > 0.0001f)
                {
                    Vector2f normal = direction / distance;
                    float overlap = minDistance - distance;

                    point.Position += normal * overlap;

                    Vector2f relativeVelocity = point.Velocity - new Vector2f(0, 0);
                    float velocityAlongNormal = Vector2Utils.Dot(relativeVelocity, normal);

                    if (velocityAlongNormal < 0)
                    {
                        float restitution = 0.5f;
                        Vector2f impulse = -(1 + restitution) * velocityAlongNormal * normal;
                        point.Velocity += impulse / point.Mass;
                    }
                }
            }
        }
    }

    private void DetectEdgeToEdgeCollisions(SoftBodyComponent bodyA, SoftBodyComponent bodyB)
    {
        foreach (var springA in bodyA.Springs)
        {
            foreach (var springB in bodyB.Springs)
            {
                if (SegmentsIntersect(
                        springA.PointA.Position, springA.PointB.Position,
                        springB.PointA.Position, springB.PointB.Position,
                        out Vector2f intersectionPoint))
                {
                    // Resolver colisión en el punto de intersección
                    Vector2f direction = springB.PointA.Position - springA.PointA.Position;
                    float overlap = Vector2Utils.Magnitude(direction);

                    if (overlap > 0.0001f)
                    {
                        Vector2f normal = direction / overlap;

                        // Ajustar posiciones
                        springA.PointA.Position -= normal * (overlap / 2f);
                        springB.PointA.Position += normal * (overlap / 2f);

                        // Aplicar respuesta física si es necesario (rebotes)
                        ApplyCollisionResponse(springA.PointA, springB.PointA, normal);
                    }
                }
            }
        }
    }

    private void ApplyCollisionResponse(PointMassComponent pointA, PointMassComponent pointB, Vector2f normal)
    {
        Vector2f relativeVelocity = pointB.Velocity - pointA.Velocity;
        float velocityAlongNormal = Vector2Utils.Dot(relativeVelocity, normal);

        if (velocityAlongNormal > 0) return;

        float restitution = 0.5f;
        float impulseScalar = -(1 + restitution) * velocityAlongNormal;
        impulseScalar /= (1 / pointA.Mass) + (1 / pointB.Mass);

        Vector2f impulse = impulseScalar * normal;

        pointA.Velocity -= impulse / pointA.Mass;
        pointB.Velocity += impulse / pointB.Mass;
    }

    private Vector2f ClosestPointOnSegment(Vector2f point, Vector2f segmentStart, Vector2f segmentEnd)
    {
        Vector2f segment = segmentEnd - segmentStart;
        float segmentLengthSquared = Vector2Utils.SqrMagnitude(segment);

        if (segmentLengthSquared == 0) return segmentStart;

        float t = Vector2Utils.Dot(point - segmentStart, segment) / segmentLengthSquared;
        t = Math.Clamp(t, 0, 1);

        return segmentStart + t * segment;
    }

    private bool SegmentsIntersect(
        Vector2f p1, Vector2f q1, Vector2f p2, Vector2f q2,
        out Vector2f intersection)
    {
        intersection = new Vector2f();

        Vector2f r = q1 - p1;
        Vector2f s = q2 - p2;

        float rxs = Vector2Utils.Cross(r, s);
        float qpCrossR = Vector2Utils.Cross(p2 - p1, r);

        if (Math.Abs(rxs) < 0.0001f && Math.Abs(qpCrossR) < 0.0001f)
        {
            // Colineales
            return false;
        }

        if (Math.Abs(rxs) < 0.0001f)
        {
            // Paralelos
            return false;
        }

        float t = Vector2Utils.Cross(p2 - p1, s) / rxs;
        float u = Vector2Utils.Cross(p2 - p1, r) / rxs;

        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            intersection = p1 + t * r;
            return true;
        }

        return false;
    }
}

public class PhysicsSystem : System<SoftBodyComponent>, IPhysicsSystem
{
    private Vector2f _gravity = new(0, 200f);

    protected override void ProcessUpdate(SoftBodyComponent component, float deltaTime)
    {
        return;
    }

    protected override void ProcessFixedUpdate(SoftBodyComponent component, float fixedDeltaTime)
    {
        ApplyGravity(component);
        ApplySpringForce(component);
        foreach (var point in component.Points)
            UpdatePosition(point, fixedDeltaTime);
        foreach (var point in component.Points)
            LimitToScreen(point);
    }

    private void LimitToScreen(PointMassComponent component)
    {
        var componentPosition = component.Position;
        var componentVelocity = component.Velocity;
        if (componentPosition.X < 0)
        {
            componentPosition.X = 0;
            componentVelocity.X = 0;
        }
        else if (componentPosition.X > GameState.windowWidth)
        {
            componentPosition.X = GameState.windowWidth;
            componentVelocity.X = 0;
        }

        if (componentPosition.Y < 0)
        {
            componentPosition.Y = 0;
            componentVelocity.Y = 0;
        }
        else if (componentPosition.Y > GameState.windowHeight)
        {
            componentPosition.Y = GameState.windowHeight;
            componentVelocity.Y = 0;
        }

        component.Position = componentPosition;
        component.Velocity = componentVelocity;
    }

    private void UpdatePosition(PointMassComponent component, float fixedDeltaTime)
    {
        component.Velocity += component.Acceleration * fixedDeltaTime;
        component.Position += component.Velocity * fixedDeltaTime;
        component.Acceleration = new Vector2f(0, 0);
    }

    private void AddForce(PointMassComponent component, Vector2f force)
    {
        if (!component.ForcesApplyOnIt) return;
        component.Acceleration += force / component.Mass;
    }

    private void ApplySpringForce(SoftBodyComponent component)
    {
        
        foreach (var spring in component.Springs)
        {
            var direction = spring.PointB.Position - spring.PointA.Position;
            var currentLength = Vector2Utils.Magnitude(direction);
            if (currentLength > 0.0001f)
            {
                direction /= currentLength;
                float forceMagnitude = spring.Stiffness * (currentLength - spring.RestLength);
                Vector2f force = forceMagnitude * direction;
        
                AddForce(spring.PointA, force);
                AddForce(spring.PointB, -force);
        
                Vector2f relativeVelocity = spring.PointB.Velocity - spring.PointA.Velocity;
                float dampingMagnitude = spring.Damping * Vector2Utils.Dot(relativeVelocity, direction);
                Vector2f dampingForce = dampingMagnitude * direction;
        
                AddForce(spring.PointA, dampingForce);
                AddForce(spring.PointB, -dampingForce);
            }
        }
    }

    private void ApplyGravity(SoftBodyComponent component)
    {
        foreach (var point in component.Points)
            AddForce(point, _gravity);
    }
}