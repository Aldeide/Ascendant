using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    [RequireComponent(typeof(ResourceInventory))]
    public class ResourceStorageHub : NetworkBehaviour
    {
        [SerializeField] private float m_UpkeepInterval = 10.0f;
        [SerializeField] private int m_UpkeepCost = 1;
        [SerializeField] private ResourceType m_UpkeepType = ResourceType.Components;

        private ResourceInventory m_Inventory;
        private float m_Timer;
        private bool m_IsDecaying;

        public float UpkeepInterval
        {
            get => m_UpkeepInterval;
            set => m_UpkeepInterval = value;
        }

        public int UpkeepCost
        {
            get => m_UpkeepCost;
            set => m_UpkeepCost = value;
        }

        public ResourceType UpkeepType
        {
            get => m_UpkeepType;
            set => m_UpkeepType = value;
        }

        public bool IsDecaying => m_IsDecaying;

        private void Awake()
        {
            m_Inventory = GetComponent<ResourceInventory>();
        }

        private void Update()
        {
            // Only simulate upkeep consumption on the server
            if (NetworkManager.Singleton != null && !IsServer) return;

            m_Timer += Time.deltaTime;
            if (m_Timer >= m_UpkeepInterval)
            {
                m_Timer -= m_UpkeepInterval;
                ConsumeUpkeep();
            }
        }

        private void ConsumeUpkeep()
        {
            if (m_Inventory == null) return;

            if (m_Inventory.GetAmount(m_UpkeepType) >= m_UpkeepCost)
            {
                m_Inventory.RemoveResource(m_UpkeepType, m_UpkeepCost);
                m_IsDecaying = false;
            }
            else
            {
                m_IsDecaying = true;
                Debug.LogWarning($"[ResourceStorageHub] WARNING: Structure '{gameObject.name}' is decaying due to lack of upkeep resources ({m_UpkeepType})!");
            }
        }
    }
}
