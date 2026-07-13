using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    [RequireComponent(typeof(ResourceInventory))]
    public class AsteroidMiningRig : NetworkBehaviour
    {
        [SerializeField] private float m_ExtractionInterval = 2.0f;
        [SerializeField] private int m_ExtractionAmount = 5;

        private ResourceInventory m_Inventory;
        private float m_Timer;

        public float ExtractionInterval
        {
            get => m_ExtractionInterval;
            set => m_ExtractionInterval = value;
        }

        public int ExtractionAmount
        {
            get => m_ExtractionAmount;
            set => m_ExtractionAmount = value;
        }

        private void Awake()
        {
            m_Inventory = GetComponent<ResourceInventory>();
        }

        private void Update()
        {
            // Only simulate extraction on the server
            if (NetworkManager.Singleton != null && !IsServer) return;

            m_Timer += Time.deltaTime;
            if (m_Timer >= m_ExtractionInterval)
            {
                m_Timer -= m_ExtractionInterval;
                ExtractOre();
            }
        }

        private void ExtractOre()
        {
            if (m_Inventory != null)
            {
                m_Inventory.AddResource(ResourceType.Ore, m_ExtractionAmount);
            }
        }
    }
}
