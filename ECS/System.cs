using SFML.Graphics;
using SFML.System;
using SFML.Window;
using SoftBody;
using Softine;
using Softine.Utils;
using System.Threading.Tasks;

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
        if (component.Entity.TryGetComponent<SoftBodyComponent>(out var softBody)) return;
        var point = component.Entity.GetComponent<Transform>().Position;
        component.Drawable.Position = point;

        if (component.IsShape)
        {
            _window.Draw(component.Drawable);
        }
        else
        {
            _window.Draw(component.VDrawable);
        }
    }

    protected override void ProcessFixedUpdate(RendererComponent component, float fixedDeltaTime)
    {
        return;
    }
}

public class SoftRenderSystem : System<SoftRendererComponent>, ISystem
{
    private RenderWindow _window;

    public SoftRenderSystem(RenderWindow window)
    {
        _window = window;
    }

    protected override void ProcessUpdate(SoftRendererComponent component, float deltaTime)
    {
        if (!component.Entity.TryGetComponent(out SoftBodyComponent softBody)) return;
        var points = softBody.Points;
        var springs = softBody.Springs;
        int pointsL = points.Count;
        int springsL = springs.Count;

        if (component.RendererComponents.Count != (pointsL + springsL))
        {
            component.RendererComponents.Clear();
            foreach (var point in points)
            {
                var circleRenderer = new RendererComponent(Shapering.CreateCircle(10f, null, point.Position));
                component.RendererComponents.Add(circleRenderer);
                component.Entity.AddComponent(circleRenderer);
            }

            foreach (var spring in springs)
            {
                var lineRenderer =
                    new RendererComponent(Shapering.CreateLine(spring.PointA.Position, spring.PointB.Position));
                component.RendererComponents.Add(lineRenderer);
                component.Entity.AddComponent(lineRenderer);
            }
        }
        int index = 0;
        foreach (var point in points)
        {
            var renderer = component.RendererComponents[index++];
            renderer.Drawable.Position = point.Position;
            _window.Draw(renderer.Drawable);
        }
        foreach (var spring in springs)
        {
            var renderer = component.RendererComponents[index++];
            if (!renderer.IsShape )
            {
                renderer.VDrawable = Shapering.CreateLine(spring.PointA.Position, spring.PointB.Position);
            }
            _window.Draw(renderer.VDrawable);
        }
        
      
    }

    protected override void ProcessFixedUpdate(SoftRendererComponent component, float fixedDeltaTime)
    {
        return;
    }
}

public class RigidCollisionSystem : System<RigidBodyComponent>, IPhysicsSystem
{
    protected override void ProcessUpdate(RigidBodyComponent component, float deltaTime)
    {
    }

    protected override void ProcessFixedUpdate(RigidBodyComponent component, float fixedDeltaTime)
    {
        DetectAndResolveCollisions(component);
    }

    private void DetectAndResolveCollisions(RigidBodyComponent currentBody)
    {
        var allBodies = GetAllComponents();
        foreach (var otherBody in allBodies)
        {
            if (currentBody == otherBody) continue;
            if (currentBody.BodyType == RigidBodyType.CIRCLE && otherBody.BodyType == RigidBodyType.CIRCLE)
                DetectPointToPointCollisions(currentBody, otherBody);
            if (currentBody.BodyType == RigidBodyType.SQUARE && otherBody.BodyType == RigidBodyType.SQUARE)
                DetectEdgeToEdgeCollisions(currentBody, otherBody);
            // DetectPointToEdgeCollisions(currentBody, otherBody);
            // DetectEdgeToEdgeCollisions(currentBody, otherBody);
        }
    }

    private void DetectPointToPointCollisions(RigidBodyComponent bodyA, RigidBodyComponent bodyB)
    {
        var pointA = bodyA.Points[0];
        var pointB = bodyB.Points[0];

        Vector2f direction = pointB.Position - pointA.Position;
        float distance = Vector2Utils.Magnitude(direction);
        float minDistance = pointA.ColliderArea + pointB.ColliderArea;

        if (distance < minDistance && distance > 0.0001f)
        {
            Vector2f normal = direction / distance;
            float overlap = minDistance - distance;

            pointA.Position -= normal * (overlap / 2f);
            pointB.Position += normal * (overlap / 2f);

            ApplyCollisionResponse(pointA, pointB, normal);
        }
    }

    private void DetectEdgeToEdgeCollisions(RigidBodyComponent bodyA, RigidBodyComponent bodyB)
    {
        var trfA = bodyA.Entity.GetComponent<Transform>();
        var trfB = bodyB.Entity.GetComponent<Transform>();

        var pointA = bodyA.Points[0];
        var pointB = bodyB.Points[0];

        float leftA = trfA.Position.X - bodyA.HalfSize; //for now, only l=l cubes
        float rightA = trfA.Position.X + bodyA.HalfSize;
        float topA = trfA.Position.Y - bodyA.HalfSize;
        float bottomA = trfA.Position.Y + bodyA.HalfSize;

        float leftB = trfB.Position.X - bodyB.HalfSize;
        float rightB = trfB.Position.X + bodyB.HalfSize;
        float topB = trfB.Position.Y - bodyB.HalfSize;
        float bottomB = trfB.Position.Y + bodyB.HalfSize;

        if (rightA < leftB || leftA > rightB || bottomA < topB || topA > bottomB) return;


        float directionX = pointB.Position.X - pointA.Position.X;
        float directionY = pointB.Position.Y - pointA.Position.Y;

        float overlapX = (bodyA.HalfSize + bodyB.HalfSize) - Math.Abs(directionX);
        float overlapY = (bodyA.HalfSize + bodyB.HalfSize) - Math.Abs(directionY);
        Vector2f normal;
        if (overlapX < overlapY)
            normal = new Vector2f(directionX > 0 ? 1 : -1, 0);
        else
            normal = new Vector2f(0, directionY > 0 ? 1 : -1);

        float contactX = (Math.Max(leftA, leftB) + Math.Min(rightA, rightB)) * 0.5f;
        float contactY = (Math.Max(topA, topB) + Math.Min(bottomA, bottomB)) * 0.5f;
        Vector2f contactPoint = new Vector2f(contactX, contactY);
        ApplyCollisionResponse(pointA, pointB, contactPoint, normal);
    }

    private void ApplyCollisionResponse(PointMassComponent boxA, PointMassComponent boxB, Vector2f contactPoint,
        Vector2f normal)
    {
        var rA = contactPoint - boxA.Position;
        var rB = contactPoint - boxB.Position;

        Vector2f velocityA = boxA.Velocity + Vector2Utils.Cross(boxA.AngularVelocity, rA);
        Vector2f velocityB = boxB.Velocity + Vector2Utils.Cross(boxB.AngularVelocity, rB);

        Vector2f relativeVelocity = velocityB - velocityA;
        float velocityAlongNormal = Vector2Utils.Dot(relativeVelocity, normal);

        if (velocityAlongNormal > 0) return;

        // Coeficiente de restitución (elasticidad)
        float restitution = 0.5f;

        // Factor de impulso basado en masa y momento de inercia
        float invMassA = 1 / boxA.Mass;
        float invMassB = 1 / boxB.Mass;
        float invInertiaA = 1 / boxA.Inertia;
        float invInertiaB = 1 / boxB.Inertia;

        // Cálculo del denominador del impulso
        float rA_cross_N = Vector2Utils.Cross(rA, normal);
        float rB_cross_N = Vector2Utils.Cross(rB, normal);
        float denominator = invMassA + invMassB + (rA_cross_N * rA_cross_N) * invInertiaA +
                            (rB_cross_N * rB_cross_N) * invInertiaB;

        // Impulso escalar
        float impulseScalar = -(1 + restitution) * velocityAlongNormal / denominator;

        // Impulso total
        Vector2f impulse = impulseScalar * normal;

        // Aplicar el impulso lineal
        boxA.Velocity -= impulse * invMassA;
        boxB.Velocity += impulse * invMassB;

        // Aplicar el impulso angular
        boxA.AngularVelocity -= rA_cross_N * impulseScalar * invInertiaA;
        boxB.AngularVelocity += rB_cross_N * impulseScalar * invInertiaB;
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
            return false;
        }

        if (Math.Abs(rxs) < 0.0001f)
        {
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

public class PhysicsSystem : System<PhysicsComponent>, IPhysicsSystem
{
    private Vector2f _gravity = new(0, 600f);
    private const double TOLERANCE = 0.0001f;

    protected override void ProcessUpdate(PhysicsComponent component, float deltaTime)
    {
        return;
    }

    protected override void ProcessFixedUpdate(PhysicsComponent component, float fixedDeltaTime)
    {
        ApplyGravity(component);
        if (component.GetType() == typeof(SoftBodyComponent))
            ApplySpringForce((SoftBodyComponent)component);
        if (component.Points == null) return;
        Parallel.ForEach(component.Points, point => UpdatePosition(point, fixedDeltaTime));
        Parallel.ForEach(component.Points, LimitToScreen);
    }

    public void ApplyExplosionForce(Vector2f explosionCenter, float explosionStrength, float explosionRadius)
    {
        var allRigidBodies = GetAllComponents();

        foreach (var rigidBody in allRigidBodies)
        {
            if (rigidBody.Entity.TryGetComponent(out PointMassComponent component))
            {
                var pos = component.Position;
                var distance = Vector2Utils.Magnitude(pos - explosionCenter);
                if (distance > explosionRadius)
                    continue;

                Vector2f direction = pos - explosionCenter;
                float distanceNormalized = distance / explosionRadius;
                float forceMagnitude = explosionStrength * (1 - distanceNormalized);
                Vector2f force = forceMagnitude * Vector2Utils.Normalize(direction);
                component.Velocity += force / component.Mass;
            }
        }
    }

    private void LimitToScreen(PointMassComponent component)
    {
        var componentPosition = component.Position;
        var componentVelocity = component.Velocity;
        if (componentPosition.X - component.ColliderArea < 0)
        {
            componentPosition.X = 0 + component.ColliderArea;
            componentVelocity.X = 0;
        }
        else if (componentPosition.X + component.ColliderArea > GameState.windowWidth)
        {
            componentPosition.X = GameState.windowWidth - component.ColliderArea;
            componentVelocity.X = 0;
        }

        if (componentPosition.Y - component.ColliderArea < 0)
        {
            componentPosition.Y = 0 + component.ColliderArea;
            componentVelocity.Y = 0;
        }
        else if (componentPosition.Y + component.ColliderArea > GameState.windowHeight)
        {
            componentPosition.Y = GameState.windowHeight - component.ColliderArea;
            componentVelocity.Y = 0;
        }

        component.Position = componentPosition;
        component.Velocity = componentVelocity;
    }

    private void UpdatePosition(PointMassComponent component, float fixedDeltaTime)
    {
        component.Velocity += component.Acceleration * fixedDeltaTime;
        component.Position += component.Velocity * fixedDeltaTime;
        component.Entity.GetComponent<Transform>().Position = component.Position;
        float rotationAngle = component.AngularVelocity * fixedDeltaTime;
        component.Entity.GetComponent<Transform>().Rotation += rotationAngle;
        component.Acceleration = new Vector2f(0, 0);
    }

    private void AddForce(PointMassComponent component, Vector2f force)
    {
        if (!component.ForcesApplyOnIt) return;
        component.Acceleration += force / component.Mass;
    }

    private void AddGravity(PointMassComponent component, Vector2f force)
    {
        if (!component.ForcesApplyOnIt) return;
        component.Acceleration += force; //* component.Mass;
    }

    private void ApplySpringForce(SoftBodyComponent component)
    {
        if (component.Springs != null)
            Parallel.ForEach(component.Springs, spring =>
            {
                if (spring is not SpringComponent elasticSpring) return;

                var direction = elasticSpring.PointB.Position - elasticSpring.PointA.Position;
                var currentLength = Vector2Utils.Magnitude(direction);
                if (currentLength > TOLERANCE)
                {
                    direction /= currentLength;
                    float forceMagnitude = elasticSpring.Stiffness * (currentLength - elasticSpring.RestLength);
                    Vector2f force = forceMagnitude * direction;

                    lock (elasticSpring.PointA)
                    {
                        AddForce(elasticSpring.PointA, force);
                    }

                    lock (elasticSpring.PointB)
                    {
                        AddForce(elasticSpring.PointB, -force);
                    }

                    Vector2f relativeVelocity = elasticSpring.PointB.Velocity - elasticSpring.PointA.Velocity;
                    float dampingMagnitude = elasticSpring.Damping * Vector2Utils.Dot(relativeVelocity, direction);
                    Vector2f dampingForce = dampingMagnitude * direction;

                    lock (elasticSpring.PointA)
                    {
                        AddForce(elasticSpring.PointA, dampingForce);
                    }

                    lock (elasticSpring.PointB)
                    {
                        AddForce(elasticSpring.PointB, -dampingForce);
                    }
                }
            });
    }

    private void ApplyGravity(PhysicsComponent component)
    {
        if (component.Points != null) Parallel.ForEach(component.Points, point => AddGravity(point, _gravity));
    }
}