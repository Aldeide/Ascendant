using System;
using AbilitySystem.Scripts;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Ascendant.Systems.Inventory
{
    public struct InventoryItem : INetworkSerializeByMemcpy, IEquatable<InventoryItem>
    {
        public FixedString32Bytes ItemId;
        public int Quantity;

        public bool Equals(InventoryItem other)
        {
            return ItemId == other.ItemId && Quantity == other.Quantity;
        }
    }

    public class NetworkInventory : NetworkBehaviour
    {
        [SerializeField] private int m_MaxCapacity = 1000;

        private AbilitySystemComponent m_AbilitySystemComp;

        public readonly NetworkList<InventoryItem> Items = new NetworkList<InventoryItem>();

        private void Awake()
        {
            m_AbilitySystemComp = GetComponent<AbilitySystemComponent>();
        }

        public int MaxCapacity
        {
            get
            {
                if (m_AbilitySystemComp != null && m_AbilitySystemComp.AbilitySystem != null)
                {
                    var attr = m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("CargoCapacity");
                    if (attr != null)
                    {
                        return (int)attr.CurrentValue;
                    }
                }
                return m_MaxCapacity;
            }
            set => m_MaxCapacity = value;
        }

        public int GetAmount(string itemId)
        {
            FixedString32Bytes id = itemId;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ItemId == id)
                {
                    return Items[i].Quantity;
                }
            }
            return 0;
        }

        public int GetTotalAmount()
        {
            int total = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                total += Items[i].Quantity;
            }
            return total;
        }

        public bool CanAdd(string itemId, int amount)
        {
            if (amount <= 0) return false;
            return GetTotalAmount() + amount <= MaxCapacity;
        }

        public bool AddResource(string itemId, int amount)
        {
            if (NetworkManager.Singleton != null && !IsServer) return false;
            if (!CanAdd(itemId, amount)) return false;

            FixedString32Bytes id = itemId;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ItemId == id)
                {
                    var item = Items[i];
                    item.Quantity += amount;
                    Items[i] = item;
                    return true;
                }
            }

            // Item doesn't exist yet, add it
            Items.Add(new InventoryItem { ItemId = id, Quantity = amount });
            return true;
        }

        public bool RemoveResource(string itemId, int amount)
        {
            if (NetworkManager.Singleton != null && !IsServer) return false;
            if (amount <= 0 || GetAmount(itemId) < amount) return false;

            FixedString32Bytes id = itemId;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ItemId == id)
                {
                    var item = Items[i];
                    item.Quantity -= amount;
                    if (item.Quantity <= 0)
                    {
                        Items.RemoveAt(i);
                    }
                    else
                    {
                        Items[i] = item;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
