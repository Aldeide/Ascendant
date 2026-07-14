using AbilitySystem.Runtime.Attributes;
using AbilitySystem.Runtime.AttributeSets;
using AbilitySystem.Runtime.Core;

namespace Ascendant.Systems.Structures
{
    public class StructureAttributeSet : AttributeSet
    {
        public Attribute StructureHealth;
        public Attribute StructureMaxHealth;
        public Attribute UpkeepFuel;
        public Attribute MaxUpkeepFuel;
        public Attribute OperationEfficiency;

        public StructureAttributeSet(IAbilitySystem owner) : base(owner)
        {
            Name = nameof(StructureAttributeSet);
            StructureHealth = new Attribute("StructureHealth", this, 1000f);
            StructureMaxHealth = new Attribute("StructureMaxHealth", this, 1000f);
            UpkeepFuel = new Attribute("UpkeepFuel", this, 100f);
            MaxUpkeepFuel = new Attribute("MaxUpkeepFuel", this, 100f);
            OperationEfficiency = new Attribute("OperationEfficiency", this, 1.0f);

            AddAttribute(StructureHealth);
            AddAttribute(StructureMaxHealth);
            AddAttribute(UpkeepFuel);
            AddAttribute(MaxUpkeepFuel);
            AddAttribute(OperationEfficiency);
        }

        public override void Reset()
        {
            return;
        }
    }
}
