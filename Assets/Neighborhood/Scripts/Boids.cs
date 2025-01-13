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
        public const int FPS = 60;
        public const float DeltaTime = 1f / FPS;

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
        public int seed = 1357902468;
        public Color leftColor = Color.red;
        public Color rightColor = Color.green;
        public NetworkTestScript network;

        private bool isReady = false;
        private float timer = 0f;
        private readonly Dictionary<int, Agent> agents = new();


        private void Start()
        {
            Random.InitState(this.seed);
            
            SpawnAgents();
            Input.gyro.enabled = true;
        }
        
        private void Update()
        {
            this.timer += Time.deltaTime;
            if (this.timer < DeltaTime)
            {
                return;
            }
            this.timer = 0f;

            if (isReady != network.isReady)
            {
                isReady = network.isReady;
                
                if (network.isReady)
                {
                    this.SpawnAgents();
                }
            }
            
            var rotRH = Input.gyro.attitude;
            var rot = new Quaternion(-rotRH.x, -rotRH.z, -rotRH.y, rotRH.w);
            var euler = rot.eulerAngles;
            euler.x = this.GetGyroAngleRate(euler.x);
            euler.z = this.GetGyroAngleRate(euler.z);

            var gyro = this.gyroPower * new Vector2(-euler.z, euler.x);
            foreach (var agent in this.agents.Values)
            {
                agent.Update(gyro);
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
            foreach (var agent in this.agents.Values)
            {
                Destroy(agent.Transform.gameObject);
            }
            this.agents.Clear();
            
            for (var i = 0; i < this.NumAgents; i++)
            {
                var pos = new Vector2(
                    Random.Range(-this.BoundSize.x, this.BoundSize.x),
                    Random.Range(-this.BoundSize.y, this.BoundSize.y)
                );
                var rot = Quaternion.Euler(this.AgentPrefab.transform.forward * Random.Range(0, 360));
                
                var obj = Instantiate(
                    this.AgentPrefab, pos, rot, this.transform
                );
                var agent = new Agent(this, obj.transform);
                agent.Renderer.color = pos.x < 0f ? this.leftColor : this.rightColor;
                
                this.agents[obj.GetInstanceID()] = agent;
            }
        }
    }
}