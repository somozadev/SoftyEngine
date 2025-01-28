using SFML.Graphics;
using SFML.System;

namespace Softine.Utils;

public static class Shapering
{
    public static CircleShape CreateCircle(float radius = 10.0f, Color? color = null, Vector2f? position = null)
    {
        color ??= new Color(100, 250, 50);
        position ??= new Vector2f(0, 0);

        CircleShape cs = new CircleShape(radius);
        cs.FillColor = color.Value;
        cs.Origin = new Vector2f(radius, radius);
        cs.Position = position.Value;
        return cs;
    }

    public static VertexArray CreateLine(Vector2f posA, Vector2f posB,Color? color = null)
    {
        color ??= new Color(250, 50, 50);

        VertexArray line;
        line = new VertexArray(PrimitiveType.Lines, 2);
        line[0] = new Vertex(new Vector2f(posA.X, posA.Y), (Color)color);
        line[1] = new Vertex(new Vector2f(posB.X, posB.Y), (Color)color);
        return line;
    }
}