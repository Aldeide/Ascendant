using AbilitySystem.Scripts;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Progression
{
    public class PlayerSpecializationTree : MonoBehaviour
    {
        private AbilitySystemComponent m_AbilitySystemComp;

        private void Awake()
        {
            m_AbilitySystemComp = GetComponent<AbilitySystemComponent>();
        }

        public void ApplyLogisticsSpecialization()
        {
            if (m_AbilitySystemComp == null || m_AbilitySystemComp.AbilitySystem == null) return;

            var cargoAttr = m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("CargoCapacity");
            if (cargoAttr != null)
            {
                // Increase cargo capacity for logistics spec
                cargoAttr.SetBaseValue(cargoAttr.BaseValue * 2f);
                Debug.Log($"[PlayerSpecializationTree] Applied Logistics Specialization! Cargo Capacity scaled to {cargoAttr.CurrentValue}");
            }
        }

        public void ApplyMiningSpecialization()
        {
            if (m_AbilitySystemComp == null || m_AbilitySystemComp.AbilitySystem == null) return;

            var miningAttr = m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("MiningSpeed");
            if (miningAttr != null)
            {
                // Increase mining speed for mining spec
                miningAttr.SetBaseValue(miningAttr.BaseValue * 3f);
                Debug.Log($"[PlayerSpecializationTree] Applied Mining Specialization! Mining Speed scaled to {miningAttr.CurrentValue}");
            }
        }
    }
}
