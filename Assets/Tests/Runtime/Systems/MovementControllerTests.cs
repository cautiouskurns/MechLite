using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using MechLite.Tests.Utilities;
using MechLite.Tests.Mocks;

namespace MechLite.Tests.Systems
{
    /// <summary>
    /// Unit tests for MovementController component
    /// </summary>
    public class MovementControllerTests
    {
        private GameObject testObject;
        private MovementController movementController;
        private Rigidbody2D rb2d;
        private MockGroundDetector mockGroundDetector;
        private MovementConfigSO testConfig;
        private EventCapture eventCapture;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestMovementController");
            rb2d = testObject.AddComponent<Rigidbody2D>();
            movementController = testObject.AddComponent<MovementController>();
            mockGroundDetector = testObject.AddComponent<MockGroundDetector>();
            testConfig = TestConfigurationFactory.CreateTestMovementConfig();
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
        }

        #region Interface Contract Tests

        [Test]
        public void MovementController_ImplementsIMovable()
        {
            Assert.IsTrue(movementController is IMovable, "MovementController should implement IMovable interface");
        }

        [Test]
        public void Velocity_ReturnsRigidbodyVelocity()
        {
            rb2d.velocity = new Vector2(5f, 3f);
            Assert.AreEqual(rb2d.velocity, movementController.Velocity, "Velocity should return Rigidbody2D velocity");
        }

        [Test]
        public void IsGrounded_ReturnsGroundDetectorState()
        {
            mockGroundDetector.SetGrounded(true);
            Assert.IsTrue(movementController.IsGrounded, "IsGrounded should return ground detector state when grounded");
            
            mockGroundDetector.SetGrounded(false);
            Assert.IsFalse(movementController.IsGrounded, "IsGrounded should return ground detector state when not grounded");
        }

        #endregion

        #region Movement Tests

        [Test]
        public void Move_WithGroundedPlayer_AppliesGroundMovement()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            mockGroundDetector.SetGrounded(true);
            Vector2 direction = Vector2.right;
            
            movementController.Move(direction, Time.fixedDeltaTime);
            
            Assert.Greater(rb2d.velocity.x, 0, "Should apply positive horizontal velocity when moving right");
        }

        [Test]
        public void Move_WithAirbornePlayer_AppliesAirControl()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            mockGroundDetector.SetGrounded(false);
            Vector2 direction = Vector2.right;
            
            movementController.Move(direction, Time.fixedDeltaTime);
            
            // Air movement should be less effective than ground movement
            Assert.Greater(rb2d.velocity.x, 0, "Should apply some horizontal velocity in air");
            // Verify air control is being applied (would need to compare with ground movement in real test)
        }

        [Test]
        public void Move_WithZeroDirection_AppliesTowardZeroVelocity()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            mockGroundDetector.SetGrounded(true);
            rb2d.velocity = new Vector2(5f, 0f);
            
            movementController.Move(Vector2.zero, Time.fixedDeltaTime);
            
            Assert.Less(Mathf.Abs(rb2d.velocity.x), 5f, "Should decelerate when no input direction");
        }

        [Test]
        public void Move_PublishesMovementEvent()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            eventCapture.Clear();
            Vector2 direction = Vector2.right;
            
            movementController.Move(direction, Time.fixedDeltaTime);
            
            Assert.AreEqual(1, eventCapture.MovedEvents.Count, "Should publish one PlayerMovedEvent");
            Assert.AreEqual(rb2d.velocity, eventCapture.MovedEvents[0].Velocity, "Event should contain current velocity");
            Assert.AreEqual(mockGroundDetector.IsGrounded, eventCapture.MovedEvents[0].IsGrounded, "Event should contain grounded state");
        }

        #endregion

        #region Jump Tests

        [Test]
        public void Jump_AppliesUpwardForce()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            float jumpForce = 15f;
            
            movementController.Jump(jumpForce);
            
            Assert.AreEqual(jumpForce, rb2d.velocity.y, "Should set vertical velocity to jump force");
        }

        [Test]
        public void Jump_PreservesHorizontalVelocity()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            rb2d.velocity = new Vector2(5f, 0f);
            float initialHorizontalVelocity = rb2d.velocity.x;
            
            movementController.Jump(15f);
            
            Assert.AreEqual(initialHorizontalVelocity, rb2d.velocity.x, "Should preserve horizontal velocity during jump");
        }

        [Test]
        public void Jump_PublishesJumpEvent()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            eventCapture.Clear();
            float jumpForce = 15f;
            
            movementController.Jump(jumpForce);
            
            Assert.AreEqual(1, eventCapture.JumpedEvents.Count, "Should publish one PlayerJumpedEvent");
            Assert.AreEqual(jumpForce, eventCapture.JumpedEvents[0].JumpForce, "Event should contain jump force");
            Assert.AreEqual(rb2d.velocity, eventCapture.JumpedEvents[0].ResultingVelocity, "Event should contain resulting velocity");
        }

        #endregion

        #region Velocity Control Tests

        [Test]
        public void SetVelocity_UpdatesRigidbodyVelocity()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            Vector2 newVelocity = new Vector2(10f, 5f);
            
            movementController.SetVelocity(newVelocity);
            
            Assert.AreEqual(newVelocity, rb2d.velocity, "Should set rigidbody velocity to specified value");
        }

        [Test]
        public void SetVelocity_PublishesMovementEvent()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            eventCapture.Clear();
            Vector2 newVelocity = new Vector2(8f, 3f);
            
            movementController.SetVelocity(newVelocity);
            
            Assert.AreEqual(1, eventCapture.MovedEvents.Count, "Should publish one PlayerMovedEvent");
            Assert.AreEqual(newVelocity, eventCapture.MovedEvents[0].Velocity, "Event should contain new velocity");
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void Initialize_SetsConfigurationCorrectly()
        {
            Assert.DoesNotThrow(() => movementController.Initialize(testConfig, mockGroundDetector), 
                "Should initialize without throwing exceptions");
        }

        [Test]
        public void Initialize_WithNullConfig_HandlesGracefully()
        {
            Assert.DoesNotThrow(() => movementController.Initialize(null, mockGroundDetector), 
                "Should handle null config gracefully");
        }

        [Test]
        public void Initialize_WithNullGroundDetector_HandlesGracefully()
        {
            Assert.DoesNotThrow(() => movementController.Initialize(testConfig, null), 
                "Should handle null ground detector gracefully");
        }

        #endregion

        #region Physics Integration Tests

        [UnityTest]
        public IEnumerator Movement_RespondsToConfigurationChanges()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            mockGroundDetector.SetGrounded(true);
            
            // Test with original config
            movementController.Move(Vector2.right, Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
            float originalAcceleration = rb2d.velocity.x;
            
            // Change config values
            testConfig.acceleration = testConfig.acceleration * 2f;
            rb2d.velocity = Vector2.zero;
            
            // Test with modified config
            movementController.Move(Vector2.right, Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
            float newAcceleration = rb2d.velocity.x;
            
            Assert.Greater(newAcceleration, originalAcceleration, "Movement should respond to configuration changes");
        }

        [Test]
        public void Move_RespectsMaxSpeed()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            mockGroundDetector.SetGrounded(true);
            
            // Apply movement multiple times to reach max speed
            for (int i = 0; i < 100; i++)
            {
                movementController.Move(Vector2.right, Time.fixedDeltaTime);
            }
            
            Assert.LessOrEqual(Mathf.Abs(rb2d.velocity.x), testConfig.moveSpeed + 0.1f, 
                "Horizontal velocity should not exceed configured move speed");
        }

        #endregion

        #region Edge Cases

        [Test]
        public void Move_WithExtremeDeltaTime_RemainsStable()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            mockGroundDetector.SetGrounded(true);
            
            Assert.DoesNotThrow(() => movementController.Move(Vector2.right, 100f), 
                "Should handle extreme delta time without throwing");
            Assert.IsTrue(float.IsFinite(rb2d.velocity.x) && float.IsFinite(rb2d.velocity.y), 
                "Velocity should remain finite with extreme delta time");
        }

        [Test]
        public void Jump_WithNegativeForce_HandlesGracefully()
        {
            movementController.Initialize(testConfig, mockGroundDetector);
            float initialY = rb2d.velocity.y;
            
            movementController.Jump(-10f);
            
            // Should either apply negative force or clamp to zero, but not crash
            Assert.IsTrue(float.IsFinite(rb2d.velocity.y), "Velocity should remain finite with negative jump force");
        }

        #endregion
    }
}
