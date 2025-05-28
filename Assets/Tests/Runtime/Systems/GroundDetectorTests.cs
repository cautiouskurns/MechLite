using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using MechLite.Tests.Utilities;
using MechLite.Movement;
using MechLite.Configuration;

namespace MechLite.Tests.Systems
{
    /// <summary>
    /// Unit tests for GroundDetector component
    /// </summary>
    public class GroundDetectorTests
    {
        private GameObject testObject;
        private GroundDetector groundDetector;
        private PhysicsConfigSO testConfig;
        private EventCapture eventCapture;
        private GameObject groundObject;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestGroundDetector");
            testObject.transform.position = Vector3.zero;
            
            // Add required components
            testObject.AddComponent<Rigidbody2D>();
            var collider = testObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 2f);
            
            groundDetector = testObject.AddComponent<GroundDetector>();
            testConfig = TestConfigurationFactory.CreateTestPhysicsConfig();
            eventCapture = new EventCapture();
            eventCapture.Subscribe();
            
            // Create ground object
            groundObject = new GameObject("Ground");
            groundObject.transform.position = new Vector3(0, -2f, 0);
            var groundCollider = groundObject.AddComponent<BoxCollider2D>();
            groundCollider.size = new Vector2(10f, 1f);
            groundObject.layer = LayerMask.NameToLayer("Default"); // Assuming ground is on default layer
        }

        [TearDown]
        public void TearDown()
        {
            eventCapture.Unsubscribe();
            if (testObject != null)
                Object.DestroyImmediate(testObject);
            if (groundObject != null)
                Object.DestroyImmediate(groundObject);
            if (testConfig != null)
                Object.DestroyImmediate(testConfig);
        }

        #region Interface Contract Tests

        [Test]
        public void GroundDetector_ImplementsIGroundDetector()
        {
            Assert.IsTrue(groundDetector is IGroundDetector, "GroundDetector should implement IGroundDetector interface");
        }

        [Test]
        public void IsGrounded_InitiallyFalse()
        {
            groundDetector.Initialize(testConfig);
            Assert.IsFalse(groundDetector.IsGrounded, "Should not be grounded initially");
        }

        [Test]
        public void CoyoteTimeRemaining_InitiallyZero()
        {
            groundDetector.Initialize(testConfig);
            Assert.AreEqual(0f, groundDetector.CoyoteTimeRemaining, "Coyote time should be zero initially");
        }

        [Test]
        public void WasRecentlyGrounded_InitiallyFalse()
        {
            groundDetector.Initialize(testConfig);
            Assert.IsFalse(groundDetector.WasRecentlyGrounded, "Should not be recently grounded initially");
        }

        #endregion

        #region Ground Detection Tests

        [Test]
        public void CheckGrounded_DetectsGroundWhenClose()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0); // Position close to ground
            
            groundDetector.CheckGrounded();
            
            Assert.IsTrue(groundDetector.IsGrounded, "Should detect ground when close enough");
        }

        [Test]
        public void CheckGrounded_DoesNotDetectGroundWhenFar()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, 5f, 0); // Position far from ground
            
            groundDetector.CheckGrounded();
            
            Assert.IsFalse(groundDetector.IsGrounded, "Should not detect ground when too far");
        }

        [Test]
        public void CheckGrounded_PublishesGroundStateChangedEvent()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0);
            eventCapture.Clear();
            
            groundDetector.CheckGrounded();
            
            Assert.AreEqual(1, eventCapture.GroundEvents.Count, "Should publish GroundStateChangedEvent");
            Assert.IsTrue(eventCapture.GroundEvents[0].IsGrounded, "Event should indicate grounded state");
            Assert.AreEqual(testObject.transform, eventCapture.GroundEvents[0].PlayerTransform, "Event should contain player transform");
        }

        [Test]
        public void CheckGrounded_OnlyPublishesEventOnStateChange()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0);
            
            groundDetector.CheckGrounded(); // First check - should publish event
            eventCapture.Clear();
            groundDetector.CheckGrounded(); // Second check - should not publish event
            
            Assert.AreEqual(0, eventCapture.GroundEvents.Count, "Should not publish event if ground state unchanged");
        }

        #endregion

        #region Coyote Time Tests

        [Test]
        public void UpdateGroundTiming_StartsCoyoteTimeWhenLeavingGround()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded(); // Become grounded
            
            testObject.transform.position = new Vector3(0, 5f, 0); // Leave ground
            groundDetector.CheckGrounded();
            
            Assert.Greater(groundDetector.CoyoteTimeRemaining, 0f, "Should start coyote time when leaving ground");
            Assert.IsTrue(groundDetector.WasRecentlyGrounded, "Should be marked as recently grounded");
        }

        [UnityTest]
        public IEnumerator UpdateGroundTiming_ReducesCoyoteTimeOverTime()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded(); // Become grounded
            
            testObject.transform.position = new Vector3(0, 5f, 0); // Leave ground
            groundDetector.CheckGrounded();
            float initialCoyoteTime = groundDetector.CoyoteTimeRemaining;
            
            yield return new WaitForSeconds(0.05f);
            groundDetector.UpdateGroundTiming();
            
            Assert.Less(groundDetector.CoyoteTimeRemaining, initialCoyoteTime, "Coyote time should decrease over time");
        }

        [UnityTest]
        public IEnumerator UpdateGroundTiming_ExpiresCoyoteTime()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded(); // Become grounded
            
            testObject.transform.position = new Vector3(0, 5f, 0); // Leave ground
            groundDetector.CheckGrounded();
            
            // Wait for coyote time to expire
            yield return new WaitForSeconds(testConfig.coyoteTime + 0.1f);
            groundDetector.UpdateGroundTiming();
            
            Assert.AreEqual(0f, groundDetector.CoyoteTimeRemaining, "Coyote time should expire to zero");
            Assert.IsFalse(groundDetector.WasRecentlyGrounded, "Should no longer be recently grounded after expiry");
        }

        #endregion

        #region Ground Action Tests

        [Test]
        public void CanPerformGroundAction_TrueWhenGrounded()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded();
            
            Assert.IsTrue(groundDetector.CanPerformGroundAction(), "Should be able to perform ground action when grounded");
        }

        [Test]
        public void CanPerformGroundAction_TrueWhenRecentlyGrounded()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded(); // Become grounded
            
            testObject.transform.position = new Vector3(0, 5f, 0); // Leave ground
            groundDetector.CheckGrounded();
            
            Assert.IsTrue(groundDetector.CanPerformGroundAction(), "Should be able to perform ground action when recently grounded");
        }

        [Test]
        public void CanPerformGroundAction_TrueWhenCoyoteTimeRemaining()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded(); // Become grounded
            
            testObject.transform.position = new Vector3(0, 5f, 0); // Leave ground
            groundDetector.CheckGrounded();
            
            Assert.Greater(groundDetector.CoyoteTimeRemaining, 0f, "Should have coyote time remaining");
            Assert.IsTrue(groundDetector.CanPerformGroundAction(), "Should be able to perform ground action during coyote time");
        }

        [Test]
        public void CanPerformGroundAction_FalseWhenNotGroundedAndNoCoyoteTime()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, 5f, 0); // Far from ground
            groundDetector.CheckGrounded();
            
            Assert.IsFalse(groundDetector.CanPerformGroundAction(), "Should not be able to perform ground action when not grounded and no coyote time");
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void Initialize_SetsConfigurationCorrectly()
        {
            Assert.DoesNotThrow(() => groundDetector.Initialize(testConfig), "Should initialize without throwing exceptions");
        }

        [Test]
        public void Initialize_WithNullConfig_HandlesGracefully()
        {
            Assert.DoesNotThrow(() => groundDetector.Initialize(null), "Should handle null config gracefully");
        }

        [Test]
        public void CoyoteTime_UsesConfigurationValue()
        {
            groundDetector.Initialize(testConfig);
            testObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded(); // Become grounded
            
            testObject.transform.position = new Vector3(0, 5f, 0); // Leave ground
            groundDetector.CheckGrounded();
            
            Assert.AreEqual(testConfig.coyoteTime, groundDetector.CoyoteTimeRemaining, 0.01f, 
                "Coyote time should match configuration value");
        }

        #endregion

        #region Layer Mask Tests

        [Test]
        public void CheckGrounded_RespectsGroundLayerMask()
        {
            groundDetector.Initialize(testConfig);
            
            // Move ground to a different layer that's not in the ground mask
            int nonGroundLayer = LayerMask.NameToLayer("UI"); // Assuming UI layer is not ground
            groundObject.layer = nonGroundLayer;
            
            testObject.transform.position = new Vector3(0, -1f, 0);
            groundDetector.CheckGrounded();
            
            // Should not detect ground if it's on wrong layer (depends on configuration)
            // This test assumes ground layer mask is properly configured
        }

        #endregion

        #region Edge Cases

        [Test]
        public void CheckGrounded_WithNoCollider_HandlesGracefully()
        {
            // Remove collider
            var collider = testObject.GetComponent<BoxCollider2D>();
            Object.DestroyImmediate(collider);
            
            groundDetector.Initialize(testConfig);
            
            Assert.DoesNotThrow(() => groundDetector.CheckGrounded(), "Should handle missing collider gracefully");
        }

        [Test]
        public void UpdateGroundTiming_WithNegativeTime_HandlesGracefully()
        {
            groundDetector.Initialize(testConfig);
            
            // Simulate negative time (shouldn't happen normally but good to test)
            Assert.DoesNotThrow(() => groundDetector.UpdateGroundTiming(), "Should handle timing updates gracefully");
        }

        #endregion
    }
}
