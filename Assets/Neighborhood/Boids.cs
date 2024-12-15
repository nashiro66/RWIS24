using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
        [Range(1, NumBoidsMax)]
        public int NumAgents = 100;
        public GameObject AgentPrefab = null;
        public ContactFilter2D OverlapFilter = default;
        public float gyroPower = 1f;
        [Header("Debug")]
        public float gyroSpeed = 50f;
        
        private readonly Dictionary<int, Agent> agents = new();


        private void Start()
        {
            SpawnAgents();
            Input.gyro.enabled = true;
        }
        
        private void Update()
        {
            var pitch = Input.gyro.rotationRate.x * Mathf.Rad2Deg;
            var roll = Input.gyro.rotationRate.z * Mathf.Rad2Deg;
            var gyro = new Vector3(roll, pitch);
            
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            gyro.x += horizontal * gyroSpeed * Time.deltaTime;
            gyro.y += vertical * gyroSpeed * Time.deltaTime;

            gyro *= this.gyroPower;
            foreach (var agent in this.agents)
            {
                agent.Value.Update(gyro);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(
                Vector3.zero,
                2f * this.BoundSize
            );
        }

        public void GetNeighbours(in Collider2D[] overlap, int length, Agent[] neighbours)
        {
            for (var i = 0; i < length && i < overlap.Length; i++)
            {
                var id = overlap[i].gameObject.GetInstanceID();
                neighbours[i] = this.agents[id];
            }
        }

        private void SpawnAgents()
        {
            for (var i = 0; i < this.NumAgents; i++)
            {
                var obj = Instantiate(
                    this.AgentPrefab,
                    new Vector2(
                        Random.Range(-this.BoundSize.x, this.BoundSize.x),
                        Random.Range(-this.BoundSize.y, this.BoundSize.y)
                    ),
                    Quaternion.Euler(this.AgentPrefab.transform.forward * Random.Range(0, 360)),
                    this.transform
                );
                
                this.agents[obj.GetInstanceID()] = new Agent(obj.transform);
            }
        }
    }
}