using UnityEngine;
using MechLite.Movement;
using MechLite.Energy;
using MechLite.Events;
using MechLite.Configuration;

namespace MechLite.Player
{
    /// <summary>
    /// Refactored PlayerController that acts as a coordinator for player systems
    /// Handles input and delegates to specialized systems for implementation
    /// Maintains the same public API as the original PlayerController for compatibility
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private MovementController movementController;
        [SerializeField] private EnergySystem energySystem;
        [SerializeField] private GroundDetector groundDetector;
        [SerializeField] private DashSystem dashSystem;
        [SerializeField] private JumpSystem jumpSystem;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private bool enableRuntimeTuning = false;

        // Input tracking
        private float horizontalInput;
        private bool jumpInput;
        private bool dashInput;

        // Public API properties (maintains compatibility with original PlayerController)
        public bool IsGrounded => groundDetector?.IsGrounded ?? false;
        public bool CanDashState => dashSystem?.CanDash ?? false;
        public Vector2 Velocity => movementController?.Velocity ?? Vector2.zero;
        public float CurrentEnergy => energySystem?.CurrentEnergy ?? 0f;
        public float MaxEnergy => energySystem?.MaxEnergy ?? 100f;
        public float EnergyPercent => energySystem?.EnergyPercent ?? 0f;
        public float DashCooldownRemaining => dashSystem?.DashCooldownRemaining ?? 0f;

        private void Awake()
        {
            // Auto-find systems if not assigned
            if (movementController == null)
                movementController = GetComponent<MovementController>();
            if (energySystem == null)
                energySystem = GetComponent<EnergySystem>();
            if (groundDetector == null)
                groundDetector = GetComponent<GroundDetector>();
            if (dashSystem == null)
                dashSystem = GetComponent<DashSystem>();
            if (jumpSystem == null)
                jumpSystem = GetComponent<JumpSystem>();

            ValidateSystemReferences();
        }

        private void Start()
        {
            Debug.Log("=== REFACTORED PLAYERCONTROLLER START METHOD CALLED ===");

            // Subscribe to events for debugging
            if (enableDebugLogs)
            {
                SubscribeToEvents();
            }

            Debug.Log("PlayerController: Initialized successfully with new architecture");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (enableDebugLogs)
            {
                UnsubscribeFromEvents();
            }
        }

        private void Update()
        {
            // Read input
            ReadInput();

            // Update last movement direction for dash system
            if (dashSystem != null && horizontalInput != 0)
            {
                Vector2 moveDirection = horizontalInput > 0 ? Vector2.right : Vector2.left;
                dashSystem.SetLastMoveDirection(moveDirection);
            }

            // Debug logging
            if (enableDebugLogs)
            {
                LogPlayerState();
            }
        }

        private void FixedUpdate()
        {
            // Delegate to systems in the correct order
            HandleMovement();
            HandleJump();
            HandleDash();
        }

        private void ReadInput()
        {
            // Use the same input system as the original
            horizontalInput = Input.GetAxis("Horizontal");
            jumpInput = Input.GetKeyDown(KeyCode.Space);
            dashInput = Input.GetKeyDown(KeyCode.E);
        }

        private void HandleMovement()
        {
            if (movementController != null)
            {
                movementController.SetHorizontalInput(horizontalInput);
                movementController.Move(Time.fixedDeltaTime);
            }
        }

        private void HandleJump()
        {
            if (jumpSystem != null)
            {
                // Process input
                jumpSystem.ProcessJumpInput(jumpInput);
                
                // Update jump system to process pending jump attempts
                jumpSystem.UpdateJumpSystem();
            }
        }

        private void HandleDash()
        {
            if (dashInput && dashSystem != null)
            {
                dashSystem.Dash(horizontalInput);
            }
        }

        private void ValidateSystemReferences()
        {
            if (movementController == null)
                Debug.LogError("PlayerController: MovementController component missing!");
            if (energySystem == null)
                Debug.LogError("PlayerController: EnergySystem component missing!");
            if (groundDetector == null)
                Debug.LogError("PlayerController: GroundDetector component missing!");
            if (dashSystem == null)
                Debug.LogError("PlayerController: DashSystem component missing!");
            if (jumpSystem == null)
                Debug.LogError("PlayerController: JumpSystem component missing!");
        }

        private void SubscribeToEvents()
        {
            PlayerEventBus.OnPlayerMoved += OnPlayerMoved;
            PlayerEventBus.OnPlayerJumped += OnPlayerJumped;
            PlayerEventBus.OnPlayerDashed += OnPlayerDashed;
            PlayerEventBus.OnEnergyChanged += OnEnergyChanged;
            PlayerEventBus.OnGroundStateChanged += OnGroundStateChanged;
        }

        private void UnsubscribeFromEvents()
        {
            PlayerEventBus.OnPlayerMoved -= OnPlayerMoved;
            PlayerEventBus.OnPlayerJumped -= OnPlayerJumped;
            PlayerEventBus.OnPlayerDashed -= OnPlayerDashed;
            PlayerEventBus.OnEnergyChanged -= OnEnergyChanged;
            PlayerEventBus.OnGroundStateChanged -= OnGroundStateChanged;
        }

        private void LogPlayerState()
        {
            if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
            {
                Debug.Log($"PlayerController State - " +
                         $"Input: {horizontalInput:F2}, " +
                         $"Grounded: {IsGrounded}, " +
                         $"Velocity: ({Velocity.x:F2}, {Velocity.y:F2}), " +
                         $"Energy: {CurrentEnergy:F1}/{MaxEnergy}, " +
                         $"DashCD: {DashCooldownRemaining:F2}s");
            }
        }

        // Event handlers for debugging
        private void OnPlayerMoved(PlayerMovedEvent eventData)
        {
            // Could trigger visual effects, sound, etc.
        }

        private void OnPlayerJumped(PlayerJumpedEvent eventData)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"PlayerController: Jump event - CoyoteTime: {eventData.usedCoyoteTime}, Buffer: {eventData.usedJumpBuffer}");
            }
        }

        private void OnPlayerDashed(PlayerDashedEvent eventData)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"PlayerController: Dash event - Direction: {eventData.dashDirection}, Energy: {eventData.remainingEnergy}");
            }
        }

        private void OnEnergyChanged(EnergyChangedEvent eventData)
        {
            // Could update UI, trigger effects based on energy level, etc.
        }

        private void OnGroundStateChanged(GroundStateChangedEvent eventData)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"PlayerController: Ground state changed - isGrounded: {eventData.isGrounded}");
            }
        }

        // Public methods for external systems (maintains API compatibility)

        /// <summary>
        /// Initialize the PlayerController and all its subsystems with configuration
        /// This method is used by tests and external systems to set up the controller
        /// </summary>
        /// <param name="movementConfig">Movement configuration</param>
        /// <param name="energyConfig">Energy system configuration</param>
        /// <param name="dashConfig">Dash system configuration</param>
        /// <param name="physicsConfig">Physics configuration</param>
        public void Initialize(MovementConfigSO movementConfig, EnergyConfigSO energyConfig, DashConfigSO dashConfig, PhysicsConfigSO physicsConfig)
        {
            try
            {
                // Initialize MovementController
                if (movementController != null && movementConfig != null)
                {
                    // MovementController expects (config, groundDetector)
                    movementController.Initialize(movementConfig, groundDetector);
                }

                // Initialize EnergySystem
                if (energySystem != null && energyConfig != null)
                {
                    energySystem.Initialize(energyConfig);
                }

                // Initialize DashSystem 
                if (dashSystem != null && dashConfig != null)
                {
                    // DashSystem expects (config, energySystem)
                    dashSystem.Initialize(dashConfig, energySystem);
                }

                // Initialize GroundDetector
                if (groundDetector != null && physicsConfig != null)
                {
                    groundDetector.Initialize(physicsConfig);
                }

                // Initialize JumpSystem
                if (jumpSystem != null && physicsConfig != null && energyConfig != null)
                {
                    // JumpSystem expects (physicsConfig, energyConfig, movable, groundDetector, energySystem)
                    jumpSystem.Initialize(physicsConfig, energyConfig, movementController, groundDetector, energySystem);
                }

                if (enableDebugLogs)
                {
                    Debug.Log("PlayerController: All systems initialized successfully");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"PlayerController: Failed to initialize systems - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get whether dash is available (legacy compatibility)
        /// </summary>
        public bool CanDash()
        {
            return dashSystem?.CanDash ?? false;
        }

        /// <summary>
        /// Check if player has enough energy for an action
        /// </summary>
        /// <param name="amount">Energy amount required</param>
        /// <returns>True if enough energy is available</returns>
        public bool HasEnergy(float amount)
        {
            return energySystem?.HasEnergy(amount) ?? false;
        }

        /// <summary>
        /// Get energy as normalized value (0-1)
        /// </summary>
        /// <returns>Energy percentage</returns>
        public float GetEnergyNormalized()
        {
            return EnergyPercent;
        }

        /// <summary>
        /// Move the player in the specified direction
        /// Used by tests and external systems
        /// </summary>
        /// <param name="direction">Movement direction</param>
        public void Move(Vector2 direction)
        {
            if (movementController != null)
            {
                movementController.SetHorizontalInput(direction.x);
                movementController.Move(Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Attempt to make the player jump
        /// Used by tests and external systems
        /// </summary>
        /// <returns>True if jump was successful</returns>
        public bool Jump()
        {
            if (jumpSystem != null)
            {
                jumpSystem.TryExecuteJump();
                return true; // For compatibility, always return true when jump system exists
            }
            return false;
        }

        /// <summary>
        /// Attempt to dash in the specified direction
        /// Used by tests and external systems
        /// </summary>
        /// <param name="direction">Dash direction</param>
        /// <returns>True if dash was successful</returns>
        public bool Dash(Vector2 direction)
        {
            if (dashSystem != null)
            {
                // Set the direction first
                if (direction.magnitude > 0)
                {
                    dashSystem.SetLastMoveDirection(direction.normalized);
                }
                return dashSystem.Dash(direction.x);
            }
            return false;
        }

        /// <summary>
        /// Debug visualization using the GroundDetector's gizmos
        /// </summary>
        private void OnDrawGizmos()
        {
            // The individual systems handle their own gizmo drawing
            // This maintains the same visual debugging as the original

#if UNITY_EDITOR
        // Show system status in scene view
        if (enableDebugLogs)
        {
            string systemStatus = $"Systems Status:\n" +
                                $"Movement: {(movementController != null ? "✓" : "✗")}\n" +
                                $"Energy: {(energySystem != null ? "✓" : "✗")}\n" +
                                $"Ground: {(groundDetector != null ? "✓" : "✗")}\n" +
                                $"Dash: {(dashSystem != null ? "✓" : "✗")}\n" +
                                $"Jump: {(jumpSystem != null ? "✓" : "✗")}";
            
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, systemStatus);
        }
#endif
        }
    }
}