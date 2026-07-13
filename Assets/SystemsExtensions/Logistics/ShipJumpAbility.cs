using AbilitySystem.Runtime.Abilities;
using AbilitySystem.Runtime.Core;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    public class ShipJumpAbility : Ability
    {
        public ShipJumpAbility() : base()
        {
        }

        public ShipJumpAbility(AbilityDefinition definition, IAbilitySystem owner, int level = 1) 
            : base(definition, owner, level)
        {
        }

        protected override void ActivateAbility(AbilityData data)
        {
            var asc = Owner.NetworkRole as MonoBehaviour;
            if (asc != null)
            {
                var attr = Owner.AttributeSetManager.GetAttribute("WarpFuel");
                var rateAttr = Owner.AttributeSetManager.GetAttribute("FuelConsumptionRate");

                float fuelCost = 20.0f;
                if (rateAttr != null)
                {
                    fuelCost *= rateAttr.CurrentValue;
                }

                if (attr != null && attr.CurrentValue >= fuelCost)
                {
                    // Consume warp fuel
                    attr.SetBaseValue(attr.BaseValue - fuelCost);
                    Debug.Log($"[ShipJumpAbility] Jump ability activated. Fuel consumed: {fuelCost}. Current fuel: {attr.CurrentValue}");

                    // Execute scene transition
                    var connectionManager = Object.FindFirstObjectByType<SystemConnectionManager>();
                    if (connectionManager != null)
                    {
                        int targetSystemId = Mathf.RoundToInt(data.TargetPosition.x);
                        string destinationSystem = targetSystemId == 1 ? "SystemBeta" : "SystemAlpha";
                        connectionManager.TransitionShip(asc.gameObject, destinationSystem);
                    }
                }
                else
                {
                    Debug.LogWarning($"[ShipJumpAbility] Jump failed: Insufficient Warp Fuel! Required: {fuelCost}, Current: {(attr != null ? attr.CurrentValue : 0)}");
                }
            }
            EndAbility();
        }

        public override void EndAbility()
        {
            IsActive = false;
        }
    }
}
