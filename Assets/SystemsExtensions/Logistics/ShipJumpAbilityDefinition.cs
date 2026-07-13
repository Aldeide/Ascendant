using AbilitySystem.Runtime.Abilities;
using AbilitySystem.Runtime.Core;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    [CreateAssetMenu(menuName = "AbilitySystem/Abilities/ShipJumpAbilityDefinition")]
    public class ShipJumpAbilityDefinition : AbilityDefinition
    {
        public override Ability CreateAbilityInstance(IAbilitySystem owner, int level)
        {
            return new ShipJumpAbility(this, owner, level);
        }
    }
}
