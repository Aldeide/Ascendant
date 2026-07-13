using NUnit.Framework;
using UnityEngine;
using Ascendant.SystemsExtensions.Logistics;

namespace Ascendant.Tests
{
    public class LogisticsTests
    {
        private GameObject m_TestObject;

        [SetUp]
        public void SetUp()
        {
            m_TestObject = new GameObject("TestLogisticsObject");
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
        public void Test_InventoryAddAndRemove()
        {
            var inventory = m_TestObject.AddComponent<ResourceInventory>();
            inventory.MaxCapacity = 100;

            // Test adding within capacity
            Assert.IsTrue(inventory.AddResource(ResourceType.Ore, 60));
            Assert.AreEqual(60, inventory.GetAmount(ResourceType.Ore));
            Assert.AreEqual(60, inventory.GetTotalAmount());

            // Test adding exceeding capacity
            Assert.IsFalse(inventory.AddResource(ResourceType.Gas, 50));
            Assert.AreEqual(0, inventory.GetAmount(ResourceType.Gas));
            Assert.AreEqual(60, inventory.GetTotalAmount());

            // Test successful removal
            Assert.IsTrue(inventory.RemoveResource(ResourceType.Ore, 20));
            Assert.AreEqual(40, inventory.GetAmount(ResourceType.Ore));
            Assert.AreEqual(40, inventory.GetTotalAmount());

            // Test removing more than exists
            Assert.IsFalse(inventory.RemoveResource(ResourceType.Ore, 50));
            Assert.AreEqual(40, inventory.GetAmount(ResourceType.Ore));
        }

        [Test]
        public void Test_MiningRigExtraction()
        {
            var inventory = m_TestObject.AddComponent<ResourceInventory>();
            inventory.MaxCapacity = 100;
            var rig = m_TestObject.AddComponent<AsteroidMiningRig>();
            rig.ExtractionInterval = 0.1f;
            rig.ExtractionAmount = 5;

            var extractMethod = typeof(AsteroidMiningRig).GetMethod("ExtractOre", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.AreEqual(0, inventory.GetAmount(ResourceType.Ore));
            extractMethod.Invoke(rig, null);
            Assert.AreEqual(5, inventory.GetAmount(ResourceType.Ore));
        }

        [Test]
        public void Test_GaseousFuelScoopHarvesting()
        {
            var inventory = m_TestObject.AddComponent<ResourceInventory>();
            inventory.MaxCapacity = 100;
            var scoop = m_TestObject.AddComponent<GaseousFuelScoop>();
            scoop.HarvestInterval = 0.1f;
            scoop.HarvestAmount = 8;

            var harvestMethod = typeof(GaseousFuelScoop).GetMethod("HarvestGas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.AreEqual(0, inventory.GetAmount(ResourceType.Gas));
            harvestMethod.Invoke(scoop, null);
            Assert.AreEqual(8, inventory.GetAmount(ResourceType.Gas));
        }

        [Test]
        public void Test_StorageHubUpkeepConsumption()
        {
            var inventory = m_TestObject.AddComponent<ResourceInventory>();
            inventory.MaxCapacity = 100;
            var hub = m_TestObject.AddComponent<ResourceStorageHub>();
            hub.UpkeepInterval = 1f;
            hub.UpkeepCost = 2;
            hub.UpkeepType = ResourceType.Components;

            var consumeMethod = typeof(ResourceStorageHub).GetMethod("ConsumeUpkeep", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Set up inventory with components
            inventory.AddResource(ResourceType.Components, 5);

            // Consume once
            consumeMethod.Invoke(hub, null);
            Assert.AreEqual(3, inventory.GetAmount(ResourceType.Components));
            Assert.IsFalse(hub.IsDecaying);

            // Consume twice
            consumeMethod.Invoke(hub, null);
            Assert.AreEqual(1, inventory.GetAmount(ResourceType.Components));
            Assert.IsFalse(hub.IsDecaying);

            // Consume third time (should fail and trigger decay)
            consumeMethod.Invoke(hub, null);
            Assert.AreEqual(1, inventory.GetAmount(ResourceType.Components));
            Assert.IsTrue(hub.IsDecaying);
        }
    }
}
