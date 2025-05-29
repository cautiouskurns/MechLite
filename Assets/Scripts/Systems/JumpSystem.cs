using UnityEngine;
using MechLite.Configuration;
using MechLite.Events;
using MechLite.Energy;

namespace MechLite.Movement
{
    /// <summary>
    /// Handles jump mechanics including coyote time and jump buffering
    /// Works with MovementController and GroundDetector for jump execution
    /// </summary>
    public class JumpSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MovementConfigSO movementConfig;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        
        // Component references
        private IMovable movementController;
        private IGroundDetector groundDetector;
        
        // Jump state
        private float lastJumpInputTime = -1f;
        private bool jumpAttemptedForCurrentInput = false;
        
        /// <summary>
        /// Initialize the jump system with configuration and component references
        /// Used by tests and programmatic setup
        /// </summary>
        public void Initialize(PhysicsConfigSO physicsConfig, EnergyConfigSO energyConfig, IMovable movable, IGroundDetector detector, IEnergyUser energy)
        {
            // JumpSystem uses MovementConfigSO, but we'll accept the physics config for compatibility
            movementController = movable;
            groundDetector = detector;
            // Energy system is not directly used by JumpSystem in current implementation
        }
        
        private void Awake()
        {
            movementController = GetComponent<IMovable>();
            groundDetector = GetComponent<IGroundDetector>();
            
            if (movementConfig == null)
            {
                Debug.LogError("JumpSystem: MovementConfigSO is not assigned!");
            }
            
            if (movementController == null)
            {
                Debug.LogError("JumpSystem: IMovable component not found!");
            }
            
            if (groundDetector == null)
            {
                Debug.LogError("JumpSystem: IGroundDetector component not found!");
            }
        }
        
        /// <summary>
        /// Process jump input (just records the input timing)
        /// </summary>
        /// <param name="jumpInput">Whether jump was pressed this frame</param>
        public void ProcessJumpInput(bool jumpInput)
        {
            // Track jump input timing for buffering
            if (jumpInput)
            {
                lastJumpInputTime = Time.time;
                jumpAttemptedForCurrentInput = false; // Reset attempt flag for new input
                
                if (enableDebugLogs)
                {
                    Debug.Log($"JumpSystem: Jump input detected at {Time.time}");
                }
            }
        }
        
        /// <summary>
        /// Update jump system - should be called every frame to process pending jump attempts
        /// </summary>
        public void UpdateJumpSystem()
        {
            // Attempt to execute jump if we have recent input and haven't tried yet
            if (HasRecentJumpInput() && !jumpAttemptedForCurrentInput)
            {
                jumpAttemptedForCurrentInput = true; // Mark that we've attempted for this input
                TryExecuteJump();
            }
        }
        
        /// <summary>
        /// Try to execute a jump based on current conditions
        /// </summary>
        public void TryExecuteJump()
        {
            if (movementConfig == null || movementController == null || groundDetector == null) return;
            
            // Check if we can jump using coyote time and ground detection
            bool canJumpGrounded = groundDetector.CanPerformGroundAction();
            bool hasRecentJumpInput = HasRecentJumpInput();
            
            // Only jump if we have recent input and can perform ground action
            if (hasRecentJumpInput && canJumpGrounded)
            {
                ExecuteJump(canJumpGrounded, hasRecentJumpInput);
            }
            else if (enableDebugLogs && HasJumpInputThisFrame())
            {
                LogJumpFailureReason(canJumpGrounded);
            }
        }
        
        /// <summary>
        /// Force execute a jump (for external systems)
        /// </summary>
        public void ForceJump()
        {
            if (movementController != null)
            {
                movementController.Jump();
                PublishJumpEvent(false, false);
                
                if (enableDebugLogs)
                {
                    Debug.Log("JumpSystem: Force jump executed");
                }
            }
        }
        
        private void ExecuteJump(bool usedGroundAction, bool usedJumpBuffer)
        {
            // Execute the jump
            movementController.Jump();
            
            // Clear the jump input buffer
            lastJumpInputTime = 0f;
            jumpAttemptedForCurrentInput = false; // Reset for next input
            
            // Determine if coyote time was used
            bool usedCoyoteTime = !groundDetector.IsGrounded && usedGroundAction;
            
            // Publish jump event
            PublishJumpEvent(usedCoyoteTime, usedJumpBuffer);
            
            if (enableDebugLogs)
            {
                Debug.Log($"JumpSystem: Jump executed - " +
                         $"Grounded: {groundDetector.IsGrounded}, " +
                         $"CoyoteTime: {usedCoyoteTime}, " +
                         $"JumpBuffer: {usedJumpBuffer}, " +
                         $"Force: {movementConfig.jumpForce}");
            }
        }
        
        private bool HasRecentJumpInput()
        {
            if (movementConfig == null) return false;
            return Time.time - lastJumpInputTime < movementConfig.jumpBufferTime;
        }
        
        private bool HasJumpInputThisFrame()
        {
            return Mathf.Abs(Time.time - lastJumpInputTime) < 0.01f;
        }
        
        private void LogJumpFailureReason(bool canJumpGrounded)
        {
            if (!canJumpGrounded)
            {
                float timeSinceGrounded = groundDetector.IsGrounded ? 0f : 
                    (movementConfig.coyoteTime - groundDetector.CoyoteTimeRemaining);
                Debug.Log($"JumpSystem: Jump failed - Not grounded. Time since grounded: {timeSinceGrounded:F2}s, " +
                         $"Coyote remaining: {groundDetector.CoyoteTimeRemaining:F2}s");
            }
            else
            {
                Debug.Log("JumpSystem: Jump failed - No recent input or other condition not met");
            }
        }
        
        private void PublishJumpEvent(bool usedCoyoteTime, bool usedJumpBuffer)
        {
            Vector2 jumpVelocity = movementController?.Velocity ?? Vector2.zero;
            
            PlayerEventBus.PublishPlayerJumped(new PlayerJumpedEvent(
                jumpVelocity,
                transform.position,
                usedCoyoteTime,
                usedJumpBuffer
            ));
        }
        
        /// <summary>
        /// Get whether a jump can currently be performed
        /// </summary>
        /// <returns>True if jump is possible</returns>
        public bool CanJump()
        {
            return groundDetector?.CanPerformGroundAction() ?? false;
        }
        
        /// <summary>
        /// Get remaining coyote time
        /// </summary>
        /// <returns>Coyote time remaining in seconds</returns>
        public float GetCoyoteTimeRemaining()
        {
            return groundDetector?.CoyoteTimeRemaining ?? 0f;
        }
        
        /// <summary>
        /// Get remaining jump buffer time
        /// </summary>
        /// <returns>Jump buffer time remaining in seconds</returns>
        public float GetJumpBufferTimeRemaining()
        {
            if (movementConfig == null) return 0f;
            return Mathf.Max(0f, movementConfig.jumpBufferTime - (Time.time - lastJumpInputTime));
        }
    }
}
