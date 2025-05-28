using UnityEngine;

/// <summary>
/// PlayerController for Mech Salvager - handles player input and mech movement
/// This is the main controller that translates input into mech actions
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f; // How quickly we reach max speed
    [SerializeField] private float deceleration = 10f; // How quickly we stop
    [Header("Jump Settings")]
    [SerializeField] private float coyoteTime = 0.15f; // Grace period after leaving ground
    [SerializeField] private float jumpBufferTime = 0.1f; // Input buffer for jump
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Energy System")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float dashEnergyCost = 25f;
    [SerializeField] private float energyRegenRate = 20f; // energy per second
    
    [Header("Dash System")]
    [SerializeField] private float dashForce = 25f;
    [SerializeField] private float dashCooldown = 0.5f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayerMask = 1 << 8; // Layer 8 for Ground
    [SerializeField] private float groundCheckDistance = 0.2f; // Increased for testing
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true; // Enabled for testing
    
    // Component references
    private Rigidbody2D rb2d;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    
    // Input tracking
    private float horizontalInput;
    private bool jumpInput;
    private bool dashInput;
    
    // State tracking
    private bool isGrounded;
    private bool canDash = true;
    private float lastDashTime;
    private float dashCooldownTimer = 0f;
    private Vector2 lastMoveDirection = Vector2.right; // default dash direction
    
    // Jump timing variables
    private float lastGroundedTime;
    private float lastJumpInputTime = -1f; // Initialize to prevent auto-jump
    
    void Start()
    {
        Debug.Log("=== PLAYERCONTROLLER START METHOD CALLED ===");
        
        // Get component references
        rb2d = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Validate components
        if (rb2d == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D component missing!");
        }
        
        if (boxCollider == null)
        {
            Debug.LogError("PlayerController: BoxCollider2D component missing!");
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerController: SpriteRenderer component missing!");
        }
        
        Debug.Log($"PlayerController: Jump force set to {jumpForce}");
        Debug.Log($"PlayerController: Ground check distance set to {groundCheckDistance}");
        
        // Set initial sprite direction (face right)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = true; // Face right by default (flipX = false means facing right)
        }
        
        if (enableDebugLogs)
        {
            Debug.Log("PlayerController: Initialized successfully");
        }
    }
    
    void Update()
    {
        // Read input using the input system we set up in T1.1.1
        ReadInput();
        
        // Check ground state
        CheckGrounded();
        
        // Store last movement direction for dash
        if (horizontalInput != 0)
        {
            lastMoveDirection = horizontalInput > 0 ? Vector2.right : Vector2.left;
        }
        
        // Regenerate energy over time
        RegenerateEnergy();
        
        // Update dash cooldown timer
        dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);
        
        // Handle dash cooldown (legacy system compatibility)
        UpdateDashCooldown();
        
        if (enableDebugLogs)
        {
            LogPlayerState();
        }
    }
    
    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics consistency
        // Handle dash BEFORE movement to prevent override
        HandleDash();
        HandleMovement();
        HandleJump();
    }
    
    private void ReadInput()
    {
        // Use the input system configured in T1.1.1
        horizontalInput = Input.GetAxis("Horizontal");
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        dashInput = Input.GetKeyDown(KeyCode.LeftShift);
        
        // Track jump input timing for buffering
        if (jumpInput)
        {
            lastJumpInputTime = Time.time;
        }
    }
    
    private void CheckGrounded()
    {
        // Cast a ray downward from the bottom of the player collider
        Vector2 raycastOrigin = (Vector2)transform.position + Vector2.down * (boxCollider.size.y / 2);
        
        // Perform the raycast
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, groundCheckDistance, groundLayerMask);
        
        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;
        
        // Update ground timing for coyote time
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
        
        // Log ground state changes for debugging
        if (enableDebugLogs && wasGrounded != isGrounded)
        {
            Debug.Log($"PlayerController: Ground state changed - isGrounded: {isGrounded}");
        }
    }
    
    private bool CanDash()
    {
        return currentEnergy >= dashEnergyCost && dashCooldownTimer <= 0f;
    }
    
    private void ConsumeEnergy(float amount)
    {
        currentEnergy = Mathf.Max(0f, currentEnergy - amount);
    }
    
    private void RegenerateEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyRegenRate * Time.deltaTime);
        }
    }
    
    private void HandleMovement()
    {
        // Calculate target velocity
        float targetVelocityX = horizontalInput * moveSpeed;
        
        // Apply acceleration/deceleration for smoother movement
        float velocityChangeRate = (Mathf.Abs(horizontalInput) > 0.1f) ? acceleration : deceleration;
        
        // When grounded, apply full movement control
        if (isGrounded)
        {
            // Smoothly move towards target velocity
            float newVelocityX = Mathf.MoveTowards(rb2d.linearVelocity.x, targetVelocityX, velocityChangeRate * Time.fixedDeltaTime);
            
            // Apply the velocity while preserving vertical velocity
            rb2d.linearVelocity = new Vector2(newVelocityX, rb2d.linearVelocity.y);
        }
        else
        {
            // In air: allow limited directional influence but don't override dash/momentum
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // Apply minimal air control (10% of ground movement)
                float airControlForce = targetVelocityX * 0.1f;
                float currentVelX = rb2d.linearVelocity.x;
                float newVelX = Mathf.MoveTowards(currentVelX, currentVelX + airControlForce, acceleration * 0.1f * Time.fixedDeltaTime);
                rb2d.linearVelocity = new Vector2(newVelX, rb2d.linearVelocity.y);
            }
        }
        
        // Handle sprite flipping based on movement direction
        if (horizontalInput > 0.1f)
        {
            spriteRenderer.flipX = true; // Face right (moving right)
        }
        else if (horizontalInput < -0.1f)
        {
            spriteRenderer.flipX = false;  // Face left (moving left)
        }
        
        // Clamp to maximum speed (safety check, but allow dash to exceed this)
        if (isGrounded && Mathf.Abs(rb2d.linearVelocity.x) > moveSpeed)
        {
            rb2d.linearVelocity = new Vector2(Mathf.Sign(rb2d.linearVelocity.x) * moveSpeed, rb2d.linearVelocity.y);
        }
    }
    
    private void HandleJump()
    {
        // Check if we can jump using coyote time and jump buffering
        bool canJumpGrounded = isGrounded || (Time.time - lastGroundedTime < coyoteTime);
        bool hasJumpInput = Time.time - lastJumpInputTime < jumpBufferTime;
        
        // Debug jump input detection
        if (jumpInput && enableDebugLogs)
        {
            Debug.Log($"PlayerController: Jump input detected! CanJump: {canJumpGrounded}, HasRecentInput: {hasJumpInput}");
        }
        
        // Only jump if we have recent input and are recently grounded
        if (hasJumpInput && canJumpGrounded)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, jumpForce);
            
            // Clear the jump input buffer so we don't double jump
            lastJumpInputTime = 0f;
            
            if (enableDebugLogs)
            {
                Debug.Log($"PlayerController: Jump executed - Grounded: {isGrounded}, CoyoteTime: {canJumpGrounded}, JumpForce: {jumpForce}");
            }
        }
        
        // Debug jump state when jump is attempted but fails
        if (jumpInput && !canJumpGrounded && enableDebugLogs)
        {
            Debug.Log($"PlayerController: Jump attempted but failed - Grounded: {isGrounded}, TimeSinceGrounded: {Time.time - lastGroundedTime:F2}");
        }
    }
    
    private void HandleDash()
    {
        // Enhanced dash system with energy and improved direction handling
        if (dashInput && CanDash())
        {
            // Determine dash direction: current input or last movement direction
            float dashDirection = horizontalInput != 0 ? horizontalInput : lastMoveDirection.x;
            
            // Apply dash force (works in air and on ground)
            rb2d.linearVelocity = new Vector2(dashDirection * dashForce, rb2d.linearVelocity.y);
            
            // Consume energy and start cooldown
            ConsumeEnergy(dashEnergyCost);
            dashCooldownTimer = dashCooldown;
            
            // Legacy compatibility
            canDash = false;
            lastDashTime = Time.time;
            
            if (enableDebugLogs)
            {
                Debug.Log($"*** DASH EXECUTED *** Direction: {dashDirection}, Force Applied: {dashDirection * dashForce}, Energy: {currentEnergy:F1}/{maxEnergy}, Cooldown: {dashCooldown}s");
            }
        }
        else if (dashInput && enableDebugLogs)
        {
            // Debug why dash failed
            if (currentEnergy < dashEnergyCost)
            {
                Debug.Log($"PlayerController: Dash failed - Insufficient energy: {currentEnergy:F1}/{dashEnergyCost}");
            }
            else if (dashCooldownTimer > 0f)
            {
                Debug.Log($"PlayerController: Dash failed - Cooldown active: {dashCooldownTimer:F2}s remaining");
            }
        }
    }
    
    private void UpdateDashCooldown()
    {
        // Legacy dash availability system (for compatibility with existing CanDash property)
        if (!canDash && Time.time - lastDashTime >= dashCooldown)
        {
            canDash = true;
        }
    }
    
    private void LogPlayerState()
    {
        if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
        {
            Debug.Log($"PlayerController State - Horizontal: {horizontalInput:F2}, " +
                     $"Grounded: {isGrounded}, Velocity: ({rb2d.linearVelocity.x:F2}, {rb2d.linearVelocity.y:F2}), " +
                     $"Energy: {currentEnergy:F1}/{maxEnergy}, DashCooldown: {dashCooldownTimer:F2}s, " +
                     $"Facing: {(spriteRenderer.flipX ? "Right" : "Left")}");
        }
    }
    
    // Public getters for other systems to access player state
    public bool IsGrounded => isGrounded;
    public bool CanDashState => CanDash();
    public Vector2 Velocity => rb2d.linearVelocity;
    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float EnergyPercent => currentEnergy / maxEnergy;
    public float DashCooldownRemaining => dashCooldownTimer;
    
    /// <summary>
    /// Debug visualization for ground detection and energy display
    /// Shows a green line when grounded, red when not grounded
    /// Also displays energy information in scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;
        
        // Calculate raycast origin (bottom of collider)
        Vector2 raycastOrigin = (Vector2)transform.position + Vector2.down * (boxCollider.size.y / 2);
        Vector2 raycastEnd = raycastOrigin + Vector2.down * groundCheckDistance;
        
        // Perform real-time raycast to get accurate ground state for gizmo
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, groundCheckDistance, groundLayerMask);
        bool isCurrentlyGrounded = hit.collider != null;
        
        // Set gizmo color based on real-time ground state
        Gizmos.color = isCurrentlyGrounded ? Color.green : Color.red;
        
        // Draw the ground detection ray
        Gizmos.DrawLine(raycastOrigin, raycastEnd);
        
        // Draw a small sphere at the raycast origin for visibility
        Gizmos.DrawWireSphere(raycastOrigin, 0.05f);
        
        #if UNITY_EDITOR
        // Show energy as text in scene view
        UnityEditor.Handles.Label(transform.position + Vector3.up, 
            $"Energy: {currentEnergy:F0}/{maxEnergy:F0}\nDash CD: {dashCooldownTimer:F1}s");
        #endif
    }
}