using SoftBody;

namespace SoftyEngine.ECS
{
    [Serializable]
    public class SpringComponent : SpringBaseComponent
    {
        public float Stiffness { get; }
        public float Damping { get; }

        public SpringComponent(PointMassComponent pointA, PointMassComponent pointB, float stiffness, float damping)
            :
            base(pointA, pointB)
        {
            Stiffness = stiffness;
            Damping = damping;
        }
    }
}