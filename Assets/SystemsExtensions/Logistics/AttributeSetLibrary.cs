using System;
using System.Collections.Generic;
using Ascendant.SystemsExtensions.Logistics;

namespace AbilitySystemExtension.Runtime.AttributeSets
{
    public static class AttributeSetLibrary
    {
        public static List<string> AttributeFullNames = new List<string>
        {
            "ShipAttributeSet.CargoCapacity",
            "ShipAttributeSet.MiningSpeed",
            "ShipAttributeSet.FuelConsumptionRate",
            "ShipAttributeSet.WarpFuel",
            "ShipAttributeSet.MaxWarpFuel"
        };

        public static Dictionary<string, Type> AttributeSetTypeDict = new Dictionary<string, Type>
        {
            { "ShipAttributeSet", typeof(ShipAttributeSet) }
        };
    }
}
