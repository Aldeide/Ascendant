using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    [RequireComponent(typeof(ResourceInventory))]
    public class GaseousFuelScoop : NetworkBehaviour
    {
        [SerializeField] private float m_HarvestInterval = 2.0f;
        [SerializeField] private int m_HarvestAmount = 5;

        private ResourceInventory m_Inventory;
        private float m_Timer;

        public float HarvestInterval
        {
            get => m_HarvestInterval;
            set => m_HarvestInterval = value;
        }

        public int HarvestAmount
        {
            get => m_HarvestAmount;
            set => m_HarvestAmount = value;
        }

        private void Awake()
        {
            m_Inventory = GetComponent<ResourceInventory>();
        }

        private void Update()
        {
            // Only simulate harvesting on the server
            if (NetworkManager.Singleton != null && !IsServer) return;

            m_Timer += Time.deltaTime;
            if (m_Timer >= m_HarvestInterval)
            {
                m_Timer -= m_HarvestInterval;
                HarvestGas();
            }
        }

        private void HarvestGas()
        {
            if (m_Inventory != null)
            {
                m_Inventory.AddResource(ResourceType.Gas, m_HarvestAmount);
            }
        }
    }
}
