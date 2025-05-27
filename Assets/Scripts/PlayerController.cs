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
    [SerializeField] private float dashForce = 15f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayerMask = 1 << 8; // Layer 8 for Ground
    [SerializeField] private float groundCheckDistance = 2.0f; // Increased for testing
    
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
    private float dashCooldown = 1f;
    
    // Jump timing variables
    private float lastGroundedTime;
    private float lastJumpInputTime;
    
    void Start()
    {
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
        
        // Handle dash cooldown
        UpdateDashCooldown();
        
        if (enableDebugLogs)
        {
            LogPlayerState();
        }
    }
    
    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics consistency
        HandleMovement();
        HandleJump();
        HandleDash();
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
    
    private void HandleMovement()
    {
        // Calculate target velocity
        float targetVelocityX = horizontalInput * moveSpeed;
        
        // Apply acceleration/deceleration for smoother movement
        float velocityChangeRate = (Mathf.Abs(horizontalInput) > 0.1f) ? acceleration : deceleration;
        
        // Smoothly move towards target velocity
        float newVelocityX = Mathf.MoveTowards(rb2d.linearVelocity.x, targetVelocityX, velocityChangeRate * Time.fixedDeltaTime);
        
        // Apply the velocity while preserving vertical velocity
        rb2d.linearVelocity = new Vector2(newVelocityX, rb2d.linearVelocity.y);
        
        // Handle sprite flipping based on movement direction
        if (horizontalInput > 0.1f)
        {
            spriteRenderer.flipX = false; // Face right
        }
        else if (horizontalInput < -0.1f)
        {
            spriteRenderer.flipX = true;  // Face left
        }
        
        // Clamp to maximum speed (safety check)
        if (Mathf.Abs(rb2d.linearVelocity.x) > moveSpeed)
        {
            rb2d.linearVelocity = new Vector2(Mathf.Sign(rb2d.linearVelocity.x) * moveSpeed, rb2d.linearVelocity.y);
        }
    }
    
    private void HandleJump()
    {
        // Check if we can jump using coyote time and jump buffering
        bool canJumpGrounded = isGrounded || (Time.time - lastGroundedTime < coyoteTime);
        bool hasJumpInput = Time.time - lastJumpInputTime < jumpBufferTime;
        
        // Only jump if we have recent input and are recently grounded
        if (hasJumpInput && canJumpGrounded)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, jumpForce);
            
            // Clear the jump input buffer so we don't double jump
            lastJumpInputTime = 0f;
            
            if (enableDebugLogs)
            {
                Debug.Log($"PlayerController: Jump executed - Grounded: {isGrounded}, CoyoteTime: {canJumpGrounded}");
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
        // Only dash if input was pressed and dash is available
        if (dashInput && canDash)
        {
            // Apply dash force in the current movement direction
            float dashDirection = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : 1f;
            rb2d.linearVelocity = new Vector2(dashDirection * dashForce, rb2d.linearVelocity.y);
            
            // Start dash cooldown
            canDash = false;
            lastDashTime = Time.time;
            
            if (enableDebugLogs)
            {
                Debug.Log($"PlayerController: Dash executed in direction {dashDirection}");
            }
        }
    }
    
    private void UpdateDashCooldown()
    {
        // Reset dash availability after cooldown
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
                     $"CanDash: {canDash}, Facing: {(spriteRenderer.flipX ? "Left" : "Right")}");
        }
    }
    
    // Public getters for other systems to access player state
    public bool IsGrounded => isGrounded;
    public bool CanDash => canDash;
    public Vector2 Velocity => rb2d.linearVelocity;
    
    /// <summary>
    /// Debug visualization for ground detection
    /// Shows a green line when grounded, red when not grounded
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
    }
}