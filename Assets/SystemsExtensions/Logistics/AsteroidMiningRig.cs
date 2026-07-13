using AbilitySystem.Scripts;
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
        private AbilitySystemComponent m_AbilitySystemComp;
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
            m_AbilitySystemComp = GetComponent<AbilitySystemComponent>();
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
                float multiplier = 1.0f;
                // Scale extraction amount dynamically based on MiningSpeed attribute
                if (m_AbilitySystemComp != null && m_AbilitySystemComp.AbilitySystem != null)
                {
                    var attr = m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("MiningSpeed");
                    if (attr != null)
                    {
                        multiplier = attr.CurrentValue;
                    }
                }
                int finalAmount = Mathf.RoundToInt(m_ExtractionAmount * multiplier);
                m_Inventory.AddResource(ResourceType.Ore, finalAmount);
            }
        }
    }
}
