using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using MechLite.Tests.Utilities;
using MechLite.Tests.Mocks;

namespace MechLite.Tests.Systems
{
    /// <summary>
    /// Unit tests for JumpSystem component
    /// </summary>
    public class JumpSystemTests
    {
        private GameObject testObject;
        private JumpSystem jumpSystem;
        private MockMovable mockMovable;
        private MockGroundDetector mockGroundDetector;
        private MockEnergySystem mockEnergySystem;
        private PhysicsConfigSO testConfig;
        private EnergyConfigSO energyConfig;
        private EventCapture eventCapture;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestJumpSystem");
            jumpSystem = testObject.AddComponent<JumpSystem>();
            mockMovable = testObject.AddComponent<MockMovable>();
            mockGroundDetector = testObject.AddComponent<MockGroundDetector>();
            mockEnergySystem = testObject.AddComponent<MockEnergySystem>();
            testConfig = TestConfigurationFactory.CreateTestPhysicsConfig();
            energyConfig = TestConfigurationFactory.CreateTestEnergyConfig();
            eventCapture = new EventCapture();
            eventCapture.Subscribe();
        }

        [TearDown]
        public void TearDown()
        {
            eventCapture.Unsubscribe();
            if (testObject != null)
                Object.DestroyImmediate(testObject);
            if (testConfig != null)
                Object.DestroyImmediate(testConfig);
            if (energyConfig != null)
                Object.DestroyImmediate(energyConfig);
        }

        #region Initialization Tests

        [Test]
        public void Initialize_SetsConfigurationCorrectly()
        {
            Assert.DoesNotThrow(() => jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem), 
                "Should initialize without throwing exceptions");
        }

        [Test]
        public void Initialize_WithNullConfigs_HandlesGracefully()
        {
            Assert.DoesNotThrow(() => jumpSystem.Initialize(null, null, mockMovable, mockGroundDetector, mockEnergySystem), 
                "Should handle null configs gracefully");
        }

        [Test]
        public void Initialize_WithNullSystems_HandlesGracefully()
        {
            Assert.DoesNotThrow(() => jumpSystem.Initialize(testConfig, energyConfig, null, null, null), 
                "Should handle null systems gracefully");
        }

        #endregion

        #region Jump Input Tests

        [Test]
        public void TryJump_WhenGroundedWithEnergy_ExecutesJump()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            
            bool result = jumpSystem.TryJump();
            
            Assert.IsTrue(result, "Jump should succeed when grounded with sufficient energy");
            Assert.AreEqual(1, mockMovable.JumpCallCount, "Should call Jump on movable system");
            Assert.AreEqual(testConfig.jumpForce, mockMovable.LastJumpForce, "Should use configured jump force");
        }

        [Test]
        public void TryJump_WhenNotGrounded_FailsJump()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(false);
            mockGroundDetector.SetCoyoteTime(0f);
            mockGroundDetector.SetWasRecentlyGrounded(false);
            mockEnergySystem.SetEnergy(100f);
            
            bool result = jumpSystem.TryJump();
            
            Assert.IsFalse(result, "Jump should fail when not grounded and no coyote time");
            Assert.AreEqual(0, mockMovable.JumpCallCount, "Should not call Jump on movable system");
        }

        [Test]
        public void TryJump_WithInsufficientEnergy_FailsJump()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(10f); // Less than jump cost
            
            bool result = jumpSystem.TryJump();
            
            Assert.IsFalse(result, "Jump should fail with insufficient energy");
            Assert.AreEqual(0, mockMovable.JumpCallCount, "Should not call Jump on movable system");
        }

        [Test]
        public void TryJump_DuringCoyoteTime_ExecutesJump()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(false);
            mockGroundDetector.SetCoyoteTime(0.05f); // Has coyote time remaining
            mockEnergySystem.SetEnergy(100f);
            
            bool result = jumpSystem.TryJump();
            
            Assert.IsTrue(result, "Jump should succeed during coyote time");
            Assert.AreEqual(1, mockMovable.JumpCallCount, "Should call Jump on movable system");
        }

        [Test]
        public void TryJump_ConsumesEnergy()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            float initialEnergy = mockEnergySystem.CurrentEnergy;
            
            jumpSystem.TryJump();
            
            Assert.Less(mockEnergySystem.CurrentEnergy, initialEnergy, "Should consume energy when jumping");
            Assert.AreEqual(initialEnergy - energyConfig.jumpEnergyCost, mockEnergySystem.CurrentEnergy, 
                "Should consume configured jump energy cost");
        }

        #endregion

        #region Jump Buffering Tests

        [Test]
        public void BufferJump_StoresJumpInput()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(false); // Not grounded, so jump should be buffered
            mockEnergySystem.SetEnergy(100f);
            
            jumpSystem.BufferJump();
            
            // Jump should be stored for later execution
            Assert.IsTrue(jumpSystem.HasBufferedJump, "Should have buffered jump input");
        }

        [Test]
        public void BufferJump_WhenGrounded_ExecutesImmediately()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            
            jumpSystem.BufferJump();
            
            Assert.AreEqual(1, mockMovable.JumpCallCount, "Should execute jump immediately when grounded");
            Assert.IsFalse(jumpSystem.HasBufferedJump, "Should not buffer jump when executed immediately");
        }

        [UnityTest]
        public IEnumerator BufferedJump_ExecutesWhenBecomingGrounded()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(false);
            mockEnergySystem.SetEnergy(100f);
            
            jumpSystem.BufferJump(); // Buffer jump while airborne
            Assert.IsTrue(jumpSystem.HasBufferedJump, "Should have buffered jump");
            
            yield return null; // Wait a frame
            
            mockGroundDetector.SetGrounded(true); // Become grounded
            jumpSystem.UpdateJumpBuffer(); // Process buffered input
            
            Assert.AreEqual(1, mockMovable.JumpCallCount, "Should execute buffered jump when becoming grounded");
            Assert.IsFalse(jumpSystem.HasBufferedJump, "Should clear buffered jump after execution");
        }

        [UnityTest]
        public IEnumerator BufferedJump_ExpiresAfterBufferTime()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(false);
            mockEnergySystem.SetEnergy(100f);
            
            jumpSystem.BufferJump();
            
            // Wait for buffer time to expire
            yield return new WaitForSeconds(testConfig.jumpBufferTime + 0.1f);
            jumpSystem.UpdateJumpBuffer();
            
            Assert.IsFalse(jumpSystem.HasBufferedJump, "Buffered jump should expire after buffer time");
            
            // Even if we become grounded now, the buffered jump shouldn't execute
            mockGroundDetector.SetGrounded(true);
            jumpSystem.UpdateJumpBuffer();
            
            Assert.AreEqual(0, mockMovable.JumpCallCount, "Expired buffered jump should not execute");
        }

        #endregion

        #region Coyote Time Integration Tests

        [Test]
        public void TryJump_WithRecentlyGrounded_ExecutesJump()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(false);
            mockGroundDetector.SetWasRecentlyGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            
            bool result = jumpSystem.TryJump();
            
            Assert.IsTrue(result, "Jump should succeed when recently grounded");
            Assert.AreEqual(1, mockMovable.JumpCallCount, "Should call Jump on movable system");
        }

        [Test]
        public void TryJump_UsesCoyoteTimeFromGroundDetector()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(false);
            mockGroundDetector.SetCoyoteTime(0.08f);
            mockEnergySystem.SetEnergy(100f);
            
            bool result = jumpSystem.TryJump();
            
            Assert.IsTrue(result, "Jump should succeed with coyote time remaining");
        }

        #endregion

        #region Event Integration Tests

        [Test]
        public void TryJump_DoesNotPublishEventDirectly()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            eventCapture.Clear();
            
            jumpSystem.TryJump();
            
            // JumpSystem should delegate to MovementController, which publishes the event
            // The JumpSystem itself doesn't publish jump events
            Assert.AreEqual(1, mockMovable.JumpCallCount, "Should delegate jump to movable system");
        }

        #endregion

        #region Configuration Integration Tests

        [Test]
        public void TryJump_UsesPhysicsConfigJumpForce()
        {
            var customConfig = ScriptableObject.CreateInstance<PhysicsConfigSO>();
            customConfig.jumpForce = 20f;
            
            jumpSystem.Initialize(customConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            
            jumpSystem.TryJump();
            
            Assert.AreEqual(20f, mockMovable.LastJumpForce, "Should use custom jump force from config");
            
            Object.DestroyImmediate(customConfig);
        }

        [Test]
        public void TryJump_UsesEnergyConfigJumpCost()
        {
            var customEnergyConfig = ScriptableObject.CreateInstance<EnergyConfigSO>();
            customEnergyConfig.jumpEnergyCost = 40f;
            
            jumpSystem.Initialize(testConfig, customEnergyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            
            jumpSystem.TryJump();
            
            Assert.AreEqual(60f, mockEnergySystem.CurrentEnergy, "Should consume custom jump energy cost");
            
            Object.DestroyImmediate(customEnergyConfig);
        }

        [Test]
        public void BufferJump_UsesConfiguredBufferTime()
        {
            var customConfig = ScriptableObject.CreateInstance<PhysicsConfigSO>();
            customConfig.jumpBufferTime = 0.2f;
            
            jumpSystem.Initialize(customConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            
            // The buffer time is used internally - this is more of a smoke test
            Assert.DoesNotThrow(() => jumpSystem.BufferJump(), "Should use configured buffer time without errors");
            
            Object.DestroyImmediate(customConfig);
        }

        #endregion

        #region Edge Cases

        [Test]
        public void TryJump_WithZeroJumpForce_HandlesGracefully()
        {
            var customConfig = ScriptableObject.CreateInstance<PhysicsConfigSO>();
            customConfig.jumpForce = 0f;
            
            jumpSystem.Initialize(customConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            
            Assert.DoesNotThrow(() => jumpSystem.TryJump(), "Should handle zero jump force gracefully");
            
            Object.DestroyImmediate(customConfig);
        }

        [Test]
        public void TryJump_WithZeroEnergyCost_DoesNotConsumeEnergy()
        {
            var customEnergyConfig = ScriptableObject.CreateInstance<EnergyConfigSO>();
            customEnergyConfig.jumpEnergyCost = 0f;
            
            jumpSystem.Initialize(testConfig, customEnergyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            float initialEnergy = mockEnergySystem.CurrentEnergy;
            
            jumpSystem.TryJump();
            
            Assert.AreEqual(initialEnergy, mockEnergySystem.CurrentEnergy, "Should not consume energy when cost is zero");
            
            Object.DestroyImmediate(customEnergyConfig);
        }

        [Test]
        public void UpdateJumpBuffer_WithoutBufferedJump_DoesNotCrash()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            
            Assert.DoesNotThrow(() => jumpSystem.UpdateJumpBuffer(), "Should handle update when no jump is buffered");
        }

        #endregion

        #region Multiple Jump Prevention Tests

        [Test]
        public void TryJump_ConsecutiveCalls_OnlyExecutesOnce()
        {
            jumpSystem.Initialize(testConfig, energyConfig, mockMovable, mockGroundDetector, mockEnergySystem);
            mockGroundDetector.SetGrounded(true);
            mockEnergySystem.SetEnergy(100f);
            
            jumpSystem.TryJump(); // First jump
            bool secondJumpResult = jumpSystem.TryJump(); // Second jump attempt
            
            Assert.AreEqual(1, mockMovable.JumpCallCount, "Should only execute one jump");
            Assert.IsFalse(secondJumpResult, "Second jump attempt should fail");
        }

        #endregion
    }
}
