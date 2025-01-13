using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neighborhood
{
    public class Agent
    {
        public readonly Transform Transform = null;
        public readonly SpriteRenderer Renderer = null;
        
        private Vector2 velocity = Vector2.zero;
        private readonly Collider2D[] overlap = Array.Empty<Collider2D>();
        private readonly Agent[] neighbours = Array.Empty<Agent>();
        private readonly Boids boids = null;


        public Agent(Boids boids, Transform transform)
        {
            this.boids = boids;
            this.Transform = transform;
            this.Renderer = transform.GetComponent<SpriteRenderer>();
            
            this.velocity = new Vector2(transform.right.x, transform.right.y);
            this.overlap = new Collider2D[this.boids.NumAgents];
            this.neighbours = new Agent[this.boids.NumAgents];
        }

        public void Update(Vector2 gyro)
        {
            var pos = (Vector2)this.Transform.position;

            var cnt = Physics2D.OverlapCircle(
                pos, this.boids.AwarenessRadius, this.boids.OverlapFilter, this.overlap
            );
            
            AvoidEdges(ref pos, this.boids);

            this.boids.GetNeighbours(this.overlap, cnt, this.neighbours);
            var alignment = Align(this.neighbours, cnt);
            var cohesion = Cohesion(this.neighbours, cnt, this.Transform);
            var separation = Separation(this.neighbours, cnt, this.boids, this.Transform);

            var accel = new Vector2(0, 0);
            accel += alignment * this.boids.Alignment;
            accel += cohesion * this.boids.Cohesion;
            accel += separation * this.boids.Separation;
            
            this.velocity += accel * Boids.DeltaTime;
            this.velocity = Vector2.ClampMagnitude(velocity + gyro, this.boids.MaxMoveSpeed);
            pos += this.velocity * Boids.DeltaTime;
            
            this.Transform.position = pos;
            Transform.right = (this.velocity);
        }

        public void SetPosition(in Vector2 pos, in Quaternion rot)
        {
            this.Transform.position = pos;
            this.Transform.rotation = rot;
            
            this.velocity = new Vector2(this.Transform.right.x, this.Transform.right.y);
        }

        private static void AvoidEdges(ref Vector2 position, in Boids boids)
        {
            if (position.x <= -boids.BoundSize.x)
            {
                position.x = boids.BoundSize.x;
            }
            else if (position.x >= boids.BoundSize.x)
            {
                position.x = -boids.BoundSize.x;
            }

            if (position.y >= boids.BoundSize.y)
            {
                position.y = -boids.BoundSize.y;
            }
            else if (position.y <= -boids.BoundSize.y)
            {
                position.y = boids.BoundSize.y;
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
                avg += (Vector2)neighbours[i].Transform.position;
            }

            avg /= neighbours.Length;
            avg -= (Vector2)transform.position;
            return avg.normalized;
        }

        private static Vector2 Separation(in Agent[] neighbours, int length, in Boids boids, in Transform transform)
        {
            if (neighbours.Length <= 0)
            {
                return Vector2.zero;
            }

            var avg = Vector2.zero;
            for (var i = 0; i < length; i++)
            {
                var d = Vector2.Distance(neighbours[i].Transform.position, transform.position);
                if (d >= boids.AwarenessRadius * boids.AvoidanceRadius)
                {
                    continue;
                }

                Vector2 difference = transform.position - neighbours[i].Transform.position;
                avg += difference;
            }

            avg /= neighbours.Length;
            return avg.normalized;
        }
    }
}
