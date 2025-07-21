using SoftBody;
using Softine;

namespace SoftyEngine.ECS;

public class PhysicsComponent : Component, IPhysicsComponent
{
    public List<PointMassComponent>? Points { get; protected init; }
    protected int NumSize { get; init; }
    public override void Destroy()
    {
        foreach (var point in Points.ToList())
        {
            Points.Remove(point);
            point.Destroy();
            
        }
    }
}