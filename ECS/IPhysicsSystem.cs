namespace SoftyEngine.ECS;

public interface IPhysicsSystem
{
    void FixedUpdate(float fixedDeltaTime);
    void Update(float fixedDeltaTime);
}