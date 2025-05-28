using UnityEngine;
using MechLite.Configuration;
using MechLite.Events;

namespace MechLite.Movement
{
    /// <summary>
    /// Handles physics-based movement for the player character
    /// Implements IMovable interface and focuses solely on movement mechanics
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementController : MonoBehaviour, IMovable
    {
        [Header("Configuration")]
        [SerializeField] private MovementConfigSO movementConfig;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        
        // Component references
        private Rigidbody2D rb2d;
        private SpriteRenderer spriteRenderer;
        
        // Movement state
        private float horizontalInput;
        private bool isGrounded;
        
        // Properties from IMovable interface
        public Vector2 Velocity => rb2d.linearVelocity;
        public bool IsGrounded => isGrounded;
        
        /// <summary>
        /// Initialize the movement controller with configuration and ground detector
        /// Used by tests and programmatic setup
        /// </summary>
        public void Initialize(MovementConfigSO config, GroundDetector detector)
        {
            movementConfig = config;
            // Ground detector is handled through events, no direct reference needed
        }
        
        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (movementConfig == null)
            {
                Debug.LogError("MovementController: MovementConfigSO is not assigned!");
            }
        }
        
        private void Start()
        {
            // Subscribe to ground state events
            PlayerEventBus.OnGroundStateChanged += HandleGroundStateChanged;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            PlayerEventBus.OnGroundStateChanged -= HandleGroundStateChanged;
        }
        
        /// <summary>
        /// Set horizontal movement input
        /// </summary>
        /// <param name="input">Horizontal input value (-1 to 1)</param>
        public void SetHorizontalInput(float input)
        {
            horizontalInput = input;
        }
        
        /// <summary>
        /// Apply movement physics in FixedUpdate
        /// </summary>
        public void Move(float deltaTime)
        {
            if (movementConfig == null) return;
            
            // Calculate target velocity
            float targetVelocityX = horizontalInput * movementConfig.moveSpeed;
            
            // Apply acceleration/deceleration for smoother movement
            float velocityChangeRate = (Mathf.Abs(horizontalInput) > 0.1f) 
                ? movementConfig.acceleration 
                : movementConfig.deceleration;
            
            // Apply movement based on ground state
            if (isGrounded)
            {
                ApplyGroundedMovement(targetVelocityX, velocityChangeRate);
            }
            else
            {
                ApplyAirMovement(targetVelocityX, velocityChangeRate);
            }
            
            // Handle sprite flipping
            UpdateSpriteDirection();
            
            // Clamp velocity if configured and grounded
            if (movementConfig.clampGroundedVelocity && isGrounded)
            {
                ClampVelocity();
            }
            
            // Publish movement event
            PublishMovementEvent();
        }
        
        /// <summary>
        /// Apply jump force
        /// </summary>
        public bool Jump()
        {
            if (movementConfig == null) return false;
            
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, movementConfig.jumpForce);
            
            if (enableDebugLogs)
            {
                Debug.Log($"MovementController: Jump executed with force {movementConfig.jumpForce}");
            }
            return true;
        }
        
        /// <summary>
        /// Set velocity directly
        /// </summary>
        /// <param name="velocity">New velocity to set</param>
        public void SetVelocity(Vector2 velocity)
        {
            rb2d.linearVelocity = velocity;
        }
        
        private void ApplyGroundedMovement(float targetVelocityX, float velocityChangeRate)
        {
            // Smoothly move towards target velocity when grounded
            float newVelocityX = Mathf.MoveTowards(
                rb2d.linearVelocity.x, 
                targetVelocityX, 
                velocityChangeRate * Time.fixedDeltaTime
            );
            
            // Apply the velocity while preserving vertical velocity
            rb2d.linearVelocity = new Vector2(newVelocityX, rb2d.linearVelocity.y);
        }
        
        private void ApplyAirMovement(float targetVelocityX, float velocityChangeRate)
        {
            // In air: allow limited directional influence
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // Apply air control
                float airControlForce = targetVelocityX * movementConfig.airControlStrength;
                float currentVelX = rb2d.linearVelocity.x;
                float newVelX = Mathf.MoveTowards(
                    currentVelX, 
                    currentVelX + airControlForce, 
                    velocityChangeRate * movementConfig.airControlStrength * Time.fixedDeltaTime
                );
                rb2d.linearVelocity = new Vector2(newVelX, rb2d.linearVelocity.y);
            }
        }
        
        private void UpdateSpriteDirection()
        {
            if (spriteRenderer == null) return;
            
            // Handle sprite flipping based on movement direction
            if (horizontalInput > 0.1f)
            {
                spriteRenderer.flipX = true; // Face right
            }
            else if (horizontalInput < -0.1f)
            {
                spriteRenderer.flipX = false; // Face left
            }
        }
        
        private void ClampVelocity()
        {
            if (Mathf.Abs(rb2d.linearVelocity.x) > movementConfig.moveSpeed)
            {
                rb2d.linearVelocity = new Vector2(
                    Mathf.Sign(rb2d.linearVelocity.x) * movementConfig.moveSpeed, 
                    rb2d.linearVelocity.y
                );
            }
        }
        
        private void PublishMovementEvent()
        {
            PlayerEventBus.PublishPlayerMoved(new PlayerMovedEvent(
                rb2d.linearVelocity,
                transform.position,
                horizontalInput,
                isGrounded
            ));
        }
        
        private void HandleGroundStateChanged(GroundStateChangedEvent eventData)
        {
            isGrounded = eventData.isGrounded;
        }
    }
}
