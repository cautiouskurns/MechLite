using UnityEngine;

namespace MechLite.Tests.Utilities
{
    /// <summary>
    /// Utility class for creating test configurations with known values
    /// </summary>
    public static class TestConfigurationFactory
    {
        public static MovementConfigSO CreateTestMovementConfig()
        {
            var config = ScriptableObject.CreateInstance<MovementConfigSO>();
            config.moveSpeed = 10f;
            config.acceleration = 50f;
            config.deceleration = 30f;
            config.airControl = 0.5f;
            return config;
        }

        public static EnergyConfigSO CreateTestEnergyConfig()
        {
            var config = ScriptableObject.CreateInstance<EnergyConfigSO>();
            config.maxEnergy = 100f;
            config.regenerationRate = 20f;
            config.regenerationDelay = 1f;
            config.jumpEnergyCost = 25f;
            config.dashEnergyCost = 50f;
            return config;
        }

        public static DashConfigSO CreateTestDashConfig()
        {
            var config = ScriptableObject.CreateInstance<DashConfigSO>();
            config.dashForce = 20f;
            config.dashDuration = 0.2f;
            config.dashCooldown = 1f;
            config.energyCost = 50f;
            return config;
        }

        public static PhysicsConfigSO CreateTestPhysicsConfig()
        {
            var config = ScriptableObject.CreateInstance<PhysicsConfigSO>();
            config.jumpForce = 15f;
            config.gravityScale = 3f;
            config.coyoteTime = 0.1f;
            config.jumpBufferTime = 0.1f;
            config.maxFallSpeed = 25f;
            return config;
        }
    }
}