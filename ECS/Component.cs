using SFML.Graphics;

namespace Softine;

public abstract class Component
{
    public Entity Entity { get; private set; }

    public virtual void SetOwner(Entity entity)
    {
        Entity = entity;
    }

    public virtual void Destroy()
    {
    }

    // public virtual void Update(float deltaTime) { } 
    // public virtual void FixedUpdate(float deltaTime) { } 
}