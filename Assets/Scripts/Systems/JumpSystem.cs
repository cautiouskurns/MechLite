using UnityEngine;
using MechLite.Configuration;
using MechLite.Events;
using MechLite.Energy;

namespace MechLite.Movement
{
    /// <summary>
    /// Dead simple jump system: Grounded + Space = Jump. That's it.
    /// </summary>
    public class JumpSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MovementConfigSO movementConfig;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true; // Enable by default for debugging
        
        // Component references
        private IMovable movementController;
        private IGroundDetector groundDetector;
        
        public void Initialize(PhysicsConfigSO physicsConfig, EnergyConfigSO energyConfig, IMovable movable, IGroundDetector detector, IEnergyUser energy)
        {
            movementController = movable;
            groundDetector = detector;
        }
        
        private void Awake()
        {
            movementController = GetComponent<IMovable>();
            groundDetector = GetComponent<IGroundDetector>();
            
            if (movementConfig == null)
                Debug.LogError("JumpSystem: MovementConfigSO is not assigned!");
            if (movementController == null)
                Debug.LogError("JumpSystem: IMovable component not found!");
            if (groundDetector == null)
                Debug.LogError("JumpSystem: IGroundDetector component not found!");
        }
        
        /// <summary>
        /// Process jump input - if space pressed and grounded, jump immediately
        /// </summary>
        public void ProcessJumpInput(bool jumpPressed)
        {
            if (enableDebugLogs)
                Debug.Log($"JumpSystem: ProcessJumpInput - Space pressed: {jumpPressed}, Grounded: {groundDetector?.IsGrounded}, CanJump: {CanJump()}");
                
            if (jumpPressed && CanJump())
            {
                ExecuteJump();
            }
            else if (jumpPressed && !CanJump())
            {
                if (enableDebugLogs)
                    Debug.Log($"JumpSystem: Jump input ignored - not grounded (IsGrounded: {groundDetector?.IsGrounded})");
            }
        }
        
        /// <summary>
        /// Not needed anymore - we jump immediately when space is pressed
        /// </summary>
        public void UpdateJumpSystem()
        {
            // Nothing to do - we handle jumps immediately in ProcessJumpInput
        }
        
        /// <summary>
        /// Check if we can jump right now
        /// </summary>
        public bool CanJump()
        {
            bool grounded = groundDetector?.IsGrounded ?? false;
            if (enableDebugLogs)
                Debug.Log($"JumpSystem CanJump: {grounded}");
            return grounded;
        }
        
        /// <summary>
        /// Execute the jump
        /// </summary>
        private void ExecuteJump()
        {
            if (movementController == null) return;
            
            // Jump!
            movementController.Jump();
            
            // Publish event
            PlayerEventBus.PublishPlayerJumped(new PlayerJumpedEvent(
                movementController.Velocity,
                transform.position,
                false, // not using coyote time for now
                false  // not using jump buffer for now
            ));
            
            if (enableDebugLogs)
            {
                Debug.Log($"JumpSystem: *** JUMP EXECUTED! *** Velocity after jump: {movementController.Velocity}");
            }
        }
        
        /// <summary>
        /// Force execute a jump (for external systems)
        /// </summary>
        public void TryExecuteJump()
        {
            if (CanJump())
            {
                ExecuteJump();
            }
            else if (enableDebugLogs)
            {
                Debug.Log($"JumpSystem: TryExecuteJump failed - not grounded");
            }
        }
        
        public float GetCoyoteTimeRemaining() => 0f; // Disabled for now
        public float GetJumpBufferTimeRemaining() => 0f; // Disabled for now
    }
}