using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Unity.Netcode;
using AbilitySystem.Scripts;
using Ascendant.Systems.Inventory;
using Ascendant.Systems.Structures;
using Ascendant.SystemsExtensions.Movement;
using Ascendant.SystemsExtensions.Logistics;
using Ascendant.SystemsExtensions.Celestial;
using Mono.Data.Sqlite;

namespace Ascendant.Tests
{
    [TestFixture]
    public class GameIntegrationTests
    {
        private SqliteConnection m_DbConn;
        private WorldDatabaseRepository m_DbRepo;

        [SetUp]
        public void SetUp()
        {
            // Set up a clean in-memory database for testing
            m_DbConn = new SqliteConnection("Data Source=:memory:;Version=3;New=True;");
            m_DbConn.Open();
            m_DbRepo = new WorldDatabaseRepository(m_DbConn);
            m_DbRepo.CreateTables();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_DbConn != null)
            {
                m_DbConn.Close();
                m_DbConn.Dispose();
            }
        }

        [Test]
        public void Test_FullGameLoop_Integration()
        {
            // 1. Setup Celestial Entity (Asteroid with Ore)
            var asteroidObj = new GameObject("Asteroid_Test");
            var asteroidInv = asteroidObj.AddComponent<ResourceInventory>();
            // Prefill with 500 Ore
            asteroidInv.AddResource(ResourceType.Ore, 500);

            // 2. Setup Player Ship
            var shipObj = new GameObject("PlayerShip");
            var shipController = shipObj.AddComponent<ShipController>();
            var shipAbilityComp = shipObj.AddComponent<AbilitySystemComponent>();
            var shipInventory = shipObj.AddComponent<ResourceInventory>();
            
            // Give ship some construction components for building
            shipInventory.AddResource(ResourceType.Components, 50);

            // Move ship to be close to the asteroid
            shipObj.transform.position = Vector3.zero;
            asteroidObj.transform.position = new Vector3(100f, 0f, 0f); // 100 meters away

            // 3. Spawn Mining Rig under construction next to the asteroid
            var rigObj = new GameObject("AsteroidMiner_Test");
            rigObj.transform.position = new Vector3(80f, 0f, 0f); // 20m from asteroid, 80m from ship (both in range)
            
            var rigAbilityComp = rigObj.AddComponent<AbilitySystemComponent>();
            var rigInventory = rigObj.AddComponent<NetworkInventory>();
            rigInventory.MaxCapacity = 500;
            
            var rig = rigObj.AddComponent<AsteroidMiningRig>();
            rig.ExtractionAmount = 25;
            rig.ExtractionInterval = 1.0f;

            // Trigger Awake on rig to bind variables
            var awakeMethod = typeof(StructureBase).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (awakeMethod != null) awakeMethod.Invoke(rig, null);

            // Verify starts under construction
            Assert.IsTrue(rig.IsUnderConstruction.Value);
            Assert.AreEqual(0f, rig.ConstructionProgress.Value);

            // 4. Run construction ticks
            // In StructureBase, UpdateConstruction consumes Components from nearby ship and increments progress.
            var updateConstructionMethod = typeof(StructureBase).GetMethod("UpdateConstruction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(updateConstructionMethod);

            // Tick 1 (consumes 5 Components, progress goes to 5 / 200 = 2.5%)
            updateConstructionMethod.Invoke(rig, new object[] { 1.0f });
            Assert.AreEqual(45, shipInventory.GetAmount(ResourceType.Components));
            Assert.AreEqual(0.025f, rig.ConstructionProgress.Value);

            // Simulating further ticks to finish construction
            shipInventory.AddResource(ResourceType.Components, 200);
            for (int i = 0; i < 39; i++)
            {
                updateConstructionMethod.Invoke(rig, new object[] { 1.0f });
            }

            // Construction should now be complete
            Assert.IsFalse(rig.IsUnderConstruction.Value);
            Assert.AreEqual(1.0f, rig.ConstructionProgress.Value);

            // 5. Test Ore Extraction
            // FindNearestAsteroid needs to run to lock target
            var findAsteroidMethod = typeof(AsteroidMiningRig).GetMethod("FindNearestAsteroid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(findAsteroidMethod);
            findAsteroidMethod.Invoke(rig, null);

            // ExtractOre extracts 25 Ore from Asteroid (500 -> 475) and adds it to rig inventory (0 -> 25)
            var extractOreMethod = typeof(AsteroidMiningRig).GetMethod("ExtractOre", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(extractOreMethod);
            extractOreMethod.Invoke(rig, null);

            Assert.AreEqual(475, asteroidInv.GetAmount(ResourceType.Ore));
            Assert.AreEqual(25, rigInventory.GetAmount("Ore"));

            // 6. Test DB Saving/Persistence
            ResourceInventoryState invState = new ResourceInventoryState
            {
                Ore = rigInventory.GetAmount("Ore"),
                Gas = rigInventory.GetAmount("Gas"),
                Fuel = rigInventory.GetAmount("Fuel"),
                Munitions = rigInventory.GetAmount("Munitions"),
                Components = rigInventory.GetAmount("Components")
            };

            m_DbRepo.SaveStructure(
                rigObj.name,
                "SystemAlpha",
                "AsteroidMiner",
                rigObj.transform.position,
                rigObj.transform.rotation,
                100.0f,
                invState
            );

            // 7. Test DB Loading/Restoring
            var loadedList = m_DbRepo.LoadStructures("SystemAlpha");
            Assert.AreEqual(1, loadedList.Count);
            
            var loadedData = loadedList[0];
            Assert.AreEqual("AsteroidMiner_Test", loadedData.StructureId);
            Assert.AreEqual("AsteroidMiner", loadedData.Type);
            Assert.AreEqual(80f, loadedData.Position.x);
            Assert.AreEqual(25, loadedData.Inventory.Ore);

            // Cleanup scene objects
            Object.DestroyImmediate(asteroidObj);
            Object.DestroyImmediate(shipObj);
            Object.DestroyImmediate(rigObj);
        }

        [Test]
        public void Test_FactoryAndRefinery_Integration()
        {
            // 1. Setup Refinery
            var refineryObj = new GameObject("Refinery_Test");
            var refineryInv = refineryObj.AddComponent<NetworkInventory>();
            refineryInv.MaxCapacity = 500;

            var refinery = refineryObj.AddComponent<Refinery>();
            refinery.OreCost = 5;
            refinery.GasCost = 2;
            refinery.ComponentsYield = 1;
            refinery.FuelYield = 3;

            // Trigger Awake & finish construction
            var awakeMethod = typeof(StructureBase).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (awakeMethod != null) awakeMethod.Invoke(refinery, null);
            refinery.IsUnderConstruction.Value = false;
            refinery.IsDisabled.Value = false;

            // Add input materials
            refineryInv.AddResource("Ore", 10);
            refineryInv.AddResource("Gas", 5);

            // Execute processing recipe
            var processRefineryMethod = typeof(Refinery).GetMethod("ProcessRecipe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(processRefineryMethod);
            processRefineryMethod.Invoke(refinery, null);

            // Verify refinery consumption and yields
            Assert.AreEqual(5, refineryInv.GetAmount("Ore"));
            Assert.AreEqual(3, refineryInv.GetAmount("Gas"));
            Assert.AreEqual(1, refineryInv.GetAmount("Components"));
            Assert.AreEqual(3, refineryInv.GetAmount("Fuel"));

            // 2. Setup Munitions Factory
            var factoryObj = new GameObject("Factory_Test");
            var factoryInv = factoryObj.AddComponent<NetworkInventory>();
            factoryInv.MaxCapacity = 500;

            var factory = factoryObj.AddComponent<MunitionsFactory>();
            factory.OreCost = 5;
            factory.ComponentsCost = 1;
            factory.MunitionsYield = 2;

            if (awakeMethod != null) awakeMethod.Invoke(factory, null);
            factory.IsUnderConstruction.Value = false;
            factory.IsDisabled.Value = false;

            // Add inputs
            factoryInv.AddResource("Ore", 5);
            factoryInv.AddResource("Components", 1);

            // Process factory recipe
            var processFactoryMethod = typeof(MunitionsFactory).GetMethod("ProcessRecipe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(processFactoryMethod);
            processFactoryMethod.Invoke(factory, null);

            // Verify factory output
            Assert.AreEqual(0, factoryInv.GetAmount("Ore"));
            Assert.AreEqual(0, factoryInv.GetAmount("Components"));
            Assert.AreEqual(2, factoryInv.GetAmount("Munitions"));

            // 3. Test SQLite Persistence for multi-structures
            ResourceInventoryState refineryState = new ResourceInventoryState
            {
                Ore = refineryInv.GetAmount("Ore"),
                Gas = refineryInv.GetAmount("Gas"),
                Fuel = refineryInv.GetAmount("Fuel"),
                Munitions = refineryInv.GetAmount("Munitions"),
                Components = refineryInv.GetAmount("Components")
            };

            m_DbRepo.SaveStructure(
                refineryObj.name,
                "SystemAlpha",
                "Refinery",
                refineryObj.transform.position,
                refineryObj.transform.rotation,
                100.0f,
                refineryState
            );

            ResourceInventoryState factoryState = new ResourceInventoryState
            {
                Ore = factoryInv.GetAmount("Ore"),
                Gas = factoryInv.GetAmount("Gas"),
                Fuel = factoryInv.GetAmount("Fuel"),
                Munitions = factoryInv.GetAmount("Munitions"),
                Components = factoryInv.GetAmount("Components")
            };

            m_DbRepo.SaveStructure(
                factoryObj.name,
                "SystemAlpha",
                "MunitionsFactory",
                factoryObj.transform.position,
                factoryObj.transform.rotation,
                100.0f,
                factoryState
            );

            // 4. Test loading back
            var loadedList = m_DbRepo.LoadStructures("SystemAlpha");
            Assert.AreEqual(2, loadedList.Count);

            var loadedRefinery = loadedList.Find(x => x.StructureId == "Refinery_Test");
            Assert.IsNotNull(loadedRefinery);
            Assert.AreEqual("Refinery", loadedRefinery.Type);
            Assert.AreEqual(5, loadedRefinery.Inventory.Ore);
            Assert.AreEqual(3, loadedRefinery.Inventory.Fuel);

            var loadedFactory = loadedList.Find(x => x.StructureId == "Factory_Test");
            Assert.IsNotNull(loadedFactory);
            Assert.AreEqual("MunitionsFactory", loadedFactory.Type);
            Assert.AreEqual(2, loadedFactory.Inventory.Munitions);

            // Cleanup
            Object.DestroyImmediate(refineryObj);
            Object.DestroyImmediate(factoryObj);
        }
    }
}
