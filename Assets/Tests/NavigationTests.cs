using NUnit.Framework;
using UnityEngine;
using AbilitySystem.Scripts;
using AbilitySystem.Runtime.Core;
using AbilitySystem.Runtime.Abilities;
using Ascendant.SystemsExtensions.Logistics;

namespace Ascendant.Tests
{
    public class NavigationTests
    {
        private GameObject m_TestObject;
        private GameObject m_ManagerObject;

        [SetUp]
        public void SetUp()
        {
            m_TestObject = new GameObject("TestShip");
            m_ManagerObject = new GameObject("SystemConnectionManager");
            m_ManagerObject.AddComponent<SystemConnectionManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_TestObject != null) Object.DestroyImmediate(m_TestObject);
            if (m_ManagerObject != null) Object.DestroyImmediate(m_ManagerObject);
        }

        [Test]
        public void Test_DatabaseReadWrite()
        {
            // Use in-memory SQLite database for testing to ensure no file locks or persistence leaks
            using (var conn = DatabaseConnectionManager.CreateConnection("URI=file::memory:"))
            {
                var repo = new WorldDatabaseRepository(conn);
                repo.CreateTables();

                // Save system
                repo.SaveStarSystem("SystemAlpha", "LogisticsAlliance");

                // Save structure with resource state
                var pos = new Vector3(10f, -5f, 25f);
                var rot = Quaternion.Euler(0, 45, 0);
                var invState = new ResourceInventoryState
                {
                    Ore = 150,
                    Gas = 50,
                    Fuel = 200,
                    Munitions = 0,
                    Components = 5
                };

                repo.SaveStructure("Rig_A", "SystemAlpha", "AsteroidMiningRig", pos, rot, 85.5f, invState);

                // Load structure
                var list = repo.LoadStructures("SystemAlpha");
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("Rig_A", list[0].StructureId);
                Assert.AreEqual("AsteroidMiningRig", list[0].Type);
                Assert.AreEqual(pos.x, list[0].Position.x, 0.001f);
                Assert.AreEqual(rot.y, list[0].Rotation.y, 0.001f);
                Assert.AreEqual(85.5f, list[0].Health, 0.001f);
                Assert.AreEqual(150, list[0].Inventory.Ore);
                Assert.AreEqual(200, list[0].Inventory.Fuel);
                Assert.AreEqual(5, list[0].Inventory.Components);
            }
        }

        [Test]
        public void Test_JumpSequenceValidation_Success()
        {
            var asc = m_TestObject.AddComponent<AbilitySystemComponent>();
            asc.Initialise();

            var attributeSet = new ShipAttributeSet(asc.AbilitySystem);
            asc.AbilitySystem.AttributeSetManager.AddAttributeSet(typeof(ShipAttributeSet), attributeSet);

            // Set warp fuel to 100 (needs 20 for standard jump)
            attributeSet.WarpFuel.SetBaseValue(100f);

            var definition = ScriptableObject.CreateInstance<ShipJumpAbilityDefinition>();
            asc.AbilitySystem.AbilityManager.GrantAbility(definition);

            // Set up a transition listener on connection manager
            var connectionManager = SystemConnectionManager.Instance;
            bool transitioned = false;
            string targetSystem = "";
            connectionManager.OnShipTransitioned += (ship, dest) =>
            {
                transitioned = true;
                targetSystem = dest;
            };

            // Trigger jump ability with target coordinates (x=1 means SystemBeta)
            asc.AbilitySystem.AbilityManager.TryActivateAbility(definition.UniqueName, new AbilityData
            {
                TargetPosition = new Vector3(1f, 0, 0)
            });

            Assert.IsTrue(transitioned);
            Assert.AreEqual("SystemBeta", targetSystem);
            Assert.AreEqual(80f, attributeSet.WarpFuel.CurrentValue); // 100 - 20 = 80
        }

        [Test]
        public void Test_JumpSequenceValidation_Failure_InsufficientFuel()
        {
            var asc = m_TestObject.AddComponent<AbilitySystemComponent>();
            asc.Initialise();

            var attributeSet = new ShipAttributeSet(asc.AbilitySystem);
            asc.AbilitySystem.AttributeSetManager.AddAttributeSet(typeof(ShipAttributeSet), attributeSet);

            // Set warp fuel to 10f (insufficient, needs 20f)
            attributeSet.WarpFuel.SetBaseValue(10f);

            var definition = ScriptableObject.CreateInstance<ShipJumpAbilityDefinition>();
            asc.AbilitySystem.AbilityManager.GrantAbility(definition);

            var connectionManager = SystemConnectionManager.Instance;
            bool transitioned = false;
            connectionManager.OnShipTransitioned += (ship, dest) => { transitioned = true; };

            // Trigger jump ability
            asc.AbilitySystem.AbilityManager.TryActivateAbility(definition.UniqueName, new AbilityData
            {
                TargetPosition = new Vector3(1f, 0, 0)
            });

            Assert.IsFalse(transitioned);
            Assert.AreEqual(10f, attributeSet.WarpFuel.CurrentValue); // Fuel not consumed
        }
    }
}
