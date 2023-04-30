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

    public enum EntityWeaponTypeState
    {
        Unarmed = 0,
        Handgun = 1,
        Rifle = 2,
    }

    public class EntityStateModel : NetworkBehaviour
    {
        [field: SyncVar]
        public Vector3 position { get; [ServerRpc] set; }
        [field: SyncVar]
        public Vector3 direction { get; [ServerRpc] set; }
        [field: SyncVar]
        public Quaternion rotation { get; [ServerRpc] set; }
        [field: SyncVar]
        public Transform aimPoint { get; [ServerRpc] set; }

        [field: SyncVar]
        public EntityMovementState movementState { get; [ServerRpc] set; }
        [field: SyncVar]
        public EntityStanceState stanceState { get; [ServerRpc] set; }
        [field: SyncVar]
        public EntityGroundedState groundedState { get; [ServerRpc] set; }
        [field: SyncVar]
        public EntityFiringState firingState { get; [ServerRpc] set; }
        [field: SyncVar]
        public EntityAliveState aliveState { get; [ServerRpc] set; }
        [field: SyncVar]
        public EntityWeaponTypeState weaponTypeState { get; [ServerRpc] set; }
        [field: SyncVar]
        public float timeOfDeath { get; [ServerRpc] set; }

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

