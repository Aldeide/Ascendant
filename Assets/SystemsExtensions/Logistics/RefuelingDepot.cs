using AbilitySystem.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    [RequireComponent(typeof(ResourceInventory))]
    public class RefuelingDepot : NetworkBehaviour
    {
        private ResourceInventory m_Inventory;

        private void Awake()
        {
            m_Inventory = GetComponent<ResourceInventory>();
        }

        public bool RefuelShip(GameObject ship)
        {
            if (NetworkManager.Singleton != null && !IsServer) return false;
            if (m_Inventory == null) return false;

            var asc = ship.GetComponent<AbilitySystemComponent>();
            if (asc == null || asc.AbilitySystem == null) return false;

            var fuelAttr = asc.AbilitySystem.AttributeSetManager.GetAttribute("WarpFuel");
            var maxFuelAttr = asc.AbilitySystem.AttributeSetManager.GetAttribute("MaxWarpFuel");

            if (fuelAttr == null || maxFuelAttr == null) return false;

            float currentFuel = fuelAttr.CurrentValue;
            float maxFuel = maxFuelAttr.CurrentValue;
            float fuelNeeded = maxFuel - currentFuel;

            if (fuelNeeded <= 0f) return false;

            int fuelToTransfer = Mathf.Min(m_Inventory.GetAmount(ResourceType.Fuel), Mathf.CeilToInt(fuelNeeded));
            if (fuelToTransfer <= 0) return false;

            m_Inventory.RemoveResource(ResourceType.Fuel, fuelToTransfer);
            fuelAttr.SetBaseValue(fuelAttr.BaseValue + fuelToTransfer);
            
            Debug.Log($"[RefuelingDepot] Refueled ship '{ship.name}'. Transferred: {fuelToTransfer} Fuel.");
            return true;
        }
    }
}
