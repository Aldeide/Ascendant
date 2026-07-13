using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Ascendant.SystemsExtensions.Movement;

namespace Ascendant.Tests
{
    public class ShipJobMovementTests
    {
        private GameObject m_TestObject;
        private TransformAccessArray m_TransformAccessArray;

        [SetUp]
        public void SetUp()
        {
            m_TestObject = new GameObject("TestShip");
            m_TransformAccessArray = new TransformAccessArray(new Transform[] { m_TestObject.transform });
        }

        [TearDown]
        public void TearDown()
        {
            if (m_TransformAccessArray.isCreated)
            {
                m_TransformAccessArray.Dispose();
            }
            if (m_TestObject != null)
            {
                Object.DestroyImmediate(m_TestObject);
            }
        }

        [Test]
        public void MovementJob_MovesTowardsTarget()
        {
            // Arrange
            var inputs = new NativeArray<ShipInput>(1, Allocator.TempJob);
            var stats = new NativeArray<ShipStats>(1, Allocator.TempJob);

            m_TestObject.transform.position = Vector3.zero;
            m_TestObject.transform.rotation = Quaternion.identity;

            inputs[0] = new ShipInput
            {
                TargetPosition = new Vector3(0, 0, 10f),
                HasTarget = true
            };

            stats[0] = new ShipStats
            {
                MoveSpeed = 10f,
                TurnSpeed = 90f
            };

            var job = new ShipMovementJob
            {
                Inputs = inputs,
                Stats = stats,
                DeltaTime = 0.5f
            };

            // Act
            var handle = job.Schedule(m_TransformAccessArray);
            handle.Complete();

            // Assert
            // Movespeed is 10, DeltaTime is 0.5, so it should advance by exactly 5 units towards the target along Z
            Vector3 expectedPos = new Vector3(0, 0, 5f);
            Assert.AreEqual(expectedPos.x, m_TestObject.transform.position.x, 0.001f);
            Assert.AreEqual(expectedPos.y, m_TestObject.transform.position.y, 0.001f);
            Assert.AreEqual(expectedPos.z, m_TestObject.transform.position.z, 0.001f);
            Assert.IsTrue(inputs[0].HasTarget);

            // Clean up NativeArrays
            inputs.Dispose();
            stats.Dispose();
        }

        [Test]
        public void MovementJob_StopsAtTarget()
        {
            // Arrange
            var inputs = new NativeArray<ShipInput>(1, Allocator.TempJob);
            var stats = new NativeArray<ShipStats>(1, Allocator.TempJob);

            // Ship is already close to target (within 0.2f tolerance)
            m_TestObject.transform.position = new Vector3(0, 0, 9.9f);
            m_TestObject.transform.rotation = Quaternion.identity;

            inputs[0] = new ShipInput
            {
                TargetPosition = new Vector3(0, 0, 10f),
                HasTarget = true
            };

            stats[0] = new ShipStats
            {
                MoveSpeed = 10f,
                TurnSpeed = 90f
            };

            var job = new ShipMovementJob
            {
                Inputs = inputs,
                Stats = stats,
                DeltaTime = 0.5f
            };

            // Act
            var handle = job.Schedule(m_TransformAccessArray);
            handle.Complete();

            // Assert
            // It should stop moving and set HasTarget = false
            Assert.IsFalse(inputs[0].HasTarget);
            Assert.AreEqual(9.9f, m_TestObject.transform.position.z, 0.001f);

            // Clean up NativeArrays
            inputs.Dispose();
            stats.Dispose();
        }

        [Test]
        public void MovementJob_TurnsTowardsTarget()
        {
            // Arrange
            var inputs = new NativeArray<ShipInput>(1, Allocator.TempJob);
            var stats = new NativeArray<ShipStats>(1, Allocator.TempJob);

            m_TestObject.transform.position = Vector3.zero;
            // Face forward (Z+)
            m_TestObject.transform.rotation = Quaternion.identity;

            // Target is to the right (X+)
            inputs[0] = new ShipInput
            {
                TargetPosition = new Vector3(10f, 0, 0),
                HasTarget = true
            };

            stats[0] = new ShipStats
            {
                MoveSpeed = 10f,
                TurnSpeed = 90f // 90 degrees/sec
            };

            var job = new ShipMovementJob
            {
                Inputs = inputs,
                Stats = stats,
                DeltaTime = 0.5f // Should rotate by 45 degrees
            };

            // Act
            var handle = job.Schedule(m_TransformAccessArray);
            handle.Complete();

            // Assert
            // Ship should turn 45 degrees towards the right (Yaw rotation around Y axis)
            Quaternion expectedRot = Quaternion.Euler(0, 45f, 0);
            Assert.AreEqual(expectedRot.x, m_TestObject.transform.rotation.x, 0.001f);
            Assert.AreEqual(expectedRot.y, m_TestObject.transform.rotation.y, 0.001f);
            Assert.AreEqual(expectedRot.z, m_TestObject.transform.rotation.z, 0.001f);
            Assert.AreEqual(expectedRot.w, m_TestObject.transform.rotation.w, 0.001f);

            // Clean up NativeArrays
            inputs.Dispose();
            stats.Dispose();
        }
    }
}
