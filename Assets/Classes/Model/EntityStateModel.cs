using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;

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
    public class EntityStateModel : NetworkBehaviour
    {
        public Vector3 position { get; set; }
        public Vector3 direction { get; set; }
        public Quaternion rotation { get; set; }
        [field: SyncVar]
        public Transform aimPoint { get; [ServerRpc] set; }

        public EntityMovementState movementState { get; set; }
        public EntityStanceState stanceState { get; set; }
        public EntityGroundedState groundedState { get; set; }
        public EntityFiringState firingState { get; set; }
        public EntityAliveState aliveState { get; set; }
        public float timeOfDeath { get; set; }

        public bool IsMoving()
        {

            return (movementState == EntityMovementState.Running || movementState == EntityMovementState.Sprinting);
        }

        public bool IsGrounded()
        {
            return groundedState == EntityGroundedState.Grounded;
        }

    }

    public class SyncEntityStateModel : SyncBase, ICustomSync
    {
        public object GetSerializedType() => typeof(EntityStateModel);
    }

}

