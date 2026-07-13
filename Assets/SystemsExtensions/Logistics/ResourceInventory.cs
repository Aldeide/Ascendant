using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    public enum ResourceType
    {
        Ore,
        Gas,
        Fuel,
        Munitions,
        Components
    }

    public struct ResourceInventoryState : INetworkSerializeByMemcpy
    {
        public int Ore;
        public int Gas;
        public int Fuel;
        public int Munitions;
        public int Components;
    }

    public class ResourceInventory : NetworkBehaviour
    {
        [SerializeField] private int m_MaxCapacity = 1000;
        
        public int MaxCapacity
        {
            get => m_MaxCapacity;
            set => m_MaxCapacity = value;
        }

        public readonly NetworkVariable<ResourceInventoryState> State = new NetworkVariable<ResourceInventoryState>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public int GetAmount(ResourceType type)
        {
            var s = State.Value;
            return type switch
            {
                ResourceType.Ore => s.Ore,
                ResourceType.Gas => s.Gas,
                ResourceType.Fuel => s.Fuel,
                ResourceType.Munitions => s.Munitions,
                ResourceType.Components => s.Components,
                _ => 0
            };
        }

        public int GetTotalAmount()
        {
            var s = State.Value;
            return s.Ore + s.Gas + s.Fuel + s.Munitions + s.Components;
        }

        public bool CanAdd(ResourceType type, int amount)
        {
            if (amount <= 0) return false;
            return GetTotalAmount() + amount <= m_MaxCapacity;
        }

        public bool AddResource(ResourceType type, int amount)
        {
            if (NetworkManager.Singleton != null && !IsServer) return false;
            if (!CanAdd(type, amount)) return false;

            var s = State.Value;
            switch (type)
            {
                case ResourceType.Ore: s.Ore += amount; break;
                case ResourceType.Gas: s.Gas += amount; break;
                case ResourceType.Fuel: s.Fuel += amount; break;
                case ResourceType.Munitions: s.Munitions += amount; break;
                case ResourceType.Components: s.Components += amount; break;
            }
            State.Value = s;
            return true;
        }

        public bool RemoveResource(ResourceType type, int amount)
        {
            if (NetworkManager.Singleton != null && !IsServer) return false;
            if (amount <= 0 || GetAmount(type) < amount) return false;

            var s = State.Value;
            switch (type)
            {
                case ResourceType.Ore: s.Ore -= amount; break;
                case ResourceType.Gas: s.Gas -= amount; break;
                case ResourceType.Fuel: s.Fuel -= amount; break;
                case ResourceType.Munitions: s.Munitions -= amount; break;
                case ResourceType.Components: s.Components -= amount; break;
            }
            State.Value = s;
            return true;
        }
    }
}
