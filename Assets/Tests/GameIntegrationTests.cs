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
    }
}
