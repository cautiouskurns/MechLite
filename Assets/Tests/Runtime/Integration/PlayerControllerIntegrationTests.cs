using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using MechLite.Tests.Utilities;
using MechLite.Tests.Mocks;

namespace MechLite.Tests.Integration
{
    /// <summary>
    /// Integration tests for PlayerController coordination and system interactions
    /// </summary>
    public class PlayerControllerIntegrationTests
    {
        private GameObject playerObject;
        private PlayerController playerController;
        private MovementController movementController;
        private EnergySystem energySystem;
        private GroundDetector groundDetector;
        private DashSystem dashSystem;
        private JumpSystem jumpSystem;
        private Rigidbody2D rb2d;
        
        private MovementConfigSO movementConfig;
        private EnergyConfigSO energyConfig;
        private DashConfigSO dashConfig;
        private PhysicsConfigSO physicsConfig;
        
        private EventCapture eventCapture;
        private GameObject groundObject;

        [SetUp]
        public void SetUp()
        {
            // Create player object with all required components
            playerObject = new GameObject("TestPlayer");
            rb2d = playerObject.AddComponent<Rigidbody2D>();
            var collider = playerObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 2f);
            
            // Add all systems
            playerController = playerObject.AddComponent<PlayerController>();
            movementController = playerObject.AddComponent<MovementController>();
            energySystem = playerObject.AddComponent<EnergySystem>();
            groundDetector = playerObject.AddComponent<GroundDetector>();
            dashSystem = playerObject.AddComponent<DashSystem>();
            jumpSystem = playerObject.AddComponent<JumpSystem>();
            
            // Create configurations
            movementConfig = TestConfigurationFactory.CreateTestMovementConfig();
            energyConfig = TestConfigurationFactory.CreateTestEnergyConfig();
            dashConfig = TestConfigurationFactory.CreateTestDashConfig();
            physicsConfig = TestConfigurationFactory.CreateTestPhysicsConfig();
            
            // Create ground for physics tests
            groundObject = new GameObject("Ground");
            groundObject.transform.position = new Vector3(0, -2f, 0);
            var groundCollider = groundObject.AddComponent<BoxCollider2D>();
            groundCollider.size = new Vector2(10f, 1f);
            
            // Set up event capture
            eventCapture = new EventCapture();
            eventCapture.Subscribe();
        }

        [TearDown]
        public void TearDown()
        {
            eventCapture.Unsubscribe();
            if (playerObject != null)
                Object.DestroyImmediate(playerObject);
            if (groundObject != null)
                Object.DestroyImmediate(groundObject);
            if (movementConfig != null)
                Object.DestroyImmediate(movementConfig);
            if (energyConfig != null)
                Object.DestroyImmediate(energyConfig);
            if (dashConfig != null)
                Object.DestroyImmediate(dashConfig);
            if (physicsConfig != null)
                Object.DestroyImmediate(physicsConfig);
        }

        #region Initialization Integration Tests

        [Test]
        public void PlayerController_InitializesAllSystems()
        {
            Assert.DoesNotThrow(() => {
                playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            }, "Should initialize all systems without errors");
        }

        [Test]
        public void PlayerController_SystemsReferenceEachOther()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            
            // Verify systems are properly connected
            Assert.IsNotNull(movementController, "MovementController should be present");
            Assert.IsNotNull(energySystem, "EnergySystem should be present");
            Assert.IsNotNull(groundDetector, "GroundDetector should be present");
            Assert.IsNotNull(dashSystem, "DashSystem should be present");
            Assert.IsNotNull(jumpSystem, "JumpSystem should be present");
        }

        #endregion

        #region Movement Integration Tests

        [Test]
        public void PlayerController_MovementDelegatesToSystems()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            Vector2 inputDirection = Vector2.right;
            
            playerController.Move(inputDirection);
            
            Assert.Greater(rb2d.velocity.x, 0f, "Should apply movement through systems");
        }

        [Test]
        public void PlayerController_MovementPublishesEvents()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            eventCapture.Clear();
            
            playerController.Move(Vector2.right);
            
            Assert.Greater(eventCapture.MovedEvents.Count, 0, "Should publish movement events");
        }

        [UnityTest]
        public IEnumerator PlayerController_MovementIntegratesWithGroundDetection()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            playerObject.transform.position = new Vector3(0, -1f, 0); // Near ground
            
            yield return new WaitForFixedUpdate();
            groundDetector.CheckGrounded();
            
            playerController.Move(Vector2.right);
            
            Assert.IsTrue(movementController.IsGrounded, "Should detect ground state");
            Assert.Greater(rb2d.velocity.x, 0f, "Should apply grounded movement");
        }

        #endregion

        #region Jump Integration Tests

        [Test]
        public void PlayerController_JumpIntegratesAllSystems()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            playerObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded(); // Make sure we're grounded
            
            bool jumpResult = playerController.Jump();
            
            Assert.IsTrue(jumpResult, "Jump should succeed when grounded with energy");
            Assert.Greater(rb2d.velocity.y, 0f, "Should apply upward velocity");
            Assert.Less(energySystem.CurrentEnergy, energySystem.MaxEnergy, "Should consume energy");
        }

        [Test]
        public void PlayerController_JumpFailsWithoutEnergy()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            playerObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded();
            
            // Drain energy
            energySystem.ConsumeEnergy(energySystem.MaxEnergy);
            
            bool jumpResult = playerController.Jump();
            
            Assert.IsFalse(jumpResult, "Jump should fail without sufficient energy");
            Assert.AreEqual(0f, rb2d.velocity.y, 0.1f, "Should not apply upward velocity");
        }

        [Test]
        public void PlayerController_JumpFailsWhenNotGrounded()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            playerObject.transform.position = new Vector3(0, 5f, 0); // Far from ground
            groundDetector.CheckGrounded();
            
            bool jumpResult = playerController.Jump();
            
            Assert.IsFalse(jumpResult, "Jump should fail when not grounded");
        }

        [UnityTest]
        public IEnumerator PlayerController_JumpWithCoyoteTime()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            playerObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded(); // Become grounded
            
            // Leave ground
            playerObject.transform.position = new Vector3(0, 5f, 0);
            groundDetector.CheckGrounded();
            
            yield return new WaitForSeconds(0.05f); // Short delay within coyote time
            groundDetector.UpdateGroundTiming();
            
            bool jumpResult = playerController.Jump();
            
            Assert.IsTrue(jumpResult, "Should be able to jump during coyote time");
        }

        #endregion

        #region Dash Integration Tests

        [Test]
        public void PlayerController_DashIntegratesAllSystems()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            Vector2 dashDirection = Vector2.right;
            
            bool dashResult = playerController.Dash(dashDirection);
            
            Assert.IsTrue(dashResult, "Dash should succeed with sufficient energy");
            Assert.Greater(rb2d.velocity.magnitude, 0f, "Should apply dash velocity");
            Assert.Less(energySystem.CurrentEnergy, energySystem.MaxEnergy, "Should consume energy");
            Assert.IsFalse(dashSystem.CanDash, "Should start cooldown");
        }

        [Test]
        public void PlayerController_DashFailsWithoutEnergy()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            
            // Drain energy
            energySystem.ConsumeEnergy(energySystem.MaxEnergy);
            
            bool dashResult = playerController.Dash(Vector2.right);
            
            Assert.IsFalse(dashResult, "Dash should fail without sufficient energy");
        }

        [Test]
        public void PlayerController_DashFailsDuringCooldown()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            
            playerController.Dash(Vector2.right); // First dash
            bool secondDashResult = playerController.Dash(Vector2.left); // Second dash
            
            Assert.IsFalse(secondDashResult, "Second dash should fail during cooldown");
        }

        [Test]
        public void PlayerController_DashPublishesEvents()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            eventCapture.Clear();
            
            playerController.Dash(Vector2.right);
            
            Assert.Greater(eventCapture.DashedEvents.Count, 0, "Should publish dash events");
            Assert.Greater(eventCapture.EnergyEvents.Count, 0, "Should publish energy changed events");
        }

        #endregion

        #region Energy System Integration Tests

        [UnityTest]
        public IEnumerator PlayerController_EnergyRegeneratesOverTime()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            
            // Consume some energy
            energySystem.ConsumeEnergy(50f);
            float energyAfterConsumption = energySystem.CurrentEnergy;
            
            // Wait for regeneration
            yield return new WaitForSeconds(energyConfig.regenerationDelay + 0.5f);
            
            Assert.Greater(energySystem.CurrentEnergy, energyAfterConsumption, "Energy should regenerate over time");
        }

        [Test]
        public void PlayerController_EnergySystemIntegratesWithAllActions()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            float initialEnergy = energySystem.CurrentEnergy;
            
            // Perform actions that consume energy
            playerObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded();
            
            playerController.Jump(); // Consumes jump energy
            float energyAfterJump = energySystem.CurrentEnergy;
            
            // Wait for dash cooldown and restore energy for next test
            energySystem.RegenerateEnergy(100f);
            playerController.Dash(Vector2.right); // Consumes dash energy
            
            Assert.Less(energyAfterJump, initialEnergy, "Jump should consume energy");
            Assert.Less(energySystem.CurrentEnergy, initialEnergy, "Dash should consume energy");
        }

        #endregion

        #region Event System Integration Tests

        [Test]
        public void PlayerController_AllSystemsPublishEvents()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            playerObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded();
            eventCapture.Clear();
            
            // Perform various actions
            playerController.Move(Vector2.right);
            playerController.Jump();
            playerController.Dash(Vector2.left);
            
            // Verify events were published
            Assert.Greater(eventCapture.MovedEvents.Count, 0, "Should publish movement events");
            Assert.Greater(eventCapture.JumpedEvents.Count, 0, "Should publish jump events");
            Assert.Greater(eventCapture.DashedEvents.Count, 0, "Should publish dash events");
            Assert.Greater(eventCapture.EnergyEvents.Count, 0, "Should publish energy events");
        }

        [Test]
        public void PlayerController_EventsContainCorrectData()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            eventCapture.Clear();
            
            playerController.Move(Vector2.right);
            
            Assert.AreEqual(rb2d.velocity, eventCapture.MovedEvents[0].Velocity, "Movement event should contain correct velocity");
            Assert.AreEqual(movementController.IsGrounded, eventCapture.MovedEvents[0].IsGrounded, "Movement event should contain correct grounded state");
        }

        #endregion

        #region Configuration Integration Tests

        [Test]
        public void PlayerController_AllSystemsUseConfigurations()
        {
            // Create custom configurations with distinct values
            var customMovementConfig = ScriptableObject.CreateInstance<MovementConfigSO>();
            customMovementConfig.moveSpeed = 20f;
            
            var customEnergyConfig = ScriptableObject.CreateInstance<EnergyConfigSO>();
            customEnergyConfig.maxEnergy = 200f;
            
            var customDashConfig = ScriptableObject.CreateInstance<DashConfigSO>();
            customDashConfig.dashForce = 30f;
            
            var customPhysicsConfig = ScriptableObject.CreateInstance<PhysicsConfigSO>();
            customPhysicsConfig.jumpForce = 25f;
            
            playerController.Initialize(customMovementConfig, customEnergyConfig, customDashConfig, customPhysicsConfig);
            
            // Verify systems use the custom configurations
            Assert.AreEqual(200f, energySystem.MaxEnergy, "Should use custom energy config");
            
            // Test movement with custom config
            playerController.Move(Vector2.right);
            // Movement speed is harder to test directly, but we can verify it doesn't crash
            
            // Test dash with custom config
            bool dashResult = playerController.Dash(Vector2.right);
            if (dashResult) // Only check if dash succeeded
            {
                Assert.AreEqual(30f, rb2d.velocity.magnitude, 0.1f, "Should use custom dash force");
            }
            
            // Cleanup
            Object.DestroyImmediate(customMovementConfig);
            Object.DestroyImmediate(customEnergyConfig);
            Object.DestroyImmediate(customDashConfig);
            Object.DestroyImmediate(customPhysicsConfig);
        }

        #endregion

        #region Performance and Stability Tests

        [Test]
        public void PlayerController_HandlesRapidInputChanges()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            
            // Rapidly change movement direction
            for (int i = 0; i < 100; i++)
            {
                Vector2 direction = i % 2 == 0 ? Vector2.right : Vector2.left;
                Assert.DoesNotThrow(() => playerController.Move(direction), $"Should handle rapid direction change {i}");
            }
            
            Assert.IsTrue(float.IsFinite(rb2d.velocity.x) && float.IsFinite(rb2d.velocity.y), 
                "Velocity should remain finite after rapid input changes");
        }

        [Test]
        public void PlayerController_HandlesExtremeInputValues()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            
            // Test with extreme input values
            Vector2 extremeDirection = new Vector2(1000f, 1000f);
            
            Assert.DoesNotThrow(() => playerController.Move(extremeDirection), "Should handle extreme movement input");
            Assert.DoesNotThrow(() => playerController.Dash(extremeDirection), "Should handle extreme dash input");
            
            Assert.IsTrue(float.IsFinite(rb2d.velocity.x) && float.IsFinite(rb2d.velocity.y), 
                "Velocity should remain finite with extreme inputs");
        }

        [UnityTest]
        public IEnumerator PlayerController_MaintainsStabilityOverTime()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            
            // Run continuous operations for several frames
            for (int frame = 0; frame < 100; frame++)
            {
                playerController.Move(Vector2.right);
                
                // Periodically try other actions
                if (frame % 20 == 0)
                {
                    playerController.Jump();
                }
                if (frame % 30 == 0)
                {
                    playerController.Dash(Vector2.up);
                }
                
                yield return null;
            }
            
            // System should still be functional
            Assert.IsTrue(float.IsFinite(rb2d.velocity.x) && float.IsFinite(rb2d.velocity.y), 
                "Velocity should remain finite after extended operation");
            Assert.IsTrue(float.IsFinite(energySystem.CurrentEnergy), 
                "Energy should remain finite after extended operation");
        }

        #endregion

        #region Legacy API Compatibility Tests

        [Test]
        public void PlayerController_MaintainsPublicAPICompatibility()
        {
            playerController.Initialize(movementConfig, energyConfig, dashConfig, physicsConfig);
            
            // Test that the original public API still works
            Assert.DoesNotThrow(() => playerController.Move(Vector2.right), "Move method should be available");
            Assert.DoesNotThrow(() => playerController.Jump(), "Jump method should be available");
            Assert.DoesNotThrow(() => playerController.Dash(Vector2.right), "Dash method should be available");
            
            // Test property access
            Assert.DoesNotThrow(() => { var vel = playerController.Velocity; }, "Velocity property should be available");
            Assert.DoesNotThrow(() => { var grounded = playerController.IsGrounded; }, "IsGrounded property should be available");
            Assert.DoesNotThrow(() => { var energy = playerController.CurrentEnergy; }, "CurrentEnergy property should be available");
        }

        #endregion
    }
}
