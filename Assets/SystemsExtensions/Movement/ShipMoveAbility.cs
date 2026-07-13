using System;
using AbilitySystem.Runtime.Abilities;
using AbilitySystem.Runtime.Core;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Movement
{
    public class ShipMoveAbility : Ability
    {
        public ShipMoveAbility() : base()
        {
        }

        public ShipMoveAbility(AbilityDefinition definition, IAbilitySystem owner, int level = 1) 
            : base(definition, owner, level)
        {
        }

        protected override void ActivateAbility(AbilityData data)
        {
            var asc = Owner.NetworkRole as MonoBehaviour;
            if (asc != null)
            {
                var shipController = asc.GetComponent<ShipController>();
                if (shipController != null)
                {
                    Debug.Log($"[ShipMoveAbility] Executing move ability. Target: {data.TargetPosition} on server={Owner.IsServer()} client={Owner.IsLocalClient()}");
                    shipController.SetTargetFromAbility(data.TargetPosition);
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
