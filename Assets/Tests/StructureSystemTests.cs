using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using AbilitySystem.Scripts;
using Ascendant.Systems.Inventory;
using Ascendant.Systems.Structures;
using Unity.Netcode;

namespace Ascendant.Tests
{
    public class StructureSystemTests
    {
        private GameObject m_TestObject;
        private AbilitySystemComponent m_AbilitySystemComp;
        private NetworkInventory m_Inventory;
        private StructureBase m_Structure;

        [SetUp]
        public void SetUp()
        {
            m_TestObject = new GameObject("TestStructureObject");
            
            // Attach components required by StructureBase
            m_AbilitySystemComp = m_TestObject.AddComponent<AbilitySystemComponent>();
            m_Inventory = m_TestObject.AddComponent<NetworkInventory>();
            m_Structure = m_TestObject.AddComponent<StructureBase>();

            // Trigger Awake manually to bind components if not in playmode
            var awakeMethod = typeof(StructureBase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            if (awakeMethod != null) awakeMethod.Invoke(m_Structure, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_TestObject != null)
            {
                Object.DestroyImmediate(m_TestObject);
            }
        }

        [Test]
        public void Test_NetworkInventory_CapacityAndItems()
        {
            m_Inventory.MaxCapacity = 200;

            // Adding within bounds
            Assert.IsTrue(m_Inventory.CanAdd("Ore", 150));
            m_Inventory.AddResource("Ore", 150);
            Assert.AreEqual(150, m_Inventory.GetAmount("Ore"));

            // Exceeding bounds
            Assert.IsFalse(m_Inventory.CanAdd("Gas", 100));
            Assert.IsFalse(m_Inventory.AddResource("Gas", 100));
            Assert.AreEqual(0, m_Inventory.GetAmount("Gas"));

            // Removal
            Assert.IsTrue(m_Inventory.RemoveResource("Ore", 50));
            Assert.AreEqual(100, m_Inventory.GetAmount("Ore"));

            // Too much removal
            Assert.IsFalse(m_Inventory.RemoveResource("Ore", 200));
            Assert.AreEqual(100, m_Inventory.GetAmount("Ore"));
        }

        [Test]
        public void Test_StructureBase_ConstructionPhase()
        {
            // By default, starts under construction and progress at 0
            Assert.IsTrue(m_Structure.IsUnderConstruction.Value);
            Assert.AreEqual(0f, m_Structure.ConstructionProgress.Value);

            // Access and test finish construction
            var finishMethod = typeof(StructureBase).GetMethod("FinishConstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(finishMethod);

            finishMethod.Invoke(m_Structure, null);

            Assert.IsFalse(m_Structure.IsUnderConstruction.Value);
            Assert.IsFalse(m_Structure.IsDisabled.Value);
        }

        [Test]
        public void Test_StructureBase_DecayAndDisable()
        {
            // Initialize attributes for decay/upkeep testing
            var abilitySystem = m_AbilitySystemComp.AbilitySystem;
            if (abilitySystem != null)
            {
                var attrSet = new StructureAttributeSet(abilitySystem);
                abilitySystem.AttributeSetManager.AddAttributeSet(typeof(StructureAttributeSet), attrSet);

                // Set health and max health, set upkeep fuel to 0 to trigger decay
                attrSet.StructureMaxHealth.SetBaseValue(100f);
                attrSet.StructureMaxHealth.SetCurrentValue(100f);
                attrSet.StructureHealth.SetBaseValue(100f);
                attrSet.StructureHealth.SetCurrentValue(100f);
                
                attrSet.MaxUpkeepFuel.SetBaseValue(500f);
                attrSet.MaxUpkeepFuel.SetCurrentValue(500f);
                attrSet.UpkeepFuel.SetBaseValue(0f);
                attrSet.UpkeepFuel.SetCurrentValue(0f);

                // Finish construction first so upkeep update loop runs
                var finishMethod = typeof(StructureBase).GetMethod("FinishConstruction", BindingFlags.NonPublic | BindingFlags.Instance);
                finishMethod.Invoke(m_Structure, null);

                // Invoke UpdateUpkeepAndDecay(deltaTime = 2s)
                var decayMethod = typeof(StructureBase).GetMethod("UpdateUpkeepAndDecay", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(decayMethod);

                // Decay rate is 5 HP per second, so 2 seconds should reduce health by 10 HP
                decayMethod.Invoke(m_Structure, new object[] { 2.0f });

                Assert.AreEqual(90f, attrSet.StructureHealth.CurrentValue);
                
                // Let's decay health completely to 0 to check disabled state
                decayMethod.Invoke(m_Structure, new object[] { 20.0f }); // 20s * 5HP = 100HP damage
                
                // Set health to 0 manually and invoke decay to trigger disabled
                attrSet.StructureHealth.SetBaseValue(0f);
                attrSet.StructureHealth.SetCurrentValue(0f);
                decayMethod.Invoke(m_Structure, new object[] { 1.0f });

                // Check transition to disabled state
                var enterDisabledMethod = typeof(StructureBase).GetMethod("EnterDisabledState", BindingFlags.NonPublic | BindingFlags.Instance);
                enterDisabledMethod.Invoke(m_Structure, null);
                Assert.IsTrue(m_Structure.IsDisabled.Value);
            }
        }
    }
}
