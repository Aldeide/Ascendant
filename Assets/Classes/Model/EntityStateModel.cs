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
        [field: SyncVar]
        public Vector3 position { get; [ServerRpc] set; }
        [field: SyncVar]
        public Vector3 direction { get; [ServerRpc] set; }
        [field: SyncVar]
        public Quaternion rotation { get; [ServerRpc] set; }
        [field: SyncVar]
        public Vector3 aimPoint { get; [ServerRpc] set; }

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
        public float timeOfDeath { get; [ServerRpc] set; }

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

    public class SyncEntityStateModel : SyncBase, ICustomSync
    {
        public object GetSerializedType() => typeof(EntityStateModel);
    }

}

