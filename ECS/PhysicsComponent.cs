using SoftBody;
using Softine;

namespace SoftyEngine.ECS;

public class PhysicsComponent : Component, IPhysicsComponent
{
    public List<PointMassComponent>? Points { get; protected init; }
    protected int NumSize { get; init; }
}