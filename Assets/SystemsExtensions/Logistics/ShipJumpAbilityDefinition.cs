using System;
using AbilitySystem.Runtime.Abilities;
using AbilitySystem.Runtime.Core;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    [CreateAssetMenu(fileName = "ShipJumpAbilityDefinition", menuName = "AbilitySystem/Abilities/ShipJumpAbilityDefinition")]
    public class ShipJumpAbilityDefinition : AbilityDefinition
    {
        public ShipJumpAbilityDefinition() : base()
        {
            NetworkPolicy = AbilityNetworkPolicy.ClientPredicted;
            NetworkSecurityPolicy = AbilityNetworkSecurityPolicy.ClientOrServer;
            UniqueName = "ShipJumpAbility";
        }

        public override Type AbilityType() => typeof(ShipJumpAbility);

        public override Ability ToAbility(IAbilitySystem owner)
        {
            return new ShipJumpAbility(this, owner);
        }
    }
}
