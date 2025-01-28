using SoftBody;
using Softine;


namespace SoftyEngine.ECS;

public class SoftEntity : Entity
{
    public SoftEntity(string name) : base(name)
    {
        AddComponent(new Transform());
        AddComponent(new SoftBodyComponent(SoftBodyType.SQUARE));
        // AddComponent(new RendererComponent());
    }
}