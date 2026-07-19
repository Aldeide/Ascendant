using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Jobs;
using Ascendant.SystemsExtensions.Celestial;

namespace Ascendant.SystemsExtensions.Movement
{
    public enum MovementPhase : int
    {
        Idle = 0,
        Orient = 1,
        Burn = 2,
        ReverseBurn = 3
    }

    public struct ShipInput : INetworkSerializeByMemcpy
    {
        public GridCoordinate TargetCoordinate;
        public bool HasTarget;
        public Vector3 Velocity;
        public MovementPhase Phase;
        public Vector3 StartPosition;
        public float HalfWayDistance;
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
            GridCoordinate currentCoord = new GridCoordinate(currentPos);
            Vector3 diff = GridCoordinate.GetDifference(input.TargetCoordinate, currentCoord);
            float dist = diff.magnitude;

            // Calculate physics acceleration rate
            float accel = stats.MoveSpeed / 3.0f; // Reaches top speed in 3 seconds
            if (accel < 0.1f) accel = 100f; // Fallback

            float speed = input.Velocity.magnitude;

            // Arrival snap condition: if extremely close or within 1 frame of movement
            float snapThreshold = Mathf.Max(15f, speed * DeltaTime * 1.5f);
            if (dist < snapThreshold)
            {
                input.HasTarget = false;
                input.Velocity = Vector3.zero;
                input.Phase = MovementPhase.Idle;
                Inputs[index] = input;
                transform.position = input.TargetCoordinate.ToWorldPosition(); // Snap to target!
                return;
            }

            // Auto-initialize movement parameters if starting from default/Idle state
            if (input.Phase == MovementPhase.Idle)
            {
                input.Phase = MovementPhase.Orient;
                input.StartPosition = currentPos;
                input.HalfWayDistance = dist * 0.5f;
            }

            Vector3 targetDir = diff / dist;
            Vector3 desiredHeading = targetDir;

            // Execute Orient Phase
            if (input.Phase == MovementPhase.Orient)
            {
                desiredHeading = targetDir;

                // Rotate towards the target direction
                Quaternion targetRot;
                if (Mathf.Abs(Vector3.Dot(desiredHeading, Vector3.up)) > 0.99f)
                {
                    targetRot = Quaternion.LookRotation(desiredHeading, Vector3.forward);
                }
                else
                {
                    targetRot = Quaternion.LookRotation(desiredHeading, Vector3.up);
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, stats.TurnSpeed * DeltaTime);

                // No thrust during orient phase
                input.Velocity = Vector3.zero;

                // Transition to Burn phase when nearly aligned (dot product > 0.99f or < ~8 degrees angle)
                Vector3 forward = transform.rotation * Vector3.forward;
                float alignment = Vector3.Dot(forward, desiredHeading);
                if (alignment > 0.99f)
                {
                    input.Phase = MovementPhase.Burn;
                }
            }

            // Execute Burn Phase
            if (input.Phase == MovementPhase.Burn)
            {
                desiredHeading = targetDir;

                // Rotate to match target direction if drifting
                Quaternion targetRot;
                if (Mathf.Abs(Vector3.Dot(desiredHeading, Vector3.up)) > 0.99f)
                {
                    targetRot = Quaternion.LookRotation(desiredHeading, Vector3.forward);
                }
                else
                {
                    targetRot = Quaternion.LookRotation(desiredHeading, Vector3.up);
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, stats.TurnSpeed * DeltaTime);

                // Apply forward thrust if aligned within 30 degrees
                Vector3 forward = transform.rotation * Vector3.forward;
                float alignment = Vector3.Dot(forward, desiredHeading);
                if (alignment > 0.866f)
                {
                    input.Velocity += forward * accel * DeltaTime;
                }

                // Transition to Reverse Burn phase once we pass the halfway distance threshold
                if (dist <= input.HalfWayDistance)
                {
                    input.Phase = MovementPhase.ReverseBurn;
                }
            }

            // Execute Reverse Burn Phase
            if (input.Phase == MovementPhase.ReverseBurn)
            {
                desiredHeading = -targetDir; // Turn engine exhausts towards target

                // Rotate to face 180 degrees away
                Quaternion targetRot;
                if (Mathf.Abs(Vector3.Dot(desiredHeading, Vector3.up)) > 0.99f)
                {
                    targetRot = Quaternion.LookRotation(desiredHeading, Vector3.forward);
                }
                else
                {
                    targetRot = Quaternion.LookRotation(desiredHeading, Vector3.up);
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, stats.TurnSpeed * DeltaTime);

                // Apply deceleration thrust once we are reasonably aligned to the reverse heading
                Vector3 forward = transform.rotation * Vector3.forward;
                float alignment = Vector3.Dot(forward, desiredHeading);
                if (alignment > 0.95f)
                {
                    input.Velocity += forward * accel * DeltaTime;
                }

                // If velocity reverses direction relative to the target (the ship has fully stopped/braked), complete movement
                if (Vector3.Dot(input.Velocity, targetDir) < 0f)
                {
                    input.HasTarget = false;
                    input.Velocity = Vector3.zero;
                    input.Phase = MovementPhase.Idle;
                    Inputs[index] = input;
                    transform.position = input.TargetCoordinate.ToWorldPosition(); // Snap to target!
                    return;
                }
            }

            // Clamp velocity to top speed
            if (input.Velocity.magnitude > stats.MoveSpeed)
            {
                input.Velocity = input.Velocity.normalized * stats.MoveSpeed;
            }

            // Update position
            transform.position = currentPos + input.Velocity * DeltaTime;

            // Save input state
            Inputs[index] = input;
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
                    Debug.Log($"[ShipSimulationManager] Simulating movement for ship '{m_ActiveShips[i].gameObject.name}' to target {m_Inputs[i].TargetCoordinate.ToWorldPosition()}. Current Position: {m_ActiveShips[i].transform.position}");
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
