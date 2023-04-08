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
    public class EntityStateModel : MonoBehaviour
    {
        public Vector3 position;
        public Vector3 direction;
        public Quaternion rotation;
        public Vector3 aimPoint;

        public EntityMovementState movementState { get; set; }
        public EntityStanceState stanceState { get; set; }
        public EntityGroundedState groundedState { get; set; }
        public EntityFiringState firingState { get; set; }
        public EntityAliveState aliveState { get; set; }
        public float timeOfDeath { get; set; }

        public EntityStateModel(Vector3 position, Vector3 direction, Quaternion rotation, Vector3 aimPoint)
        {
            this.position = position;
            this.direction = direction;
            this.rotation = rotation;
            this.aimPoint = aimPoint;

            movementState = EntityMovementState.Idle;
            stanceState = EntityStanceState.Upright;
            groundedState = EntityGroundedState.Grounded;
            firingState = EntityFiringState.Firing;
            aliveState = EntityAliveState.Alive;
        }

        public EntityStateModel()
        {
            this.position = Vector3.zero;
            this.direction = Vector3.zero;
            this.rotation= Quaternion.identity;
            this.aimPoint = Vector3.zero;

            movementState = EntityMovementState.Idle;
            stanceState = EntityStanceState.Upright;
            groundedState = EntityGroundedState.Grounded;
            firingState = EntityFiringState.Firing;
            aliveState = EntityAliveState.Alive;
        }

        public bool IsMoving()
        {

            return (movementState == EntityMovementState.Running || movementState == EntityMovementState.Sprinting);
        }

        public bool IsGrounded()
        {
            return groundedState == EntityGroundedState.Grounded;
        }

        public Networking.PlayerStateData ToNetworkedState()
        {
            return new Networking.PlayerStateData(
                GameManager.Instance.localPlayerId,
                position,
                direction,
                rotation,
                aimPoint,
                (ushort)movementState,
                (ushort)stanceState,
                (ushort)groundedState,
                (ushort)firingState,
                (ushort)aliveState
                );
        }

        public void SyncFromNetworkedState(Networking.PlayerStateData data)
        {
            position = data.position;
            direction = data.forward;
            rotation = data.rotation;
            aimPoint = data.aimPoint;
            movementState = (EntityMovementState)data.movementState;
            stanceState = (EntityStanceState)data.stanceState;
            groundedState = (EntityGroundedState)data.groundedState;
            firingState = (EntityFiringState)data.firingState;
            aliveState = (EntityAliveState)data.aliveState;
        }

    }
}

