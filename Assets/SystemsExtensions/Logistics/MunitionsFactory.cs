using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    [RequireComponent(typeof(ResourceInventory))]
    public class MunitionsFactory : NetworkBehaviour
    {
        [SerializeField] private float m_ProcessInterval = 5.0f;
        [SerializeField] private int m_OreCost = 10;
        [SerializeField] private int m_ComponentsCost = 3;
        [SerializeField] private int m_MunitionsYield = 1;

        private ResourceInventory m_Inventory;
        private float m_Timer;

        public float ProcessInterval
        {
            get => m_ProcessInterval;
            set => m_ProcessInterval = value;
        }

        public int OreCost { get => m_OreCost; set => m_OreCost = value; }
        public int ComponentsCost { get => m_ComponentsCost; set => m_ComponentsCost = value; }
        public int MunitionsYield { get => m_MunitionsYield; set => m_MunitionsYield = value; }

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

            if (m_Inventory.GetAmount(ResourceType.Ore) >= m_OreCost &&
                m_Inventory.GetAmount(ResourceType.Components) >= m_ComponentsCost &&
                m_Inventory.CanAdd(ResourceType.Munitions, m_MunitionsYield))
            {
                m_Inventory.RemoveResource(ResourceType.Ore, m_OreCost);
                m_Inventory.RemoveResource(ResourceType.Components, m_ComponentsCost);
                m_Inventory.AddResource(ResourceType.Munitions, m_MunitionsYield);
                Debug.Log($"[MunitionsFactory] Manufactured Munitions. Spent: {m_OreCost} Ore, {m_ComponentsCost} Components.");
            }
        }
    }
}
