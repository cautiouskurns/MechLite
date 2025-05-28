using NUnit.Framework;
using UnityEngine;
using MechLite.Tests.Utilities;
using MechLite.Configuration;

namespace MechLite.Tests.Editor
{
    /// <summary>
    /// Editor tests for ScriptableObject configuration validation
    /// </summary>
    public class ConfigurationValidationTests
    {
        #region MovementConfigSO Tests

        [Test]
        public void MovementConfigSO_CreateInstance()
        {
            var config = ScriptableObject.CreateInstance<MovementConfigSO>();
            Assert.IsNotNull(config, "Should be able to create MovementConfigSO instance");
            Object.DestroyImmediate(config);
        }

        [Test]
        public void MovementConfigSO_DefaultValues()
        {
            var config = TestConfigurationFactory.CreateTestMovementConfig();
            
            Assert.Greater(config.moveSpeed, 0f, "Move speed should be positive");
            Assert.Greater(config.acceleration, 0f, "Acceleration should be positive");
            Assert.Greater(config.deceleration, 0f, "Deceleration should be positive");
            Assert.Greater(config.jumpForce, 0f, "Jump force should be positive");
            Assert.Greater(config.coyoteTime, 0f, "Coyote time should be positive");
            Assert.GreaterOrEqual(config.airControlStrength, 0f, "Air control should be non-negative");
            Assert.LessOrEqual(config.airControlStrength, 1f, "Air control should not exceed 1.0");
            
            Object.DestroyImmediate(config);
        }

        [Test]
        public void MovementConfigSO_ValidatesRanges()
        {
            var config = ScriptableObject.CreateInstance<MovementConfigSO>();
            
            // Test setting valid values
            config.moveSpeed = 10f;
            config.acceleration = 50f;
            config.deceleration = 30f;
            config.airControlStrength = 0.5f;
            
            Assert.AreEqual(10f, config.moveSpeed, "Should accept valid move speed");
            Assert.AreEqual(50f, config.acceleration, "Should accept valid acceleration");
            Assert.AreEqual(30f, config.deceleration, "Should accept valid deceleration");
            Assert.AreEqual(0.5f, config.airControlStrength, "Should accept valid air control");
            
            Object.DestroyImmediate(config);
        }

        #endregion

        #region EnergyConfigSO Tests

        [Test]
        public void EnergyConfigSO_CreateInstance()
        {
            var config = ScriptableObject.CreateInstance<EnergyConfigSO>();
            Assert.IsNotNull(config, "Should be able to create EnergyConfigSO instance");
            Object.DestroyImmediate(config);
        }

        [Test]
        public void EnergyConfigSO_DefaultValues()
        {
            var config = TestConfigurationFactory.CreateTestEnergyConfig();
            
            Assert.Greater(config.maxEnergy, 0f, "Max energy should be positive");
            Assert.GreaterOrEqual(config.energyRegenRate, 0f, "Regeneration rate should be non-negative");
            Assert.GreaterOrEqual(config.regenDelay, 0f, "Regeneration delay should be non-negative");
            Assert.GreaterOrEqual(config.dashEnergyCost, 0f, "Dash energy cost should be non-negative");
            
            Object.DestroyImmediate(config);
        }

        [Test]
        public void EnergyConfigSO_ValidatesEnergyConstraints()
        {
            var config = TestConfigurationFactory.CreateTestEnergyConfig();
            
            Assert.LessOrEqual(config.dashEnergyCost, config.maxEnergy, 
                "Dash energy cost should not exceed max energy");
            
            Object.DestroyImmediate(config);
        }

        #endregion

        #region DashConfigSO Tests

        [Test]
        public void DashConfigSO_CreateInstance()
        {
            var config = ScriptableObject.CreateInstance<DashConfigSO>();
            Assert.IsNotNull(config, "Should be able to create DashConfigSO instance");
            Object.DestroyImmediate(config);
        }

        [Test]
        public void DashConfigSO_DefaultValues()
        {
            var config = TestConfigurationFactory.CreateTestDashConfig();
            
            Assert.Greater(config.dashForce, 0f, "Dash force should be positive");
            Assert.GreaterOrEqual(config.dashCooldown, 0f, "Dash cooldown should be non-negative");
            
            Object.DestroyImmediate(config);
        }

        #endregion

        #region PhysicsConfigSO Tests

        [Test]
        public void PhysicsConfigSO_CreateInstance()
        {
            var config = ScriptableObject.CreateInstance<PhysicsConfigSO>();
            Assert.IsNotNull(config, "Should be able to create PhysicsConfigSO instance");
            Object.DestroyImmediate(config);
        }

        [Test]
        public void PhysicsConfigSO_DefaultValues()
        {
            var config = TestConfigurationFactory.CreateTestPhysicsConfig();
            
            Assert.Greater(config.groundCheckDistance, 0f, "Ground check distance should be positive");
            Assert.Greater(config.groundCheckRadius, 0f, "Ground check radius should be positive");
            
            Object.DestroyImmediate(config);
        }

        #endregion

        #region Configuration Persistence Tests

        [Test]
        public void ConfigurationSO_PersistsModifications()
        {
            var energyConfig = TestConfigurationFactory.CreateTestEnergyConfig();
            var originalMaxEnergy = energyConfig.maxEnergy;
            
            // Modify values and verify they persist
            energyConfig.maxEnergy = 150f;
            energyConfig.energyRegenRate = 25f;
            
            Assert.AreEqual(150f, energyConfig.maxEnergy, "Modified max energy should persist");
            Assert.AreEqual(25f, energyConfig.energyRegenRate, "Modified regeneration rate should persist");
            
            Object.DestroyImmediate(energyConfig);
        }

        #endregion
    }
}

        #region DashConfigSO Tests

        [Test]
        public void DashConfigSO_CreateInstance()
        {
            var config = ScriptableObject.CreateInstance<DashConfigSO>();
            Assert.IsNotNull(config, "Should be able to create DashConfigSO instance");
            Object.DestroyImmediate(config);
        }

        [Test]
        public void DashConfigSO_DefaultValues()
        {
            var config = TestConfigurationFactory.CreateTestDashConfig();
            
            Assert.Greater(config.dashForce, 0f, "Dash force should be positive");
            Assert.Greater(config.dashDuration, 0f, "Dash duration should be positive");
            Assert.GreaterOrEqual(config.dashCooldown, 0f, "Dash cooldown should be non-negative");
            Assert.GreaterOrEqual(config.energyCost, 0f, "Energy cost should be non-negative");
            
            Object.DestroyImmediate(config);
        }

        [Test]
        public void DashConfigSO_ValidatesTimingConstraints()
        {
            var config = TestConfigurationFactory.CreateTestDashConfig();
            
            Assert.Greater(config.dashDuration, 0f, "Dash duration must be positive for physics integration");
            Assert.LessOrEqual(config.dashDuration, 1f, "Dash duration should be reasonable (<=1 second)");
            
            Object.DestroyImmediate(config);
        }

        #endregion

        #region PhysicsConfigSO Tests

        [Test]
        public void PhysicsConfigSO_CreateInstance()
        {
            var config = ScriptableObject.CreateInstance<PhysicsConfigSO>();
            Assert.IsNotNull(config, "Should be able to create PhysicsConfigSO instance");
            Object.DestroyImmediate(config);
        }

        [Test]
        public void PhysicsConfigSO_DefaultValues()
        {
            var config = TestConfigurationFactory.CreateTestPhysicsConfig();
            
            Assert.Greater(config.jumpForce, 0f, "Jump force should be positive");
            Assert.Greater(config.gravityScale, 0f, "Gravity scale should be positive");
            Assert.GreaterOrEqual(config.coyoteTime, 0f, "Coyote time should be non-negative");
            Assert.GreaterOrEqual(config.jumpBufferTime, 0f, "Jump buffer time should be non-negative");
            Assert.Greater(config.maxFallSpeed, 0f, "Max fall speed should be positive");
            
            Object.DestroyImmediate(config);
        }

        [Test]
        public void PhysicsConfigSO_ValidatesPhysicsConstraints()
        {
            var config = TestConfigurationFactory.CreateTestPhysicsConfig();
            
            Assert.LessOrEqual(config.coyoteTime, 0.5f, "Coyote time should be reasonable (<=0.5 seconds)");
            Assert.LessOrEqual(config.jumpBufferTime, 0.5f, "Jump buffer time should be reasonable (<=0.5 seconds)");
            Assert.Greater(config.gravityScale, 0f, "Gravity scale must be positive for proper physics");
            
            Object.DestroyImmediate(config);
        }

        #endregion

        #region Cross-Configuration Validation Tests

        [Test]
        public void CrossConfiguration_EnergyAndActionCosts()
        {
            var energyConfig = TestConfigurationFactory.CreateTestEnergyConfig();
            var dashConfig = TestConfigurationFactory.CreateTestDashConfig();
            
            // Verify dash energy cost is consistent between configs
            Assert.AreEqual(energyConfig.dashEnergyCost, dashConfig.energyCost, 
                "Dash energy cost should be consistent between EnergyConfig and DashConfig");
            
            // Verify costs allow for multiple actions
            float totalCostForBasicCombo = energyConfig.jumpEnergyCost + energyConfig.dashEnergyCost;
            Assert.LessOrEqual(totalCostForBasicCombo, energyConfig.maxEnergy, 
                "Should be able to perform jump+dash combo within max energy");
            
            Object.DestroyImmediate(energyConfig);
            Object.DestroyImmediate(dashConfig);
        }

        [Test]
        public void CrossConfiguration_MovementAndPhysicsAlignment()
        {
            var movementConfig = TestConfigurationFactory.CreateTestMovementConfig();
            var physicsConfig = TestConfigurationFactory.CreateTestPhysicsConfig();
            
            // Verify movement and physics values are balanced
            Assert.Greater(physicsConfig.jumpForce, movementConfig.moveSpeed * 0.5f, 
                "Jump force should be significant relative to move speed");
            Assert.Less(physicsConfig.jumpForce, movementConfig.moveSpeed * 3f, 
                "Jump force should not be excessive relative to move speed");
            
            Object.DestroyImmediate(movementConfig);
            Object.DestroyImmediate(physicsConfig);
        }

        [Test]
        public void CrossConfiguration_TimingConsistency()
        {
            var physicsConfig = TestConfigurationFactory.CreateTestPhysicsConfig();
            var dashConfig = TestConfigurationFactory.CreateTestDashConfig();
            var energyConfig = TestConfigurationFactory.CreateTestEnergyConfig();
            
            // Verify timing values are reasonable relative to each other
            Assert.GreaterOrEqual(dashConfig.dashCooldown, dashConfig.dashDuration, 
                "Dash cooldown should be at least as long as dash duration");
            
            Assert.LessOrEqual(physicsConfig.coyoteTime, dashConfig.dashCooldown, 
                "Coyote time should be shorter than dash cooldown for balanced gameplay");
            
            Assert.LessOrEqual(physicsConfig.jumpBufferTime, energyConfig.regenerationDelay, 
                "Jump buffer should be shorter than energy regeneration delay");
            
            Object.DestroyImmediate(physicsConfig);
            Object.DestroyImmediate(dashConfig);
            Object.DestroyImmediate(energyConfig);
        }

        #endregion

        #region Configuration Modification Tests

        [Test]
        public void Configuration_AllowsRuntimeModification()
        {
            var movementConfig = TestConfigurationFactory.CreateTestMovementConfig();
            float originalMoveSpeed = movementConfig.moveSpeed;
            
            movementConfig.moveSpeed = originalMoveSpeed * 2f;
            
            Assert.AreEqual(originalMoveSpeed * 2f, movementConfig.moveSpeed, 
                "Should allow runtime modification of configuration values");
            
            Object.DestroyImmediate(movementConfig);
        }

        [Test]
        public void Configuration_MaintainsDataIntegrity()
        {
            var energyConfig = TestConfigurationFactory.CreateTestEnergyConfig();
            
            // Modify values and verify they persist
            energyConfig.maxEnergy = 150f;
            energyConfig.regenerationRate = 25f;
            
            Assert.AreEqual(150f, energyConfig.maxEnergy, "Modified max energy should persist");
            Assert.AreEqual(25f, energyConfig.regenerationRate, "Modified regeneration rate should persist");
            
            Object.DestroyImmediate(energyConfig);
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void Configuration_HandlesExtremeValues()
        {
            var config = ScriptableObject.CreateInstance<MovementConfigSO>();
            
            // Test with extreme values (should not crash)
            Assert.DoesNotThrow(() => config.moveSpeed = float.MaxValue, "Should handle extreme move speed");
            Assert.DoesNotThrow(() => config.acceleration = 0f, "Should handle zero acceleration");
            Assert.DoesNotThrow(() => config.airControl = -1f, "Should handle negative air control");
            
            // Values should be stored even if they're extreme
            Assert.AreEqual(float.MaxValue, config.moveSpeed, "Should store extreme values");
            Assert.AreEqual(0f, config.acceleration, "Should store zero values");
            Assert.AreEqual(-1f, config.airControl, "Should store negative values");
            
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Configuration_HandlesNaNAndInfinity()
        {
            var config = ScriptableObject.CreateInstance<EnergyConfigSO>();
            
            // Test with NaN and infinity
            config.maxEnergy = float.NaN;
            config.regenerationRate = float.PositiveInfinity;
            config.regenerationDelay = float.NegativeInfinity;
            
            Assert.IsTrue(float.IsNaN(config.maxEnergy), "Should preserve NaN values");
            Assert.IsTrue(float.IsPositiveInfinity(config.regenerationRate), "Should preserve positive infinity");
            Assert.IsTrue(float.IsNegativeInfinity(config.regenerationDelay), "Should preserve negative infinity");
            
            Object.DestroyImmediate(config);
        }

        #endregion
    }
}
