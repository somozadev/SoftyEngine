using System;
using SFML.Graphics;
using SFML.System;
using Softine;
using Softine.Utils;

namespace SoftBody
{
    [Serializable]
    public class PointMassComponent : Component
    {
        public float Mass{ get; set; }
        public Vector2f Position{ get; set; }
        public Vector2f Velocity{ get; set; }
        public Vector2f Acceleration { get; set; } 
        public bool ForcesApplyOnIt { get; set; } 
        // public CircleShape Visuals;

        public PointMassComponent(float mass, Vector2f position, bool forcesApplyOnIt = false)
        {
            Mass = mass;
            Position = position;
            ForcesApplyOnIt = forcesApplyOnIt;
            //
            // Visuals = new CircleShape(10.0f);
            // Visuals.FillColor = new Color(100, 250, 50);
            // Visuals.Origin = new Vector2f(10.0f, 10.0f);
            // Visuals.Position = new Vector2f(Position.X, Position.Y);
        }
    }
}