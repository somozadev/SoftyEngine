using Softine.Utils;

namespace Softine
{
    public class Physics
    {
        private List<PhysicsBody> physicsBodies = new List<PhysicsBody>();

        public void AddBody(PhysicsBody body)
        {
            physicsBodies.Add(body);
        }

        public void Tick(float deltaTime)
        {
            foreach (var body in physicsBodies)
            {
                body.UpdatePhysics(deltaTime);
            }

            ResolveCollisions();
        }

        private void ResolveCollisions()
        {
        }
    }
}