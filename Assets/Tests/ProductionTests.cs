using NUnit.Framework;
using UnityEngine;
using AbilitySystem.Scripts;
using AbilitySystem.Runtime.Core;
using Ascendant.SystemsExtensions.Logistics;

namespace Ascendant.Tests
{
    public class ProductionTests
    {
        private GameObject m_TestObject;
        private GameObject m_ShipObject;

        [SetUp]
        public void SetUp()
        {
            m_TestObject = new GameObject("TestProductionFacility");
            m_ShipObject = new GameObject("TestShip");
        }

        [TearDown]
        public void TearDown()
        {
            if (m_TestObject != null) Object.DestroyImmediate(m_TestObject);
            if (m_ShipObject != null) Object.DestroyImmediate(m_ShipObject);
        }

        [Test]
        public void Test_RefineryProcessRecipe()
        {
            var inventory = m_TestObject.AddComponent<ResourceInventory>();
            inventory.MaxCapacity = 100;
            var refinery = m_TestObject.AddComponent<Refinery>();
            refinery.ProcessInterval = 0.1f;
            refinery.OreCost = 5;
            refinery.GasCost = 2;
            refinery.ComponentsYield = 1;
            refinery.FuelYield = 3;

            var processMethod = typeof(Refinery).GetMethod("ProcessRecipe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Add raw materials
            inventory.AddResource(ResourceType.Ore, 12);
            inventory.AddResource(ResourceType.Gas, 5);

            // Run process recipe once
            processMethod.Invoke(refinery, null);
            Assert.AreEqual(7, inventory.GetAmount(ResourceType.Ore));  // 12 - 5
            Assert.AreEqual(3, inventory.GetAmount(ResourceType.Gas));  // 5 - 2
            Assert.AreEqual(1, inventory.GetAmount(ResourceType.Components));
            Assert.AreEqual(3, inventory.GetAmount(ResourceType.Fuel));

            // Run process recipe second time
            processMethod.Invoke(refinery, null);
            Assert.AreEqual(2, inventory.GetAmount(ResourceType.Ore));  // 7 - 5
            Assert.AreEqual(1, inventory.GetAmount(ResourceType.Gas));  // 3 - 2
            Assert.AreEqual(2, inventory.GetAmount(ResourceType.Components));
            Assert.AreEqual(6, inventory.GetAmount(ResourceType.Fuel));

            // Run process recipe third time (should fail due to gas shortage)
            processMethod.Invoke(refinery, null);
            Assert.AreEqual(2, inventory.GetAmount(ResourceType.Ore)); // No changes
            Assert.AreEqual(1, inventory.GetAmount(ResourceType.Gas));
        }

        [Test]
        public void Test_MunitionsFactoryProcessRecipe()
        {
            var inventory = m_TestObject.AddComponent<ResourceInventory>();
            inventory.MaxCapacity = 100;
            var factory = m_TestObject.AddComponent<MunitionsFactory>();
            factory.ProcessInterval = 0.1f;
            factory.OreCost = 10;
            factory.ComponentsCost = 3;
            factory.MunitionsYield = 1;

            var processMethod = typeof(MunitionsFactory).GetMethod("ProcessRecipe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Add ingredients
            inventory.AddResource(ResourceType.Ore, 25);
            inventory.AddResource(ResourceType.Components, 5);

            // Run once
            processMethod.Invoke(factory, null);
            Assert.AreEqual(15, inventory.GetAmount(ResourceType.Ore));       // 25 - 10
            Assert.AreEqual(2, inventory.GetAmount(ResourceType.Components)); // 5 - 3
            Assert.AreEqual(1, inventory.GetAmount(ResourceType.Munitions));

            // Run second time (should fail due to component shortage)
            processMethod.Invoke(factory, null);
            Assert.AreEqual(15, inventory.GetAmount(ResourceType.Ore));
            Assert.AreEqual(2, inventory.GetAmount(ResourceType.Components));
            Assert.AreEqual(1, inventory.GetAmount(ResourceType.Munitions));
        }

        [Test]
        public void Test_RefuelingDepotTransfer()
        {
            // Set up refueling depot
            var depotInventory = m_TestObject.AddComponent<ResourceInventory>();
            depotInventory.MaxCapacity = 100;
            var depot = m_TestObject.AddComponent<RefuelingDepot>();

            // Stock depot with fuel
            depotInventory.AddResource(ResourceType.Fuel, 50);

            // Set up ship with GAS attributes
            var asc = m_ShipObject.AddComponent<AbilitySystemComponent>();
            asc.Initialise();

            var attributeSet = new ShipAttributeSet(asc.AbilitySystem);
            asc.AbilitySystem.AttributeSetManager.AddAttributeSet(typeof(ShipAttributeSet), attributeSet);

            // Ship starts with 30 fuel out of 100 capacity
            attributeSet.WarpFuel.SetBaseValue(30f);
            attributeSet.MaxWarpFuel.SetBaseValue(100f);

            // Refuel ship
            bool success = depot.RefuelShip(m_ShipObject);
            Assert.IsTrue(success);

            // Ship should have siphoned 50 fuel (depot's entire stock) and now has 80
            Assert.AreEqual(80f, attributeSet.WarpFuel.CurrentValue);
            Assert.AreEqual(0, depotInventory.GetAmount(ResourceType.Fuel));

            // Stock depot with more fuel (e.g. 40 fuel)
            depotInventory.AddResource(ResourceType.Fuel, 40);

            // Refuel ship again
            success = depot.RefuelShip(m_ShipObject);
            Assert.IsTrue(success);

            // Ship needed only 20 fuel to reach max (100 - 80 = 20), so depot should have 20 fuel remaining
            Assert.AreEqual(100f, attributeSet.WarpFuel.CurrentValue);
            Assert.AreEqual(20, depotInventory.GetAmount(ResourceType.Fuel));
        }
    }
}
