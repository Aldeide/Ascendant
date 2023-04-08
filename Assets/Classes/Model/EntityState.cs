using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.Models
{
    public enum EntityMovementState
    {
        Idle,
        Running,
        Sprinting
    }

    public enum EntityStanceState
    {
        Upright,
        Crouched,
        Aiming
    }

    public enum EntityGroundedState
    {
        Grounded,
        Jumping,
        Falling,
        Climbing
    }

    public enum EntityFiringState
    {
        NotFiring,
        Firing
    }

    public enum EntityAliveState
    {
        Alive,
        Dead
    }
    public class EntityStateModel
    {
        public Vector3 position;
        public Vector3 forward;
        public Quaternion rotation;
        public Vector3 aimPoint;

        public EntityMovementState movementState;
        public EntityStanceState stanceState;
        public EntityGroundedState groundedState;
        public EntityFiringState firingState;
        public EntityAliveState aliveState { get; set; }

        public float timeOfDeath { get; set; }

        public EntityStateModel(Vector3 position, Vector3 forward, Quaternion rotation, Vector3 aimPoint)
        {
            this.position = position;
            this.forward = forward;
            this.rotation = rotation;
            this.aimPoint = aimPoint;

            movementState = EntityMovementState.Idle;
            stanceState = EntityStanceState.Upright;
            groundedState = EntityGroundedState.Grounded;
            firingState = EntityFiringState.Firing;
            aliveState = EntityAliveState.Alive;
        }

    }
}

