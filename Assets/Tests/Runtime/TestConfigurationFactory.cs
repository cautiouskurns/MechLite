using UnityEngine;
using MechLite.Configuration;

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
            config.airControlStrength = 0.5f;
            return config;
        }

        public static EnergyConfigSO CreateTestEnergyConfig()
        {
            var config = ScriptableObject.CreateInstance<EnergyConfigSO>();
            config.maxEnergy = 100f;
            config.energyRegenRate = 20f;
            config.regenDelay = 1f;
            config.dashEnergyCost = 50f;
            return config;
        }

        public static DashConfigSO CreateTestDashConfig()
        {
            var config = ScriptableObject.CreateInstance<DashConfigSO>();
            config.dashForce = 20f;
            config.dashCooldown = 1f;
            config.useInputDirection = true;
            config.defaultDashDirection = Vector2.right;
            config.allowAirDash = true;
            config.preserveVerticalVelocity = true;
            return config;
        }

        public static PhysicsConfigSO CreateTestPhysicsConfig()
        {
            var config = ScriptableObject.CreateInstance<PhysicsConfigSO>();
            config.groundLayerMask = 1 << 8; // Ground layer
            config.groundCheckDistance = 0.2f;
            config.groundCheckRadius = 0.05f;
            config.useSphereCast = false;
            config.groundCheckOffset = Vector2.zero;
            config.showGroundGizmos = true;
            config.groundedColor = Color.green;
            config.airborneColor = Color.red;
            return config;
        }
    }
}