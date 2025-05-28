using UnityEngine;
using MechLite.Configuration;
using MechLite.Events;
using MechLite.Energy;

namespace MechLite.Movement
{
    /// <summary>
    /// Handles dash mechanics for the player character
    /// Implements IDashable interface and manages dash cooldowns and energy consumption
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class DashSystem : MonoBehaviour, IDashable
    {
        [Header("Configuration")]
        [SerializeField] private DashConfigSO dashConfig;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        
        // Component references
        private Rigidbody2D rb2d;
        private IEnergyUser energySystem;
        
        // Dash state
        private float dashCooldownTimer = 0f;
        private Vector2 lastMoveDirection = Vector2.right;
        private float lastDashTime;
        
        // Properties from IDashable interface
        public bool CanDash => dashCooldownTimer <= 0f && HasSufficientEnergy();
        public float DashCooldownRemaining => Mathf.Max(0f, dashCooldownTimer);
        
        /// <summary>
        /// Initialize the dash system with configuration and energy system
        /// Used by tests and programmatic setup
        /// </summary>
        public void Initialize(DashConfigSO config, IEnergyUser energy)
        {
            dashConfig = config;
            energySystem = energy;
        }
        
        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            energySystem = GetComponent<IEnergyUser>();
            
            if (dashConfig == null)
            {
                Debug.LogError("DashSystem: DashConfigSO is not assigned!");
            }
            
            if (energySystem == null)
            {
                Debug.LogError("DashSystem: IEnergyUser component not found! DashSystem requires an energy system.");
            }
        }
        
        private void Update()
        {
            UpdateDashCooldown();
        }
        
        /// <summary>
        /// Attempt to perform a dash
        /// </summary>
        /// <param name="inputDirection">Current input direction (-1, 0, or 1)</param>
        /// <returns>True if dash was successfully executed</returns>
        public bool Dash(float inputDirection = 0f)
        {
            if (!CanDash)
            {
                if (enableDebugLogs)
                {
                    LogDashFailureReason();
                }
                return false;
            }
            
            // Determine dash direction
            Vector2 dashDirection = GetDashDirection(inputDirection);
            
            // Apply dash force
            Vector2 dashVelocity = CalculateDashVelocity(dashDirection);
            rb2d.linearVelocity = dashVelocity;
            
            // Consume energy and start cooldown
            energySystem?.ConsumeEnergy(GetDashEnergyCost());
            dashCooldownTimer = dashConfig.dashCooldown;
            lastDashTime = Time.time;
            
            // Publish dash event
            PublishDashEvent(dashDirection, dashVelocity);
            
            if (enableDebugLogs)
            {
                Debug.Log($"DashSystem: Dash executed - Direction: {dashDirection}, Velocity: {dashVelocity}, Energy Cost: {GetDashEnergyCost()}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Update dash cooldown timer
        /// </summary>
        public void UpdateDashCooldown()
        {
            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Set the last movement direction for dash direction determination
        /// </summary>
        /// <param name="direction">Movement direction vector</param>
        public void SetLastMoveDirection(Vector2 direction)
        {
            if (direction.magnitude > 0.1f)
            {
                lastMoveDirection = direction.normalized;
            }
        }
        
        /// <summary>
        /// Check if dash can be performed in current state
        /// </summary>
        /// <returns>True if dash is available</returns>
        public bool IsDashAvailable()
        {
            return CanDash;
        }
        
        private bool HasSufficientEnergy()
        {
            if (energySystem == null) return true; // If no energy system, allow dash
            return energySystem.HasEnergy(GetDashEnergyCost());
        }
        
        private float GetDashEnergyCost()
        {
            // Try to get cost from energy config, fallback to dash config if available
            var energyConfig = FindAnyObjectByType<EnergySystem>()?.GetComponent<EnergySystem>();
            if (energyConfig != null)
            {
                // This would require accessing the config - for now use default
                return 25f; // Default dash energy cost
            }
            return 25f;
        }
        
        private Vector2 GetDashDirection(float inputDirection)
        {
            if (dashConfig == null) return Vector2.right;
            
            if (dashConfig.useInputDirection && Mathf.Abs(inputDirection) > 0.1f)
            {
                // Use current input direction
                return new Vector2(Mathf.Sign(inputDirection), 0f);
            }
            else if (lastMoveDirection.magnitude > 0.1f)
            {
                // Use last movement direction
                return new Vector2(lastMoveDirection.x, 0f).normalized;
            }
            else
            {
                // Use default direction
                return dashConfig.defaultDashDirection;
            }
        }
        
        private Vector2 CalculateDashVelocity(Vector2 dashDirection)
        {
            if (dashConfig == null) return Vector2.zero;
            
            Vector2 dashVelocity = dashDirection * dashConfig.dashForce;
            
            // Preserve vertical velocity if configured
            if (dashConfig.preserveVerticalVelocity)
            {
                dashVelocity.y = rb2d.linearVelocity.y;
            }
            
            return dashVelocity;
        }
        
        private void PublishDashEvent(Vector2 dashDirection, Vector2 dashVelocity)
        {
            PlayerEventBus.PublishPlayerDashed(new PlayerDashedEvent(
                dashDirection,
                dashConfig?.dashForce ?? 18f,
                transform.position,
                GetDashEnergyCost(),
                energySystem?.CurrentEnergy ?? 0f
            ));
        }
        
        private void LogDashFailureReason()
        {
            if (dashCooldownTimer > 0f)
            {
                Debug.Log($"DashSystem: Dash failed - Cooldown active: {dashCooldownTimer:F2}s remaining");
            }
            else if (!HasSufficientEnergy())
            {
                Debug.Log($"DashSystem: Dash failed - Insufficient energy: {energySystem?.CurrentEnergy ?? 0f}/{GetDashEnergyCost()}");
            }
            else
            {
                Debug.Log("DashSystem: Dash failed - Unknown reason");
            }
        }
        
        /// <summary>
        /// Reset dash cooldown (for testing or special abilities)
        /// </summary>
        public void ResetCooldown()
        {
            dashCooldownTimer = 0f;
            
            if (enableDebugLogs)
            {
                Debug.Log("DashSystem: Cooldown reset");
            }
        }
        
        /// <summary>
        /// Get time since last dash
        /// </summary>
        /// <returns>Time in seconds since last dash</returns>
        public float GetTimeSinceLastDash()
        {
            return Time.time - lastDashTime;
        }
    }
}
