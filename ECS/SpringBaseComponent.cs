using SoftBody;
using Softine;
using Softine.Utils;

namespace SoftyEngine.ECS;

[Serializable]
public abstract class SpringBaseComponent : Component
{
    public PointMassComponent PointA { get; }
    public PointMassComponent PointB { get; }
    public float RestLength { get; }

    protected SpringBaseComponent(PointMassComponent pointA, PointMassComponent pointB)
    {
        PointA = pointA;
        PointB = pointB;
        RestLength = Vector2Utils.Distance(PointA.Position, PointB.Position);
    }
}