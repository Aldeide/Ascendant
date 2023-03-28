using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public enum Tags
{
    JoinGameRequest,
    JoinGameResponse,
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

