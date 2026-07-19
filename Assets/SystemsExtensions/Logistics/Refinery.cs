using Unity.Netcode;
using UnityEngine;
using Ascendant.Systems.Structures;

namespace Ascendant.SystemsExtensions.Logistics
{
    public class Refinery : StructureBase
    {
        [SerializeField] private float m_ProcessInterval = 3.0f;
        [SerializeField] private int m_OreCost = 5;
        [SerializeField] private int m_GasCost = 2;
        [SerializeField] private int m_ComponentsYield = 1;
        [SerializeField] private int m_FuelYield = 3;

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

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();

            if (NetworkManager.Singleton != null && !IsServer) return;
            if (IsUnderConstruction.Value || IsDisabled.Value) return;

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

            if (m_Inventory.GetAmount("Ore") >= m_OreCost &&
                m_Inventory.GetAmount("Gas") >= m_GasCost &&
                m_Inventory.CanAdd("Components", m_ComponentsYield) &&
                m_Inventory.CanAdd("Fuel", m_FuelYield))
            {
                m_Inventory.RemoveResource("Ore", m_OreCost);
                m_Inventory.RemoveResource("Gas", m_GasCost);
                m_Inventory.AddResource("Components", m_ComponentsYield);
                m_Inventory.AddResource("Fuel", m_FuelYield);
                Debug.Log($"[Refinery] Refined: Spent {m_OreCost} Ore, {m_GasCost} Gas -> Gained {m_ComponentsYield} Components, {m_FuelYield} Fuel.");
            }
        }
    }
}
