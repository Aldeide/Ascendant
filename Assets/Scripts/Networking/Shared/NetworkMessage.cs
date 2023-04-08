using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

namespace Ascendant.Networking
{
    public enum Tags
    {
        LoginRequest = 0,
        LoginRequestAccepted = 1,
        LoginRequestRejected = 2,
        JoinGameRequest = 3,
        JoinGameResponse = 4,
        SpawnLocalPlayerRequest = 5,
        SpawnLocalPlayerResponse = 6,
        SpawnPlayerNotification = 7,
        PlayerInputDataFromServer = 8,
        SyncPlayerStateRequest = 9,
        SyncPlayerStateResponse = 10,
        SyncOtherPlayer = 11,
        PlayerDisconnectedNotification = 12,
        PlayerDamagedNotification = 13,
        SyncPlayerStatsRequest = 14,
        SyncPlayerStatsNotification = 15
    }

    public struct LoginRequestData : IDarkRiftSerializable
    {
        public string name;
        public LoginRequestData(string name)
        {
            this.name = name;
        }

        public void Deserialize(DeserializeEvent e)
        {
            name = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(name);
        }
    }

    public struct LoginInfoData : IDarkRiftSerializable
    {
        public ushort clientId;

        public LoginInfoData(ushort clientId)
        {
            this.clientId = clientId;
        }

        public void Deserialize(DeserializeEvent e)
        {
            clientId = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(clientId);
        }
    }

    public struct JoinGameResponseData : IDarkRiftSerializable
    {
        public bool JoinGameRequestAccepted;

        public JoinGameResponseData(bool accepted)
        {
            JoinGameRequestAccepted = accepted;
        }

        public void Deserialize(DeserializeEvent e)
        {
            JoinGameRequestAccepted = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(JoinGameRequestAccepted);
        }
    }

    public struct SpawnLocalPlayerResponseData : IDarkRiftSerializable
    {
        public ushort ID;

        public SpawnLocalPlayerResponseData(ushort id)
        {
            ID = id;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
        }
    }

    public struct PlayerInputData : IDarkRiftSerializable
    {
        Vector2 movementInput;
        public Quaternion lookDirection;
        public float sprintInput;
        public float crouchInput;
        public float aimInput;
        public float fireInput;
        public float jumpInput;
        public uint time;

        public PlayerInputData(Vector2 movementInput, Quaternion lookDirection, float sprintInput, float crouchInput, float aimInput, float fireInput, float jumpInput, uint time)
        {
            this.movementInput = movementInput;
            this.lookDirection = lookDirection;
            this.sprintInput = sprintInput;
            this.crouchInput = crouchInput;
            this.aimInput = aimInput;
            this.fireInput = fireInput;
            this.jumpInput = jumpInput;
            this.time = time;
        }

        public void Deserialize(DeserializeEvent e)
        {
            movementInput = new Vector2(e.Reader.ReadSingle(), e.Reader.ReadSingle());
            lookDirection = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            sprintInput = e.Reader.ReadSingle();
            crouchInput = e.Reader.ReadSingle();
            aimInput = e.Reader.ReadSingle();
            fireInput = e.Reader.ReadSingle();
            jumpInput = e.Reader.ReadSingle();
            time = e.Reader.ReadUInt32();

        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(movementInput.x);
            e.Writer.Write(movementInput.y);
            e.Writer.Write(lookDirection.x);
            e.Writer.Write(lookDirection.y);
            e.Writer.Write(lookDirection.z);
            e.Writer.Write(lookDirection.w);
            e.Writer.Write(sprintInput);
            e.Writer.Write(crouchInput);
            e.Writer.Write(aimInput);
            e.Writer.Write(fireInput);
            e.Writer.Write(jumpInput);
            e.Writer.Write(time);

        }
    }

    public struct PlayerStateData : IDarkRiftSerializable
    {

        public PlayerStateData(
            ushort id,
            Vector3 position,
            Vector3 forward,
            Quaternion rotation,
            Vector3 aimPoint,
            ushort movementState,
            ushort stanceState,
            ushort groundedState,
            ushort firingState,
            ushort aliveState)
        {
            this.id = id;
            this.position = position;
            this.forward = forward;
            this.rotation = rotation;
            this.aimPoint = aimPoint;
            this.movementState = movementState;
            this.stanceState = stanceState;
            this.groundedState = groundedState;
            this.firingState = firingState;
            this.aliveState = aliveState;
        }

        public ushort id;
        public Vector3 position;
        public Vector3 forward;
        public Quaternion rotation;
        public Vector3 aimPoint;
        public ushort movementState;
        public ushort stanceState;
        public ushort groundedState;
        public ushort firingState;
        public ushort aliveState;

        public void Deserialize(DeserializeEvent e)
        {
            id = e.Reader.ReadUInt16();
            position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            forward = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            rotation = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            aimPoint = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            movementState = e.Reader.ReadUInt16();
            stanceState = e.Reader.ReadUInt16();
            groundedState = e.Reader.ReadUInt16();
            firingState = e.Reader.ReadUInt16();
            aliveState = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(id);
            e.Writer.Write(position.x);
            e.Writer.Write(position.y);
            e.Writer.Write(position.z);
            e.Writer.Write(forward.x);
            e.Writer.Write(forward.y);
            e.Writer.Write(forward.z);
            e.Writer.Write(rotation.x);
            e.Writer.Write(rotation.y);
            e.Writer.Write(rotation.z);
            e.Writer.Write(rotation.w);
            e.Writer.Write(aimPoint.x);
            e.Writer.Write(aimPoint.y);
            e.Writer.Write(aimPoint.z);
            e.Writer.Write(movementState);
            e.Writer.Write(stanceState);
            e.Writer.Write(groundedState);
            e.Writer.Write(firingState);
            e.Writer.Write(aliveState);
        }
    }

    public struct PlayerClientId : IDarkRiftSerializable
    {
        public ushort id;

        public PlayerClientId(ushort id)
        {
            this.id = id;
        }

        public void Deserialize(DeserializeEvent e)
        {
            id = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(id);
        }

    }

    public struct PlayerDamagedData : IDarkRiftSerializable
    {
        public ushort id;
        public float damage;

        public PlayerDamagedData(ushort id, float damage)
        {
            this.id = id;
            this.damage = damage;
        }

        public void Deserialize(DeserializeEvent e)
        {
            id = e.Reader.ReadUInt16();
            damage = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(id);
            e.Writer.Write(damage);
        }
    }

    public struct PlayerStatsData : IDarkRiftSerializable
    {
        public ushort id;
        public float currentHealth;
        public float maxHealth;
        public float currentShield;
        public float maxShield;

        public PlayerStatsData(ushort id, float currentHealth, float maxHealth, float currentShield, float maxShield)
        {
            this.id = id;
            this.currentHealth = currentHealth;
            this.maxHealth = maxHealth;
            this.currentShield = currentShield;
            this.maxShield = maxShield;
        }

        public void Deserialize(DeserializeEvent e)
        {
            id = e.Reader.ReadUInt16();
            currentHealth = e.Reader.ReadSingle();
            maxHealth = e.Reader.ReadSingle();
            currentShield = e.Reader.ReadSingle();
            maxShield = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(id);
            e.Writer.Write(currentHealth);
            e.Writer.Write(maxHealth);
            e.Writer.Write(currentShield);
            e.Writer.Write(maxShield);
        }
    }
}


