using SoftyEngine.ECS;

namespace Softine;

public class Scene(string name)
{
    public string Name = name;
    private readonly List<Entity> _entities = [];
    private readonly List<ISystem> _systems = [];
    private readonly List<IPhysicsSystem> _physicsSystems = [];

    public Entity Instantiate(Entity entity)
    {
        _entities.Add(entity);
        return entity;
    }

    public int GetEntitiesAmount()
    {
        return _entities.Count;
    }
    public Entity? Get(Guid entityGuid)
    {
        return _entities.Find(e => e.Guid.Equals(entityGuid));
    }

    public Entity? Get(string entityId)
    {
        return _entities.Find(e => e.Id.Equals(entityId));
    }

    public void Destroy(Entity entity)
    {
        _entities.Remove(entity);
    }

    public ISystem GetSystem(Type type)
    {
        return _systems.FirstOrDefault(system => system.GetType() == type);
    }
    
    public void AddSystem(ISystem system)
    {
        _systems.Add(system);
    }

    public void AddPhysicsSystem(IPhysicsSystem physicsSystem)
    {
        _physicsSystems.Add(physicsSystem);
    }
    public T GetPhysicsSystem<T>() where T : IPhysicsSystem
    {
        return _physicsSystems.OfType<T>().FirstOrDefault();
    }

    public void Update(float deltaTime)
    {
        foreach (var system in _systems)
            system.Update(deltaTime);

        foreach (var pSystem in _physicsSystems)
            pSystem.Update(deltaTime);
    }

    public void FixedUpdate(float fixedDeltaTime)
    {
        foreach (var physicsSystem in _physicsSystems)
        {
            physicsSystem.FixedUpdate(fixedDeltaTime);
        }
    }
}