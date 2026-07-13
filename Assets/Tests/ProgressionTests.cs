using NUnit.Framework;
using UnityEngine;
using AbilitySystem.Scripts;
using AbilitySystem.Runtime.Core;
using Ascendant.SystemsExtensions.Progression;
using Ascendant.SystemsExtensions.Logistics;

namespace Ascendant.Tests
{
    public class ProgressionTests
    {
        private GameObject m_TestObject;

        [SetUp]
        public void SetUp()
        {
            m_TestObject = new GameObject("FactionTechManager");
            m_TestObject.AddComponent<FactionTechTree>();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_TestObject != null) Object.DestroyImmediate(m_TestObject);
        }

        [Test]
        public void Test_FactionTechInvestment()
        {
            var tree = FactionTechTree.Instance;

            // Create some TechNode assets in memory
            var basicAlloys = ScriptableObject.CreateInstance<TechNode>();
            basicAlloys.TechName = "BasicAlloys";
            basicAlloys.CostInTechParts = 10;
            basicAlloys.IsUnlocked = false;

            var heavyHull = ScriptableObject.CreateInstance<TechNode>();
            heavyHull.TechName = "HeavyHull";
            heavyHull.CostInTechParts = 20;
            heavyHull.IsUnlocked = false;
            heavyHull.Prerequisites.Add(basicAlloys);

            // Test prerequisite check (cannot unlock HeavyHull before BasicAlloys)
            Assert.IsFalse(tree.InvestTechParts(heavyHull, 5));

            // Invest in BasicAlloys (5 parts)
            Assert.IsFalse(tree.InvestTechParts(basicAlloys, 5));
            Assert.AreEqual(5, tree.GetInvestment("BasicAlloys"));
            Assert.IsFalse(tree.IsTechUnlocked("BasicAlloys"));

            // Invest in BasicAlloys (5 more parts -> unlocks)
            Assert.IsTrue(tree.InvestTechParts(basicAlloys, 5));
            Assert.IsTrue(tree.IsTechUnlocked("BasicAlloys"));

            // Now, we can invest in HeavyHull
            Assert.IsFalse(tree.InvestTechParts(heavyHull, 10));
            Assert.IsTrue(tree.InvestTechParts(heavyHull, 10));
            Assert.IsTrue(tree.IsTechUnlocked("HeavyHull"));
        }

        [Test]
        public void Test_PerkAppliedToShip()
        {
            var ship = new GameObject("TestShipPlayer");
            var asc = ship.AddComponent<AbilitySystemComponent>();
            asc.Initialise();

            var attributeSet = new ShipAttributeSet(asc.AbilitySystem);
            asc.AbilitySystem.AttributeSetManager.AddAttributeSet(typeof(ShipAttributeSet), attributeSet);

            var specTree = ship.AddComponent<PlayerSpecializationTree>();

            // Initial checks
            Assert.AreEqual(1000f, attributeSet.CargoCapacity.CurrentValue);
            Assert.AreEqual(1.0f, attributeSet.MiningSpeed.CurrentValue);

            // Apply logistics spec (should double cargo capacity)
            specTree.ApplyLogisticsSpecialization();
            Assert.AreEqual(2000f, attributeSet.CargoCapacity.CurrentValue);

            // Apply mining spec (should triple mining speed)
            specTree.ApplyMiningSpecialization();
            Assert.AreEqual(3.0f, attributeSet.MiningSpeed.CurrentValue);

            Object.DestroyImmediate(ship);
        }
    }
}
