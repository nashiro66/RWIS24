using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neighborhood
{
    public class Boids : MonoBehaviour
    {
        public const int NumBoidsMax = 100;

        public static Boids Instance
        {
            get
            {
                if (!InstanceInternal)
                {
                    InstanceInternal = FindObjectsByType<Boids>(FindObjectsSortMode.None).FirstOrDefault();
                }

                return InstanceInternal;
            }
        }
        private static Boids InstanceInternal = null;

        public Vector2 BoundSize = Vector2.one;
        [Range(0f, 1f)]
        public float Alignment = 1f;
        [Range(0f, 1f)]
        public float Separation = 1f;
        [Range(0f, 1f)]
        public float Cohesion = 1f;
        public float AwarenessRadius = 5f;
        [Range(0f, 1f)]
        public float AvoidanceRadius = 0.5f;
        public float MaxMoveSpeed = 2f;
        public float MaxTurnEffect = 1f;
        [Range(1, NumBoidsMax)]
        public int NumBoids = 100;
        public GameObject AgentPrefab = null;

        private List<Agent> agents = new();


        private void Start()
        {
            SpawnBois();
        }

        private void SpawnBois()
        {
            for (var i = 0; i < this.NumBoids; i++)
            {
                Instantiate(
                    this.AgentPrefab,
                    new Vector2(
                        Random.Range(-this.BoundSize.x, this.BoundSize.x),
                        Random.Range(-this.BoundSize.y, this.BoundSize.y)
                    ),
                    Quaternion.Euler(this.AgentPrefab.transform.forward * Random.Range(0, 360)),
                    this.transform
                );
            }
        }
    }
}