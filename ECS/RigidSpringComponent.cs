using SoftBody;

namespace SoftyEngine.ECS;

public class RigidSpringComponent(PointMassComponent pointA, PointMassComponent pointB)
    : SpringBaseComponent(pointA, pointB);