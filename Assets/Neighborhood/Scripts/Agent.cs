using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neighborhood
{
    public class Agent
    {
        private static Boids Boids => Boids.Instance;

        private Vector2 velocity = Vector2.zero;
        private readonly Collider2D[] overlap = Array.Empty<Collider2D>();
        private readonly Agent[] neighbours = Array.Empty<Agent>();
        private readonly Transform transform = null;


        public Agent(Transform transform)
        {
            this.transform = transform;
            this.velocity = new Vector2(transform.right.x, transform.right.y);
            this.overlap = new Collider2D[Boids.NumAgents];
            this.neighbours = new Agent[Boids.NumAgents];
        }

        public void Update(Vector2 gyro)
        {
            var pos = (Vector2)this.transform.position;

            var cnt = Physics2D.OverlapCircle(
                pos, Boids.AwarenessRadius, Boids.OverlapFilter, this.overlap
            );
            
            AvoidEdges(ref pos);

            Boids.GetNeighbours(this.overlap, cnt, this.neighbours);
            var alignment = Align(this.neighbours, cnt);
            var cohesion = Cohesion(this.neighbours, cnt, this.transform);
            var separation = Separation(this.neighbours, cnt, this.transform);

            var accel = new Vector2(0, 0);
            accel += alignment * Boids.Alignment;
            accel += cohesion * Boids.Cohesion;
            accel += separation * Boids.Separation;
            
            this.velocity += accel * Time.deltaTime;
            this.velocity = Vector2.ClampMagnitude(velocity + gyro, Boids.MaxMoveSpeed);
            pos += this.velocity * Time.deltaTime;
            
            this.transform.position = pos;
            transform.right = (this.velocity);
        }

        private static void AvoidEdges(ref Vector2 position)
        {
            if (position.x <= -Boids.BoundSize.x)
            {
                position.x = Boids.BoundSize.x;
            }
            else if (position.x >= Boids.BoundSize.x)
            {
                position.x = -Boids.BoundSize.x;
            }

            if (position.y >= Boids.BoundSize.y)
            {
                position.y = -Boids.BoundSize.y;
            }
            else if (position.y <= -Boids.BoundSize.y)
            {
                position.y = Boids.BoundSize.y;
            }
        }

        private static Vector2 Align(in Agent[] neighbours, int length)
        {
            if (neighbours.Length <= 0)
            {
                return Vector2.zero;
            }

            var avg = Vector2.zero;
            for (var i = 0; i < length; i++)
            {
                avg += neighbours[i].velocity;
            }

            avg /= neighbours.Length;
            return avg.normalized;
        }

        private static Vector2 Cohesion(in Agent[] neighbours, int length, in Transform transform)
        {
            if (neighbours.Length <= 0)
            {
                return Vector2.zero;
            }

            var avg = Vector2.zero;
            for (var i = 0; i < length; i++)
            {
                avg += (Vector2)neighbours[i].transform.position;
            }

            avg /= neighbours.Length;
            avg -= (Vector2)transform.position;
            return avg.normalized;
        }

        private static Vector2 Separation(in Agent[] neighbours, int length, in Transform transform)
        {
            if (neighbours.Length <= 0)
            {
                return Vector2.zero;
            }

            var avg = Vector2.zero;
            for (var i = 0; i < length; i++)
            {
                var d = Vector2.Distance(neighbours[i].transform.position, transform.position);
                if (d >= Boids.AwarenessRadius * Boids.AvoidanceRadius)
                {
                    continue;
                }

                Vector2 difference = transform.position - neighbours[i].transform.position;
                avg += difference;
            }

            avg /= neighbours.Length;
            return avg.normalized;
        }
    }
}
