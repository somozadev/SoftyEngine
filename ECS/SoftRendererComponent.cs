using SFML.System;
using SoftBody;
using SoftyEngine.ECS;

namespace Softine;

public class SoftRendererComponent : Component
{
    public List<RendererComponent> RendererComponents { get; set; }

    public SoftRendererComponent()
    {
        SoftRenderSystem.Register(this);
        RendererComponents = new List<RendererComponent>();
    }

    public SoftRendererComponent(IEnumerable<RendererComponent> rendererComponents)
    {
        SoftRenderSystem.Register(this);
        RendererComponents = rendererComponents.ToList();
    }

    //
    // private void InitializeDefaultCircle()
    // {
    //     var numPoints = 16;
    //     var radius = Entity.GetComponent<Transform>().Scale.X;
    //     var center = Entity.GetComponent<Transform>().Position;
    //
    //     for (int i = 0; i < numPoints; i++)
    //     {
    //         float angle = i * (360f / numPoints);
    //         float angleRad = angle * (float)Math.PI / 180f;
    //
    //         Vector2f position = new Vector2f(
    //             center.X + radius * (float)Math.Cos(angleRad),
    //             center.Y + radius * (float)Math.Sin(angleRad)
    //         );
    //
    //         Points.Add(new PointMassComponent(1f, position, true));
    //     }
    //
    //     for (int i = 0; i < numPoints; i++)
    //         Springs.Add(new SpringComponent(Points[i], Points[(i + 1) % numPoints], 10f, 1f));
    // }
    //
    // private void InitializeDefaultSquare()
    // {
    //     Entity.TryGetComponent<Transform>(out var trf);
    //     float size = trf.Scale.X;
    //     float halfSize = size / 2;
    //     var center = trf.Position;
    //
    //     Points.Add(new PointMassComponent(1f, new Vector2f(center.X - halfSize, center.Y - halfSize), true));
    //     Points.Add(new PointMassComponent(1f, new Vector2f(center.X + halfSize, center.Y - halfSize), true));
    //     Points.Add(new PointMassComponent(1f, new Vector2f(center.X - halfSize, center.Y + halfSize), true));
    //     Points.Add(new PointMassComponent(1f, new Vector2f(center.X + halfSize, center.Y + halfSize), true));
    //
    //     Springs.Add(new SpringComponent(Points[0], Points[1], 10f, 1f));
    //     Springs.Add(new SpringComponent(Points[1], Points[3], 10f, 1f));
    //     Springs.Add(new SpringComponent(Points[3], Points[2], 10f, 1f));
    //     Springs.Add(new SpringComponent(Points[2], Points[0], 10f, 1f));
    //     Springs.Add(new SpringComponent(Points[0], Points[3], 10f, 1f));
    //     Springs.Add(new SpringComponent(Points[1], Points[2], 10f, 1f));
    // }
    //
    // private void InitializeDefaultPolygon()
    // {
    //     var numSides = 5;
    //     var radius = Entity.GetComponent<Transform>().Scale.X;
    //     var center = Entity.GetComponent<Transform>().Position;
    //
    //
    //     for (int i = 0; i < numSides; i++)
    //     {
    //         float angle = i * (360f / numSides);
    //         float angleRad = angle * (float)Math.PI / 180f;
    //
    //         Vector2f position = new Vector2f(
    //             center.X + radius * (float)Math.Cos(angleRad),
    //             center.Y + radius * (float)Math.Sin(angleRad)
    //         );
    //
    //         Points.Add(new PointMassComponent(1f, position, true));
    //     }
    //
    //     for (int i = 0; i < numSides; i++)
    //         Springs.Add(new SpringComponent(Points[i], Points[(i + 1) % numSides], 10f, 1f));
    // }
}