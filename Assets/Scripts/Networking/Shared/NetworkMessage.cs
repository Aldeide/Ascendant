using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public enum Tags
{
    JoinGameRequest,
    JoinGameResponse,
    SpawnLocalPlayerRequest,
    SpawnLocalPlayerResponse
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

