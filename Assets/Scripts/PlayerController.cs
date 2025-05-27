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
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float dashForce = 15f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
    [SerializeField] private float groundCheckDistance = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    
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
    }
    
    private void CheckGrounded()
    {
        // Cast a ray downward from the bottom of the collider
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.down * (boxCollider.size.y / 2);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayerMask);
        
        isGrounded = hit.collider != null;
        
        // Debug visualization
        if (enableDebugLogs)
        {
            Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
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
        // Only jump if grounded and jump input was pressed
        if (jumpInput && isGrounded)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, jumpForce);
            
            if (enableDebugLogs)
            {
                Debug.Log("PlayerController: Jump executed");
            }
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
}