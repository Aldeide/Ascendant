using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object.Prediction;

// Defines data needed for the server to replicate movement logic.
public struct MoveData : IReplicateData
{
    public Ascendant.InputData inputData;
    public Vector3 cameraForward;
    public Vector3 cameraRight;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

public struct MoveReconcileData : IReconcileData
{
    public Vector3 position;
    public Quaternion rotation;
    public float verticalVelocity;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}