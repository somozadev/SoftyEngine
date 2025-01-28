
namespace Softine;

public class Entity
{
    public string Id { get; private set; }
    public Guid Guid { get; private set; }

    private List<Component> Components = new List<Component>();

    public Entity(string name)
    {
        Guid = Guid.NewGuid();
        Id = name == "" ? Guid.ToString() : name;
    }

    public void AddComponent(Component component)
    {
        component.SetOwner(this);
        Components.Add(component);
    }

    public T GetComponent<T>() where T : Component
    {
        return Components.Find(c => c is T) as T;
    }
    public bool TryGetComponent<T>(out T component) where T : Component
    {
        component = Components.Find(c => c is T) as T;
        return component != null;
    }
}