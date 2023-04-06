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
        SpawnPlayer = 7,
        PlayerInputDataFromServer = 8,
        SyncPlayerStateRequest = 9,
        SyncPlayerStateResponse = 10,
        SyncOtherPlayer = 11,
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

        public PlayerStateData(ushort id, float gravity, Vector3 position, Quaternion lookDirection)
        {
            this.id = id;
            this.position = position;
            this.lookDirection = lookDirection;
            this.gravity = gravity;
        }

        public ushort id;
        public Vector3 position;
        public float gravity;
        public Quaternion lookDirection;

        public void Deserialize(DeserializeEvent e)
        {
            position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            lookDirection = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            id = e.Reader.ReadUInt16();
            gravity = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(position.x);
            e.Writer.Write(position.y);
            e.Writer.Write(position.z);

            e.Writer.Write(lookDirection.x);
            e.Writer.Write(lookDirection.y);
            e.Writer.Write(lookDirection.z);
            e.Writer.Write(lookDirection.w);
            e.Writer.Write(id);
            e.Writer.Write(gravity);
        }
    }

    public struct SpawnPlayerData : IDarkRiftSerializable
    {
        public ushort id;

        public SpawnPlayerData(ushort id)
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
}


