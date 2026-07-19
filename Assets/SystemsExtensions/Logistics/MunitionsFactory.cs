using Unity.Netcode;
using UnityEngine;
using Ascendant.Systems.Structures;

namespace Ascendant.SystemsExtensions.Logistics
{
    public class MunitionsFactory : StructureBase
    {
        [SerializeField] private float m_ProcessInterval = 5.0f;
        [SerializeField] private int m_OreCost = 10;
        [SerializeField] private int m_ComponentsCost = 3;
        [SerializeField] private int m_MunitionsYield = 1;

        private float m_Timer;

        public float ProcessInterval
        {
            get => m_ProcessInterval;
            set => m_ProcessInterval = value;
        }

        public int OreCost { get => m_OreCost; set => m_OreCost = value; }
        public int ComponentsCost { get => m_ComponentsCost; set => m_ComponentsCost = value; }
        public int MunitionsYield { get => m_MunitionsYield; set => m_MunitionsYield = value; }

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
                m_Inventory.GetAmount("Components") >= m_ComponentsCost &&
                m_Inventory.CanAdd("Munitions", m_MunitionsYield))
            {
                m_Inventory.RemoveResource("Ore", m_OreCost);
                m_Inventory.RemoveResource("Components", m_ComponentsCost);
                m_Inventory.AddResource("Munitions", m_MunitionsYield);
                Debug.Log($"[MunitionsFactory] Manufactured Munitions. Spent: {m_OreCost} Ore, {m_ComponentsCost} Components.");
            }
        }
    }
}
