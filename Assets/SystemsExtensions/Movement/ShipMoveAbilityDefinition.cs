using System;
using AbilitySystem.Runtime.Abilities;
using AbilitySystem.Runtime.Core;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Movement
{
    [CreateAssetMenu(fileName = "ShipMoveAbilityDefinition", menuName = "AbilitySystem/Abilities/ShipMoveAbilityDefinition")]
    public class ShipMoveAbilityDefinition : AbilityDefinition
    {
        public ShipMoveAbilityDefinition() : base()
        {
            NetworkPolicy = AbilityNetworkPolicy.ClientPredicted;
            NetworkSecurityPolicy = AbilityNetworkSecurityPolicy.ClientOrServer;
            UniqueName = "ShipMoveAbility";
        }

        public override Type AbilityType() => typeof(ShipMoveAbility);

        public override Ability ToAbility(IAbilitySystem owner)
        {
            return new ShipMoveAbility(this, owner);
        }
    }
}
