using AbilitySystem.Runtime.Attributes;
using AbilitySystem.Runtime.AttributeSets;
using AbilitySystem.Runtime.Core;

namespace Ascendant.SystemsExtensions.Logistics
{
    public class ShipAttributeSet : AttributeSet
    {
        public Attribute CargoCapacity;
        public Attribute MiningSpeed;
        public Attribute FuelConsumptionRate;
        public Attribute WarpFuel;
        public Attribute MaxWarpFuel;

        public ShipAttributeSet(IAbilitySystem owner) : base(owner)
        {
            Name = nameof(ShipAttributeSet);
            CargoCapacity = new Attribute("CargoCapacity", this, 1000);
            MiningSpeed = new Attribute("MiningSpeed", this, 1.0f);
            FuelConsumptionRate = new Attribute("FuelConsumptionRate", this, 1.0f);
            WarpFuel = new Attribute("WarpFuel", this, 100);
            MaxWarpFuel = new Attribute("MaxWarpFuel", this, 100);

            AddAttribute(CargoCapacity);
            AddAttribute(MiningSpeed);
            AddAttribute(FuelConsumptionRate);
            AddAttribute(WarpFuel);
            AddAttribute(MaxWarpFuel);
        }

        public override void Reset()
        {
            return;
        }
    }
}
