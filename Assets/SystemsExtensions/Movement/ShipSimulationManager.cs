using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Jobs;

namespace Ascendant.SystemsExtensions.Movement
{
    public struct ShipInput : INetworkSerializeByMemcpy
    {
        public Vector3 TargetPosition;
        public bool HasTarget;
    }

    public struct ShipStats : INetworkSerializeByMemcpy
    {
        public float MoveSpeed;
        public float TurnSpeed;
    }

    [BurstCompile]
    public struct ShipMovementJob : IJobParallelForTransform
    {
        public NativeArray<ShipInput> Inputs;
        [ReadOnly] public NativeArray<ShipStats> Stats;
        public float DeltaTime;

        public void Execute(int index, TransformAccess transform)
        {
            var input = Inputs[index];
            if (!input.HasTarget) return;

            var stats = Stats[index];
            Vector3 currentPos = transform.position;
            Vector3 dir = input.TargetPosition - currentPos;
            float dist = dir.magnitude;

            // Arrival tolerance
            if (dist < 0.2f)
            {
                input.HasTarget = false;
                Inputs[index] = input;
                return;
            }

            Vector3 targetDir = dir / dist;

            // Calculate rotation towards the target direction
            Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, stats.TurnSpeed * DeltaTime);

            // Move forward
            Vector3 forward = transform.rotation * Vector3.forward;
            float step = stats.MoveSpeed * DeltaTime;
            if (step > dist)
            {
                step = dist;
            }

            transform.position = currentPos + forward * step;
        }
    }

    public class ShipSimulationManager : MonoBehaviour
    {
        public static ShipSimulationManager Instance { get; private set; }

        private List<ShipController> m_ActiveShips = new List<ShipController>();
        private NativeArray<ShipInput> m_Inputs;
        private NativeArray<ShipStats> m_Stats;
        private TransformAccessArray m_TransformAccessArray;
        private JobHandle m_JobHandle;
        private bool m_IsJobScheduled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            m_TransformAccessArray = new TransformAccessArray(0);
            Debug.Log("[ShipSimulationManager] Awake: Instance initialized successfully.");
        }

        private void OnDestroy()
        {
            EnsureJobCompleted();
            if (m_Inputs.IsCreated) m_Inputs.Dispose();
            if (m_Stats.IsCreated) m_Stats.Dispose();
            if (m_TransformAccessArray.isCreated) m_TransformAccessArray.Dispose();
        }

        public void RegisterShip(ShipController ship)
        {
            EnsureJobCompleted();
            if (!m_ActiveShips.Contains(ship))
            {
                m_ActiveShips.Add(ship);
                RebuildArrays();
                Debug.Log($"[ShipSimulationManager] Registered ship '{ship.gameObject.name}'. Total registered: {m_ActiveShips.Count}");
            }
        }

        public void UnregisterShip(ShipController ship)
        {
            EnsureJobCompleted();
            if (m_ActiveShips.Remove(ship))
            {
                RebuildArrays();
                Debug.Log($"[ShipSimulationManager] Unregistered ship '{ship.gameObject.name}'. Total registered: {m_ActiveShips.Count}");
            }
        }

        private void RebuildArrays()
        {
            if (m_Inputs.IsCreated) m_Inputs.Dispose();
            if (m_Stats.IsCreated) m_Stats.Dispose();
            if (m_TransformAccessArray.isCreated) m_TransformAccessArray.Dispose();

            int count = m_ActiveShips.Count;
            m_Inputs = new NativeArray<ShipInput>(count, Allocator.Persistent);
            m_Stats = new NativeArray<ShipStats>(count, Allocator.Persistent);

            Transform[] transforms = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                transforms[i] = m_ActiveShips[i].transform;
            }
            m_TransformAccessArray = new TransformAccessArray(transforms);
        }

        private void FixedUpdate()
        {
            EnsureJobCompleted();

            // Only run the authoritative simulation on the server
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

            int count = m_ActiveShips.Count;
            if (count == 0) return;

            // Gather inputs and stats from the controllers
            for (int i = 0; i < count; i++)
            {
                m_Inputs[i] = m_ActiveShips[i].GetCurrentInput();
                m_Stats[i] = m_ActiveShips[i].GetStats();

                if (m_Inputs[i].HasTarget)
                {
                    Debug.Log($"[ShipSimulationManager] Simulating movement for ship '{m_ActiveShips[i].gameObject.name}' to target {m_Inputs[i].TargetPosition}. Current Position: {m_ActiveShips[i].transform.position}");
                }
            }

            // Schedule the simulation job
            var job = new ShipMovementJob
            {
                Inputs = m_Inputs,
                Stats = m_Stats,
                DeltaTime = Time.fixedDeltaTime
            };

            m_JobHandle = job.Schedule(m_TransformAccessArray);
            m_IsJobScheduled = true;

            // Complete synchronously in FixedUpdate so NetworkTransform reads the updated transform position immediately
            EnsureJobCompleted();

            // Write back updated inputs (HasTarget flag) to controllers on the server
            if (m_Inputs.IsCreated && count == m_Inputs.Length)
            {
                for (int i = 0; i < count; i++)
                {
                    m_ActiveShips[i].SetInputFromServer(m_Inputs[i]);
                }
            }
        }

        private void EnsureJobCompleted()
        {
            if (m_IsJobScheduled)
            {
                m_JobHandle.Complete();
                m_IsJobScheduled = false;
            }
        }
    }
}
