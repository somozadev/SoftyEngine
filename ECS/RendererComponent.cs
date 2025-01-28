using SFML.Graphics;
using SoftyEngine.ECS;

namespace Softine;

public class RendererComponent : Component
{
    public bool IsShape => Drawable != null;
    public Shape Drawable { get; private set; }
    public VertexArray VDrawable { get;  set; }

    public RendererComponent(Shape shape)
    {
        RenderSystem.Register(this);
        Drawable = shape;
    }

    public RendererComponent(VertexArray shape)
    {
        RenderSystem.Register(this);
        VDrawable = shape;
    }
}