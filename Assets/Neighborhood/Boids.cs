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
        [Range(1f, 90f)]
        public float gyroAngleLimit = 45f;
        
        private readonly Dictionary<int, Agent> agents = new();


        private void Start()
        {
            SpawnAgents();
            Input.gyro.enabled = true;
        }
        
        private void Update()
        {
            var rotRH = Input.gyro.attitude;
            var rot = new Quaternion(-rotRH.x, -rotRH.z, -rotRH.y, rotRH.w);
            var euler = rot.eulerAngles;
            euler.x = this.GetGyroAngleRate(euler.x);
            euler.z = this.GetGyroAngleRate(euler.z);

            var gyro = this.gyroPower * new Vector2(-euler.z, euler.x);
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
        
        private float GetGyroAngleRate(float angle)
        {
            if (angle < 180f)
            {
                return Mathf.Clamp01(angle / this.gyroAngleLimit);   
            }
            
            angle -= (360f - this.gyroAngleLimit);
            return -Mathf.Clamp01(1f - angle / this.gyroAngleLimit);
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