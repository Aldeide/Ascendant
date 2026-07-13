using NUnit.Framework;
using UnityEngine;
using Ascendant.SystemsExtensions.Celestial;
using Ascendant.SystemsExtensions.Logistics;

namespace Ascendant.Tests
{
    public class CelestialTests
    {
        private GameObject m_SystemAlphaObj;
        private GameObject m_SystemBetaObj;
        private GameObject m_ShipObj;

        [SetUp]
        public void SetUp()
        {
            m_SystemAlphaObj = new GameObject("SystemAlpha");
            m_SystemBetaObj = new GameObject("SystemBeta");
            m_ShipObj = new GameObject("TestMiningShip");
        }

        [TearDown]
        public void TearDown()
        {
            if (m_SystemAlphaObj != null) Object.DestroyImmediate(m_SystemAlphaObj);
            if (m_SystemBetaObj != null) Object.DestroyImmediate(m_SystemBetaObj);
            if (m_ShipObj != null) Object.DestroyImmediate(m_ShipObj);
        }

        [Test]
        public void Test_StarSystemSetup()
        {
            var alpha = m_SystemAlphaObj.AddComponent<StarSystem>();
            alpha.InitializeSystem("SystemAlpha");

            Assert.AreEqual("SystemAlpha", alpha.SystemName);
            Assert.IsNotNull(alpha.Star);
            Assert.AreEqual("Helios", alpha.Star.BodyName);
            Assert.AreEqual(CelestialType.Star, alpha.Star.Type);

            // Verifies Helios has 4 orbit bodies (planet, moon, gas planet, belt)
            Assert.AreEqual(4, alpha.Bodies.Count);

            var gaia = alpha.Bodies.Find(b => b.BodyName == "Gaia");
            Assert.IsNotNull(gaia);
            Assert.AreEqual(CelestialType.TerrestrialPlanet, gaia.Type);

            var luna = alpha.Bodies.Find(b => b.BodyName == "Luna");
            Assert.IsNotNull(luna);
            Assert.AreEqual(CelestialType.Moon, luna.Type);
            Assert.AreEqual(gaia, luna.ParentBody);

            var ares = alpha.Bodies.Find(b => b.BodyName == "Ares");
            Assert.IsNotNull(ares);
            Assert.AreEqual(CelestialType.GaseousPlanet, ares.Type);

            var primeBelt = alpha.Bodies.Find(b => b.BodyName == "Prime Belt");
            Assert.IsNotNull(primeBelt);
            Assert.AreEqual(CelestialType.AsteroidBelt, primeBelt.Type);

            // Verify SystemBeta Setup
            var beta = m_SystemBetaObj.AddComponent<StarSystem>();
            beta.InitializeSystem("SystemBeta");

            Assert.AreEqual("SystemBeta", beta.SystemName);
            Assert.AreEqual("Kepler", beta.Star.BodyName);
            Assert.AreEqual(4, beta.Bodies.Count);

            var keplerC = beta.Bodies.Find(b => b.BodyName == "Kepler-c");
            Assert.IsNotNull(keplerC);
            Assert.AreEqual(CelestialType.GaseousPlanet, keplerC.Type);
        }

        [Test]
        public void Test_AsteroidSpawning()
        {
            var alpha = m_SystemAlphaObj.AddComponent<StarSystem>();
            alpha.InitializeSystem("SystemAlpha");

            var beltObj = alpha.GetComponentInChildren<AsteroidBelt>();
            Assert.IsNotNull(beltObj);
            
            // Belt should have spawned 8 asteroids
            Assert.AreEqual(8, beltObj.SpawnedAsteroids.Count);
            
            var firstAsteroid = beltObj.SpawnedAsteroids[0];
            Assert.IsNotNull(firstAsteroid);
            
            var inventory = firstAsteroid.GetComponent<ResourceInventory>();
            Assert.IsNotNull(inventory);
            Assert.AreEqual(500, inventory.GetAmount(ResourceType.Ore));
        }

        [Test]
        public void Test_GasPlanetFuelScooping()
        {
            var alpha = m_SystemAlphaObj.AddComponent<StarSystem>();
            alpha.InitializeSystem("SystemAlpha");

            var ares = alpha.Bodies.Find(b => b.BodyName == "Ares");
            Assert.IsNotNull(ares);

            // Setup ship logistics scoop
            var shipInventory = m_ShipObj.AddComponent<ResourceInventory>();
            shipInventory.MaxCapacity = 100;
            var scoop = m_ShipObj.AddComponent<GaseousFuelScoop>();
            scoop.HarvestAmount = 10;
            
            var harvestMethod = typeof(GaseousFuelScoop).GetMethod("HarvestGas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Position ship within scooping radius of Ares gaseous planet
            m_ShipObj.transform.position = ares.transform.position + new Vector3(50f, 0f, 0f);

            // Execute scoop harvest tick
            float distance = Vector3.Distance(m_ShipObj.transform.position, ares.transform.position);
            Assert.LessOrEqual(distance, ares.Radius);

            harvestMethod.Invoke(scoop, null);
            Assert.AreEqual(10, shipInventory.GetAmount(ResourceType.Gas));
        }
    }
}
