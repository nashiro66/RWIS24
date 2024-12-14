using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neighborhood
{
    public class Agent : MonoBehaviour
    {
        [SerializeField]
        private ContactFilter2D overlapFilter = default;
        private Vector2 velocity = Vector2.zero;
        private Collider2D[] overlap = Array.Empty<Collider2D>();
        private readonly List<Agent> neighbours = new();


        private void Awake()
        {
            velocity = new Vector2(transform.right.x, transform.right.y);
            this.overlap = new Collider2D[Boids.Instance.NumBoids];
        }

        private void Update()
        {
            this.neighbours.Clear();
            var pos = (Vector2)this.transform.position;

            var cnt = Physics2D.OverlapCircle(
                pos, Boids.Instance.AwarenessRadius, overlapFilter, this.overlap
            );
            for (var i = 0; i < cnt; i++)
            {
                this.neighbours.Add(this.overlap[i].GetComponent<Agent>());
            }

            AvoidEdges(ref pos);

            var alignment = Align();
            var cohesion = Cohesion();
            var separation = Separation();

            var accel = new Vector2(0, 0);
            accel += alignment * (Boids.Instance.Alignment * Boids.Instance.MaxMoveSpeed);
            accel += cohesion * (Boids.Instance.Cohesion * Boids.Instance.MaxMoveSpeed);
            accel += separation * (Boids.Instance.Separation * Boids.Instance.MaxMoveSpeed);

            //apply to vars
            pos += velocity * Time.deltaTime;
            velocity += accel * Time.deltaTime;
            velocity = Vector2.ClampMagnitude(velocity, Boids.Instance.MaxMoveSpeed);

            //apply to transform
            this.transform.position = pos;
            transform.right = velocity;
        }

        private static void AvoidEdges(ref Vector2 position)
        {
            if (position.x <= -Boids.Instance.BoundSize.x)
            {
                position.x = Boids.Instance.BoundSize.x;
            }
            else if (position.x >= Boids.Instance.BoundSize.x)
            {
                position.x = -Boids.Instance.BoundSize.x;
            }

            if (position.y >= Boids.Instance.BoundSize.y)
            {
                position.y = -Boids.Instance.BoundSize.y;
            }
            else if (position.y <= -Boids.Instance.BoundSize.y)
            {
                position.y = Boids.Instance.BoundSize.y;
            }
        }

        private Vector2 Align()
        {
            if (this.neighbours.Count <= 0)
            {
                return Vector2.zero;
            }

            var avg = Vector2.zero;
            foreach (var neighbour in this.neighbours)
            {
                avg += neighbour.velocity;
            }

            avg /= this.neighbours.Count;
            avg = avg.normalized * Boids.Instance.MaxMoveSpeed;
            avg -= velocity;
            avg = Vector2.ClampMagnitude(avg, Boids.Instance.MaxTurnEffect);

            return avg;
        }

        private Vector2 Cohesion()
        {
            if (this.neighbours.Count <= 0)
            {
                return Vector2.zero;
            }

            var avg = Vector2.zero;
            foreach (var neighbour in this.neighbours)
            {
                avg += (Vector2)neighbour.transform.position;
            }

            avg /= this.neighbours.Count;
            avg -= (Vector2)this.transform.position;
            avg = avg.normalized * Boids.Instance.MaxMoveSpeed;
            avg -= velocity;
            avg = Vector2.ClampMagnitude(avg, Boids.Instance.MaxTurnEffect);

            return avg;
        }

        private Vector2 Separation()
        {
            if (this.neighbours.Count <= 0)
            {
                return Vector2.zero;
            }

            var avg = Vector2.zero;
            foreach (var neighbour in this.neighbours)
            {
                var d = Vector2.Distance(neighbour.transform.position, this.transform.position);
                if (d >= Boids.Instance.AwarenessRadius * Boids.Instance.AvoidanceRadius)
                {
                    continue;
                }

                Vector2 difference = this.transform.position - neighbour.transform.position;
                difference /= d;
                avg += difference;
            }

            avg /= this.neighbours.Count;
            avg = avg.normalized * Boids.Instance.MaxMoveSpeed;
            avg -= velocity;
            avg = Vector2.ClampMagnitude(avg, Boids.Instance.MaxTurnEffect);

            return avg;
        }
    }
}