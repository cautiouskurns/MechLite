using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using MechLite.Tests.Utilities;
using MechLite.Tests.Mocks;
using MechLite.Configuration;
using MechLite.Movement;
using MechLite.Energy;
using MechLite.Events;

namespace MechLite.Tests.Systems
{
    /// <summary>
    /// Unit tests for DashSystem component
    /// </summary>
    public class DashSystemTests
    {
        private GameObject testObject;
        private DashSystem dashSystem;
        private Rigidbody2D rb2d;
        private MockEnergySystem mockEnergySystem;
        private DashConfigSO testConfig;
        private EventCapture eventCapture;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestDashSystem");
            rb2d = testObject.AddComponent<Rigidbody2D>();
            dashSystem = testObject.AddComponent<DashSystem>();
            mockEnergySystem = testObject.AddComponent<MockEnergySystem>();
            testConfig = TestConfigurationFactory.CreateTestDashConfig();
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
        public void DashSystem_ImplementsIDashable()
        {
            Assert.IsTrue(dashSystem is IDashable, "DashSystem should implement IDashable interface");
        }

        [Test]
        public void CanDash_InitiallyTrue()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            Assert.IsTrue(dashSystem.CanDash, "Should be able to dash initially");
        }

        [Test]
        public void DashCooldownRemaining_InitiallyZero()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            Assert.AreEqual(0f, dashSystem.DashCooldownRemaining, "Dash cooldown should be zero initially");
        }

        #endregion

        #region Dash Execution Tests

        [Test]
        public void Dash_WithSufficientEnergy_ExecutesDash()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            Vector2 dashDirection = Vector2.right;
            
            bool result = dashSystem.Dash(dashDirection);
            
            Assert.IsTrue(result, "Dash should succeed with sufficient energy");
            Assert.Greater(rb2d.velocity.magnitude, 0f, "Should apply velocity during dash");
        }

        [Test]
        public void Dash_WithInsufficientEnergy_FailsDash()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(10f); // Less than dash cost
            Vector2 dashDirection = Vector2.right;
            
            bool result = dashSystem.Dash(dashDirection);
            
            Assert.IsFalse(result, "Dash should fail with insufficient energy");
        }

        [Test]
        public void Dash_ConsumesEnergy()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            float initialEnergy = mockEnergySystem.CurrentEnergy;
            
            dashSystem.Dash(Vector2.right);
            
            Assert.Less(mockEnergySystem.CurrentEnergy, initialEnergy, "Should consume energy when dashing");
            Assert.AreEqual(initialEnergy - testConfig.energyCost, mockEnergySystem.CurrentEnergy, 
                "Should consume exactly the configured energy cost");
        }

        [Test]
        public void Dash_AppliesCorrectForce()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            Vector2 dashDirection = Vector2.right;
            
            dashSystem.Dash(dashDirection);
            
            Vector2 expectedVelocity = dashDirection.normalized * testConfig.dashForce;
            Assert.AreEqual(expectedVelocity.x, rb2d.velocity.x, 0.1f, "Should apply correct horizontal dash force");
            Assert.AreEqual(expectedVelocity.y, rb2d.velocity.y, 0.1f, "Should apply correct vertical dash force");
        }

        [Test]
        public void Dash_WithDiagonalDirection_NormalizesCorrectly()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            Vector2 dashDirection = new Vector2(1f, 1f); // Diagonal
            
            dashSystem.Dash(dashDirection);
            
            float expectedMagnitude = testConfig.dashForce;
            Assert.AreEqual(expectedMagnitude, rb2d.velocity.magnitude, 0.1f, 
                "Dash magnitude should be consistent regardless of input direction magnitude");
        }

        [Test]
        public void Dash_StartsCooldown()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            
            dashSystem.Dash(Vector2.right);
            
            Assert.IsFalse(dashSystem.CanDash, "Should not be able to dash immediately after dashing");
            Assert.Greater(dashSystem.DashCooldownRemaining, 0f, "Should have cooldown remaining after dash");
            Assert.AreEqual(testConfig.dashCooldown, dashSystem.DashCooldownRemaining, 0.1f, 
                "Cooldown should match configuration");
        }

        [Test]
        public void Dash_PublishesDashEvent()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            eventCapture.Clear();
            Vector2 dashDirection = Vector2.right;
            
            dashSystem.Dash(dashDirection);
            
            Assert.AreEqual(1, eventCapture.DashedEvents.Count, "Should publish one PlayerDashedEvent");
            Assert.AreEqual(dashDirection.normalized, eventCapture.DashedEvents[0].Direction, "Event should contain dash direction");
            Assert.AreEqual(testConfig.dashForce, eventCapture.DashedEvents[0].Force, "Event should contain dash force");
            Assert.AreEqual(rb2d.velocity, eventCapture.DashedEvents[0].ResultingVelocity, "Event should contain resulting velocity");
        }

        #endregion

        #region Cooldown Tests

        [Test]
        public void Dash_DuringCooldown_Fails()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            
            dashSystem.Dash(Vector2.right); // First dash
            bool secondDashResult = dashSystem.Dash(Vector2.left); // Second dash during cooldown
            
            Assert.IsFalse(secondDashResult, "Should not be able to dash during cooldown");
        }

        [UnityTest]
        public IEnumerator UpdateDashCooldown_ReducesCooldownOverTime()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            
            dashSystem.Dash(Vector2.right);
            float initialCooldown = dashSystem.DashCooldownRemaining;
            
            yield return new WaitForSeconds(0.1f);
            dashSystem.UpdateDashCooldown();
            
            Assert.Less(dashSystem.DashCooldownRemaining, initialCooldown, "Cooldown should decrease over time");
        }

        [UnityTest]
        public IEnumerator UpdateDashCooldown_RestoresCanDashAfterCooldown()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            
            dashSystem.Dash(Vector2.right);
            
            // Wait for cooldown to expire
            yield return new WaitForSeconds(testConfig.dashCooldown + 0.1f);
            dashSystem.UpdateDashCooldown();
            
            Assert.IsTrue(dashSystem.CanDash, "Should be able to dash again after cooldown expires");
            Assert.AreEqual(0f, dashSystem.DashCooldownRemaining, "Cooldown should be zero after expiry");
        }

        [Test]
        public void UpdateDashCooldown_DoesNotGoNegative()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            
            // Call update cooldown when no cooldown is active
            dashSystem.UpdateDashCooldown();
            
            Assert.AreEqual(0f, dashSystem.DashCooldownRemaining, "Cooldown should not go negative");
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void Initialize_SetsConfigurationCorrectly()
        {
            Assert.DoesNotThrow(() => dashSystem.Initialize(testConfig, mockEnergySystem), 
                "Should initialize without throwing exceptions");
        }

        [Test]
        public void Initialize_WithNullConfig_HandlesGracefully()
        {
            Assert.DoesNotThrow(() => dashSystem.Initialize(null, mockEnergySystem), 
                "Should handle null config gracefully");
        }

        [Test]
        public void Initialize_WithNullEnergySystem_HandlesGracefully()
        {
            Assert.DoesNotThrow(() => dashSystem.Initialize(testConfig, null), 
                "Should handle null energy system gracefully");
        }

        [Test]
        public void Dash_RespectsConfigurationValues()
        {
            // Test with custom config values
            var customConfig = ScriptableObject.CreateInstance<DashConfigSO>();
            customConfig.dashForce = 25f;
            customConfig.energyCost = 75f;
            customConfig.dashCooldown = 2f;
            
            dashSystem.Initialize(customConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            
            dashSystem.Dash(Vector2.right);
            
            Assert.AreEqual(25f, rb2d.velocity.magnitude, 0.1f, "Should use configured dash force");
            Assert.AreEqual(25f, mockEnergySystem.CurrentEnergy, "Should use configured energy cost");
            Assert.AreEqual(2f, dashSystem.DashCooldownRemaining, 0.1f, "Should use configured cooldown");
            
            Object.DestroyImmediate(customConfig);
        }

        #endregion

        #region Direction Tests

        [Test]
        public void Dash_WithZeroDirection_HandlesGracefully()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            
            bool result = dashSystem.Dash(Vector2.zero);
            
            // Should either fail gracefully or use a default direction
            Assert.IsTrue(float.IsFinite(rb2d.velocity.x) && float.IsFinite(rb2d.velocity.y), 
                "Velocity should remain finite with zero direction");
        }

        [Test]
        public void Dash_PreservesDirectionality()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            
            // Test all cardinal directions
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            
            foreach (var direction in directions)
            {
                rb2d.velocity = Vector2.zero;
                mockEnergySystem.SetEnergy(100f);
                dashSystem.CanDash = true; // Reset dash state for testing
                
                dashSystem.Dash(direction);
                
                Vector2 expectedDirection = direction.normalized;
                Vector2 actualDirection = rb2d.velocity.normalized;
                
                Assert.AreEqual(expectedDirection.x, actualDirection.x, 0.1f, 
                    $"Should preserve X direction for {direction}");
                Assert.AreEqual(expectedDirection.y, actualDirection.y, 0.1f, 
                    $"Should preserve Y direction for {direction}");
            }
        }

        #endregion

        #region Integration Tests

        [UnityTest]
        public IEnumerator DashSystem_IntegratesWithEnergyRegeneration()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(testConfig.energyCost); // Just enough for one dash
            
            dashSystem.Dash(Vector2.right); // Consume all energy
            Assert.IsFalse(dashSystem.Dash(Vector2.left), "Should fail second dash due to insufficient energy");
            
            // Restore energy
            mockEnergySystem.SetEnergy(100f);
            yield return new WaitForSeconds(testConfig.dashCooldown + 0.1f); // Wait for cooldown
            dashSystem.UpdateDashCooldown();
            
            Assert.IsTrue(dashSystem.Dash(Vector2.left), "Should succeed after energy restoration and cooldown");
        }

        #endregion

        #region Edge Cases

        [Test]
        public void Dash_WithExtremeDirection_RemainsStable()
        {
            dashSystem.Initialize(testConfig, mockEnergySystem);
            mockEnergySystem.SetEnergy(100f);
            Vector2 extremeDirection = new Vector2(1000f, 1000f);
            
            dashSystem.Dash(extremeDirection);
            
            Assert.IsTrue(float.IsFinite(rb2d.velocity.x) && float.IsFinite(rb2d.velocity.y), 
                "Velocity should remain finite with extreme direction values");
            Assert.AreEqual(testConfig.dashForce, rb2d.velocity.magnitude, 0.1f, 
                "Should normalize extreme directions to configured force");
        }

        #endregion
    }
}
