using AbilitySystem.Scripts;
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
        private AbilitySystemComponent m_AbilitySystemComp;
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
            m_AbilitySystemComp = GetComponent<AbilitySystemComponent>();
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
                float multiplier = 1.0f;
                // Scale gas harvest yield dynamically based on MiningSpeed attribute
                if (m_AbilitySystemComp != null && m_AbilitySystemComp.AbilitySystem != null)
                {
                    var attr = m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("MiningSpeed");
                    if (attr != null)
                    {
                        multiplier = attr.CurrentValue;
                    }
                }
                int finalAmount = Mathf.RoundToInt(m_HarvestAmount * multiplier);
                m_Inventory.AddResource(ResourceType.Gas, finalAmount);
            }
        }
    }
}
