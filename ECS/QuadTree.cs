using System.Drawing;
using System.Collections.Generic;

namespace SoftyEngine.ECS
{
    public class QuadTree
    {
        private readonly int MAX_OBJECTS = 10;
        private readonly int MAX_LEVELS = 5;
        private int level;
        private List<PhysicsComponent> objects;
        private Rectangle bounds;
        private QuadTree[] nodes;

        public QuadTree(int level, Rectangle bounds)
        {
            this.level = level;
            this.bounds = bounds;
            objects = new List<PhysicsComponent>();
            nodes = new QuadTree[4];
        }

        // Método para insertar un objeto con área circular (ColliderArea)
        public void Insert(PhysicsComponent component)
        {
            var point = component.Points[0];
            var collider = component.Points[0].ColliderArea;
            var circleBounds = new RectangleF(point.Position.X - collider, point.Position.Y - collider,
                collider * 2, collider * 2);

            int index = GetIndex(circleBounds);

            if (index != -1)
            {
                // Si el índice es válido, inserta en el cuadrante correspondiente
                nodes[index].Insert(component);
            }
            else
            {
                // Si no cabe en un cuadrante, lo agrega al nodo actual
                objects.Add(component);
            }
        }

        // Método para recuperar objetos del QuadTree
        public List<PhysicsComponent> Retrieve(List<PhysicsComponent> returnObjects, PhysicsComponent component)
        {
            var point = component.Points[0];
            var collider = component.Points[0].ColliderArea;
            var circleBounds = new RectangleF(point.Position.X - collider, point.Position.Y - collider,
                collider * 2, collider * 2);

            int index = GetIndex(circleBounds);
            if (index != -1 && nodes[0] != null)
            {
                nodes[index].Retrieve(returnObjects, component);
            }

            returnObjects.AddRange(objects);
            return returnObjects;
        }

        // Método para obtener el índice del cuadrante en el que cae el círculo
        public int GetIndex(RectangleF objectBounds)
        {
            int index = -1; // Si no se encuentra en ningún cuadrante, retornar -1

            float verticalMidpoint = bounds.Left + (bounds.Width / 2);
            float horizontalMidpoint = bounds.Top + (bounds.Height / 2);

            bool topQuadrant = (objectBounds.Top + objectBounds.Height / 2 < horizontalMidpoint);
            bool bottomQuadrant = (objectBounds.Top - objectBounds.Height / 2 > horizontalMidpoint);

            bool leftQuadrant = (objectBounds.Left + objectBounds.Width / 2 < verticalMidpoint);
            bool rightQuadrant = (objectBounds.Left - objectBounds.Width / 2 > verticalMidpoint);

            // Determina en qué cuadrante está el objeto
            if (topQuadrant)
            {
                if (leftQuadrant) // Arriba a la izquierda
                {
                    index = 0;
                }
                else if (rightQuadrant) // Arriba a la derecha
                {
                    index = 1;
                }
            }
            else if (bottomQuadrant)
            {
                if (leftQuadrant) // Abajo a la izquierda
                {
                    index = 2;
                }
                else if (rightQuadrant) // Abajo a la derecha
                {
                    index = 3;
                }
            }

            return index;
        }
    }
}