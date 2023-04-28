using UnityEngine;
using FishNet.Object.Prediction;

namespace Ascendant.NetworkData
{
    // Defines data needed for the server to replicate dash movement logic.
    public struct DashData : IReplicateData
    {
        public bool isDashing;
        public int charges;
        public float lastDash;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct DashReconcileData : IReconcileData
    {
        public Vector3 position;
        public Quaternion rotation;
        public int charges;
        public float lastDash;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
}

