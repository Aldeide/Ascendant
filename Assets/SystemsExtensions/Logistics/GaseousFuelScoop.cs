using Unity.Netcode;
using UnityEngine;
using Ascendant.Systems.Structures;

namespace Ascendant.SystemsExtensions.Logistics
{
    public class GaseousFuelScoop : StructureBase
    {
        [SerializeField] private float m_HarvestInterval = 2.0f;
        [SerializeField] private int m_HarvestAmount = 5;

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
                if (m_AbilitySystemComp != null && m_AbilitySystemComp.AbilitySystem != null)
                {
                    var attr = m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("MiningSpeed");
                    if (attr != null)
                    {
                        multiplier = attr.CurrentValue;
                    }
                }
                int finalAmount = Mathf.RoundToInt(m_HarvestAmount * multiplier);
                m_Inventory.AddResource("Gas", finalAmount);
            }
        }
    }
}
