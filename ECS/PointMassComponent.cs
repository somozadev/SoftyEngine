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
        public float Mass { get; set; }
        public Vector2f Position { get; set; }
        public Vector2f Velocity { get; set; }
        public Vector2f Acceleration { get; set; }
        public bool ForcesApplyOnIt { get; set; }

        public float ColliderArea { get; set; }

        public float AngularVelocity { get; set; }
        public float Inertia { get; set; }


        public PointMassComponent(float mass, Vector2f position, bool forcesApplyOnIt = false, float colliderArea = 0)
        {
            Mass = mass;
            Position = position;
            ForcesApplyOnIt = forcesApplyOnIt;
            ColliderArea = colliderArea; //the circle itself
        }

        public void UpdateInertia(string newInteria, int n = 0)
        {
            switch (newInteria)
            {
                case "circle":
                    Inertia = (1f / 2f) * Mass * ColliderArea/2 * ColliderArea/2;
                    break;
                case "square":
                    Inertia = (1f / 12f) * Mass * ( ColliderArea * ColliderArea + ColliderArea * ColliderArea);
                    break;
                case "triangle":
                    Inertia = (1f / 18f) * Mass * (ColliderArea * ColliderArea +  ColliderArea * ColliderArea); //b at 2 +h at 2
                    break;
                case "polygon":
                    Inertia = (1f / 6f) * Mass * ColliderArea * ColliderArea * ( 1 / (1 - (1/( n * MathF.Cos((float)Math.PI/n)))));
                    break;
            }
        }
        
    }
}