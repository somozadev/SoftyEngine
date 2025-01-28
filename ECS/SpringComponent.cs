using Softine;
using Softine.Utils;

namespace SoftBody
{
    [Serializable]
    public class SpringComponent : Component
    {
        public PointMassComponent PointA { get; }
        public PointMassComponent PointB { get; }
        public float RestLength { get; }
        public float Stiffness { get; }
        public float Damping { get; } //reduce excessive oscillation 
        // public VertexArray  Visuals;

        public SpringComponent(PointMassComponent pointA, PointMassComponent pointB, float stiffness, float damping)
        {
            PointA = pointA;
            PointB = pointB;
            RestLength = Vector2Utils.Distance(PointA.Position, PointB.Position);
            Stiffness = stiffness;
            Damping = damping;
             }
    }
}