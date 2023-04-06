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
        SpawnLocalPlayerResponse = 6
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
        // z = 0, q = 1, s = 2, d = 3, space = 4, leftClick = 5. 
        public bool[] keyInputs;
        public Quaternion lookDirection;
        public uint time;

        public PlayerInputData(bool[] keyInputs, Quaternion lookDirection, uint time)
        {
            this.keyInputs = keyInputs;
            this.lookDirection = lookDirection;
            this.time = time;
        }

        public void Deserialize(DeserializeEvent e)
        {
            keyInputs = e.Reader.ReadBooleans();
            lookDirection = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            if (keyInputs[5])
            {
                time = e.Reader.ReadUInt32();
            }
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(keyInputs);
            e.Writer.Write(lookDirection.x);
            e.Writer.Write(lookDirection.y);
            e.Writer.Write(lookDirection.z);
            e.Writer.Write(lookDirection.w);
            if (keyInputs[5])
            {
                e.Writer.Write(time);
            }
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

}


