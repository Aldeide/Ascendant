using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    [RequireComponent(typeof(ResourceInventory))]
    public class Refinery : NetworkBehaviour
    {
        [SerializeField] private float m_ProcessInterval = 3.0f;
        [SerializeField] private int m_OreCost = 5;
        [SerializeField] private int m_GasCost = 2;
        [SerializeField] private int m_ComponentsYield = 1;
        [SerializeField] private int m_FuelYield = 3;

        private ResourceInventory m_Inventory;
        private float m_Timer;

        public float ProcessInterval
        {
            get => m_ProcessInterval;
            set => m_ProcessInterval = value;
        }

        public int OreCost { get => m_OreCost; set => m_OreCost = value; }
        public int GasCost { get => m_GasCost; set => m_GasCost = value; }
        public int ComponentsYield { get => m_ComponentsYield; set => m_ComponentsYield = value; }
        public int FuelYield { get => m_FuelYield; set => m_FuelYield = value; }

        private void Awake()
        {
            m_Inventory = GetComponent<ResourceInventory>();
        }

        private void Update()
        {
            if (NetworkManager.Singleton != null && !IsServer) return;

            m_Timer += Time.deltaTime;
            if (m_Timer >= m_ProcessInterval)
            {
                m_Timer -= m_ProcessInterval;
                ProcessRecipe();
            }
        }

        private void ProcessRecipe()
        {
            if (m_Inventory == null) return;

            // Check if we have input ingredients and space for outputs
            if (m_Inventory.GetAmount(ResourceType.Ore) >= m_OreCost &&
                m_Inventory.GetAmount(ResourceType.Gas) >= m_GasCost &&
                m_Inventory.CanAdd(ResourceType.Components, m_ComponentsYield) &&
                m_Inventory.CanAdd(ResourceType.Fuel, m_FuelYield))
            {
                m_Inventory.RemoveResource(ResourceType.Ore, m_OreCost);
                m_Inventory.RemoveResource(ResourceType.Gas, m_GasCost);
                m_Inventory.AddResource(ResourceType.Components, m_ComponentsYield);
                m_Inventory.AddResource(ResourceType.Fuel, m_FuelYield);
                Debug.Log($"[Refinery] Refined: Spent {m_OreCost} Ore, {m_GasCost} Gas $\\rightarrow$ Gained {m_ComponentsYield} Components, {m_FuelYield} Fuel.");
            }
        }
    }
}
