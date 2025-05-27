# Sequential VS Code Copilot Agent Prompts for M1.1: Player Movement System

## Task T1.1.1: Basic Input Setup

**Context**: Building foundational movement system for Mech Salvager, a 2D tactical platformer. This task establishes simple input handling using Unity's built-in Legacy Input Manager for mech movement (walk, jump, dash).

**Goal**: Verify and configure Unity's Legacy Input Manager for basic movement controls.

**Requirements**: 
- Use Unity's built-in Input Manager (no additional packages needed)
- Verify default input axes for movement
- Identify key bindings for jump and dash actions
- Simple, immediate functionality without configuration overhead

**Assets to Create**:
- No assets needed - using Unity's built-in Input Manager
- Input handling code will use `Input.GetAxis()` and `Input.GetKey()` methods

**Implementation Steps**:
1. Go to Edit → Project Settings → Input Manager
2. Verify existing axes are configured:
   - "Horizontal" axis should exist (mapped to A/D keys and Left/Right arrows)
   - "Vertical" axis should exist (mapped to W/S keys and Up/Down arrows)
3. Note the key mappings we'll use:
   - Movement: A/D keys via "Horizontal" axis
   - Jump: Space key via `Input.GetKeyDown(KeyCode.Space)`
   - Dash: Left Shift key via `Input.GetKeyDown(KeyCode.LeftShift)`
4. Create basic test to verify input reading:
   - Create temporary script to log input values
   - Test: `Debug.Log(Input.GetAxis("Horizontal"));`
   - Test: `Debug.Log(Input.GetKeyDown(KeyCode.Space));`

**Validation**: 
- Input Manager shows "Horizontal" and "Vertical" axes configured
- A/D keys register as positive/negative values on "Horizontal" axis
- Space key registers true when pressed via `GetKeyDown(KeyCode.Space)`
- Left Shift key registers true when pressed via `GetKeyDown(KeyCode.LeftShift)`
- No compilation errors in Unity console
- Input system responds immediately without setup

**Next**: This establishes the simple input foundation that PlayerController will use to read movement commands directly.

---

## Task T1.1.2: Player GameObject Setup

**Context**: Creating the player GameObject hierarchy for Mech Salvager. This establishes the visual representation and physics foundation for our mech character that will be controlled by the input system.

**Goal**: Create a properly configured Player GameObject with all necessary components for 2D physics-based movement.

**Requirements**:
- Single GameObject named "Player" with all movement components
- Physics-based movement using Rigidbody2D
- Collision detection with BoxCollider2D
- Visual representation with SpriteRenderer
- PlayerController script attachment point

**Assets to Create**:
- GameObject: "Player" in scene hierarchy
- Component: SpriteRenderer with 32x32 colored square sprite
- Component: Rigidbody2D with specific physics settings
- Component: BoxCollider2D sized to match sprite
- Script: `Assets/Scripts/PlayerController.cs` (skeleton)

**Implementation Steps**:
1. Create folder `Assets/Scripts/` if it doesn't exist
2. In scene hierarchy, right-click → Create Empty, name it "Player"
3. Position Player at (0, 0, 0)
4. Add SpriteRenderer component:
   - Create a 32x32 colored square sprite (use Unity's built-in sprite creation or import simple colored square)
   - Set sprite to the colored square
   - Set Order in Layer to 1
5. Add Rigidbody2D component:
   - Mass: 1
   - Linear Drag: 0.5
   - Angular Drag: 0.05
   - Gravity Scale: 1
   - Freeze Rotation Z: true (check this box)
6. Add BoxCollider2D component:
   - Size: (0.9, 0.9) to slightly smaller than sprite
7. Create `PlayerController.cs` script in Assets/Scripts/
8. Add basic skeleton code with Start/Update methods
9. Attach PlayerController script to Player GameObject

**Validation**:
- Player GameObject exists in hierarchy at (0,0,0)
- SpriteRenderer shows colored square sprite
- Rigidbody2D settings match specifications exactly
- BoxCollider2D is slightly smaller than sprite
- PlayerController script is attached and shows in Inspector
- Player GameObject falls due to gravity when Play is pressed

**Next**: This creates the GameObject foundation that PlayerController will manipulate for movement.

---

## Task T1.1.3: Horizontal Movement

**Context**: Implementing the core horizontal movement for our mech in Mech Salvager. This provides the foundation left/right movement that players will use most frequently during gameplay.

**Goal**: Enable smooth left/right movement with proper sprite flipping and speed limiting using Unity's Input System.

**Requirements**:
- Read input from PlayerInputActions system
- Apply velocity to Rigidbody2D for physics-based movement
- Clamp maximum movement speed to 5 units/second
- Flip sprite based on movement direction
- Use FixedUpdate for consistent physics

**Assets to Create**:
- Complete `PlayerController.cs` with movement logic
- SerializeField parameters for speed tuning
- Reference to PlayerInputActions in code

**Implementation Steps**:
1. Open `PlayerController.cs` script
2. Add using statements: `using UnityEngine.InputSystem;`
3. Add private fields:
   - `PlayerInputActions playerInput`
   - `Rigidbody2D rb`
   - `SpriteRenderer spriteRenderer`
   - `[SerializeField] float moveSpeed = 5f`
   - `Vector2 moveDirection`
4. In Awake():
   - Get Rigidbody2D component
   - Get SpriteRenderer component
   - Initialize playerInput = new PlayerInputActions()
5. In OnEnable(): playerInput.Enable()
6. In OnDisable(): playerInput.Disable()
7. In Update():
   - Read moveDirection = playerInput.Player.Move.ReadValue<Vector2>()
   - Handle sprite flipping based on moveDirection.x
8. In FixedUpdate():
   - Apply horizontal velocity: rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y)
9. Add sprite flipping logic:
   - If moveDirection.x > 0: spriteRenderer.flipX = false
   - If moveDirection.x < 0: spriteRenderer.flipX = true

**Validation**:
- Press A/D keys: Player moves left/right smoothly
- Movement speed caps at 5 units/second
- Sprite flips correctly when changing direction
- Movement feels responsive without stuttering
- Player maintains vertical velocity (doesn't interfere with gravity)
- Movement works in both Play mode and builds

**Next**: This enables basic locomotion, setting up the foundation for jump and dash mechanics.

---

## Task T1.1.4: Ground Detection

**Context**: Adding ground detection system for Mech Salvager's jump mechanics. Mechs should only be able to jump when touching solid ground, preventing infinite jumping and maintaining tactical movement constraints.

**Goal**: Implement reliable ground detection using Unity's 2D physics system to enable grounded-only jumping.

**Requirements**:
- Use Physics2D.Raycast or OverlapCircle for ground detection
- LayerMask system to identify ground objects
- Cache isGrounded boolean for performance
- Debug visualization with Gizmos
- Ground detection from bottom of player collider

**Assets to Create**:
- Ground GameObject with proper layer setup
- LayerMask configuration in Project Settings
- Ground detection method in PlayerController
- Debug Gizmo visualization

**Implementation Steps**:
1. Set up Ground layer:
   - Go to Edit → Project Settings → Tags and Layers
   - Create new layer called "Ground" (layer 8)
2. Create ground GameObject:
   - Create empty GameObject, name it "Ground"
   - Add BoxCollider2D, scale it to create a platform (e.g., scale: 10, 1, 1)
   - Set GameObject layer to "Ground"
   - Position below Player spawn point
3. Update PlayerController.cs:
   - Add field: `[SerializeField] LayerMask groundLayerMask = 1 << 8;` (layer 8 for Ground)
   - Add field: `[SerializeField] float groundCheckDistance = 0.1f;`
   - Add field: `bool isGrounded;`
   - Add field: `[SerializeField] Transform groundCheckPoint;` (we'll use player's position)
4. Add CheckGrounded method:
   ```csharp
   private void CheckGrounded()
   {
       Vector2 raycastOrigin = transform.position + Vector3.down * 0.5f;
       RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, groundCheckDistance, groundLayerMask);
       isGrounded = hit.collider != null;
   }
   ```
5. Call CheckGrounded() in Update() method
6. Add OnDrawGizmos for debug visualization:
   ```csharp
   private void OnDrawGizmos()
   {
       Gizmos.color = isGrounded ? Color.green : Color.red;
       Vector3 rayStart = transform.position + Vector3.down * 0.5f;
       Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
   }
   ```

**Validation**:
- Player shows green debug ray when on ground, red when airborne
- isGrounded boolean correctly reflects ground contact state
- Ground detection works when player is stationary and moving
- Ray casts from bottom of player collider
- Ground GameObject has proper layer assignment
- No false positives when jumping or falling

**Next**: This enables the jump mechanics to be restricted to grounded-only activation.

---

## Task T1.1.5: Jump Mechanics

**Context**: Implementing jump mechanics for Mech Salvager that respects ground detection. Mechs should have weighty, deliberate jumps that feel powerful but not floaty, maintaining the tactical platformer feel.

**Goal**: Add jump functionality that only works when grounded and applies appropriate upward force to the Rigidbody2D.

**Requirements**:
- Jump only when isGrounded is true AND jump input is pressed
- Apply upward force to Rigidbody2D for physics-based jumping
- Tunable jump force parameter
- Prevent double jumping or air jumping
- Immediate input response for tight controls

**Assets to Create**:
- Jump logic in PlayerController.cs
- SerializeField jump force parameter
- Input handling for jump button

**Implementation Steps**:
1. Open PlayerController.cs
2. Add jump parameter:
   - `[SerializeField] float jumpForce = 10f;`
3. Add jump input reading in Update():
   ```csharp
   bool jumpPressed = playerInput.Player.Jump.WasPressedThisFrame();
   ```
4. Add jump logic in Update() (after CheckGrounded call):
   ```csharp
   if (jumpPressed && isGrounded)
   {
       rb.velocity = new Vector2(rb.velocity.x, jumpForce);
   }
   ```
5. Optional: Add jump input buffering for better feel:
   - Add field: `float jumpInputTime = 0f;`
   - If jump pressed: `jumpInputTime = 0.1f;` (100ms buffer)
   - In FixedUpdate: decrease jumpInputTime by Time.fixedDeltaTime
   - Check `jumpInputTime > 0f` instead of just jumpPressed
6. Ensure jump doesn't interfere with horizontal movement
7. Test and tune jumpForce value for good feel (start with 10, adjust as needed)

**Validation**:
- Jump only works when player is on ground
- Pressing jump while airborne does nothing
- Jump feels responsive (no input delay)
- Jump arc feels weighty, not floaty
- Horizontal movement continues normally during jump
- Player lands and can jump again immediately
- Jump force is easily tunable in Inspector

**Next**: This completes basic locomotion, enabling the dash system to build on existing movement mechanics.

---

## Task T1.1.6: Dash System Foundation

**Context**: Adding tactical dash mechanics to Mech Salvager's movement system. Dash should be a limited resource that provides quick directional movement, adding tactical depth by consuming energy and having cooldown restrictions.

**Goal**: Implement dash system with energy cost, directional movement, and resource management constraints.

**Requirements**:
- Energy system with current/max values and consumption
- Dash applies directional force based on last movement input
- Energy cost prevents spam-dashing
- Energy regeneration over time
- Cooldown system to prevent rapid successive dashes
- Dash works in air and on ground

**Assets to Create**:
- Energy system variables in PlayerController
- Dash logic and energy management methods
- CanDash() validation method
- Energy regeneration system

**Implementation Steps**:
1. Add energy system fields to PlayerController.cs:
   ```csharp
   [Header("Energy System")]
   [SerializeField] float maxEnergy = 100f;
   [SerializeField] float currentEnergy = 100f;
   [SerializeField] float dashEnergyCost = 25f;
   [SerializeField] float energyRegenRate = 20f; // energy per second
   
   [Header("Dash System")]
   [SerializeField] float dashForce = 15f;
   [SerializeField] float dashCooldown = 0.5f;
   private float dashCooldownTimer = 0f;
   private Vector2 lastMoveDirection = Vector2.right; // default dash direction
   ```

2. Add energy management methods:
   ```csharp
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
   ```

3. Update Update() method:
   - Store last movement direction: `if (horizontal != 0) lastMoveDirection = horizontal > 0 ? Vector2.right : Vector2.left;`
   - Handle dash input: `bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift);`
   - Add dash logic: 
   ```csharp
   if (dashPressed && CanDash())
   {
       float dashDirection = horizontal != 0 ? horizontal : lastMoveDirection.x;
       rb.velocity = new Vector2(dashDirection * dashForce, rb.velocity.y);
       ConsumeEnergy(dashEnergyCost);
       dashCooldownTimer = dashCooldown;
   }
   ```
   - Call RegenerateEnergy()
   - Update cooldown: `dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);`

4. Add debug display in OnDrawGizmos:
   ```csharp
   // Show energy as text in scene view (optional)
   UnityEditor.Handles.Label(transform.position + Vector3.up, $"Energy: {currentEnergy:F0}");
   ```

**Validation**:
- Dash works when energy is available and cooldown is ready
- Dash consumes energy and triggers cooldown
- Energy regenerates over time back to maximum
- Dash direction follows current or last movement input
- Multiple dash attempts respect energy and cooldown limits
- Dash force provides satisfying burst of speed
- Energy values are visible and tunable in Inspector

**Next**: This completes the core movement mechanics, enabling movement parameter tuning and polish.

---

## Task T1.1.7: Movement Tuning

**Context**: Fine-tuning all movement parameters in Mech Salvager to achieve the desired "weighty but responsive" feel. This task focuses on balancing speed, forces, and energy costs to create satisfying tactical movement.

**Goal**: Optimize all movement parameters through testing and iteration, ensuring values are easily adjustable and well-documented.

**Requirements**:
- All key movement values exposed as SerializeField
- Parameter ranges and tooltips for designer clarity
- Organized Inspector layout with headers
- Documented "good feel" baseline values
- Easy testing workflow for rapid iteration

**Assets to Create**:
- Organized parameter structure in PlayerController
- Parameter documentation comments
- Baseline configuration values

**Implementation Steps**:
1. Organize PlayerController.cs parameters with headers and tooltips:
   ```csharp
   [Header("Movement Settings")]
   [SerializeField, Range(1f, 10f), Tooltip("Maximum horizontal movement speed")]
   float moveSpeed = 5f;
   
   [SerializeField, Range(5f, 20f), Tooltip("Upward force applied when jumping")]
   float jumpForce = 12f;
   
   [SerializeField, Range(10f, 25f), Tooltip("Horizontal force applied when dashing")]
   float dashForce = 18f;
   
   [Header("Physics Settings")]
   [SerializeField, Range(0.1f, 2f), Tooltip("How far to check for ground")]
   float groundCheckDistance = 0.1f;
   
   [Header("Energy System")]
   [SerializeField, Range(50f, 200f), Tooltip("Maximum energy capacity")]
   float maxEnergy = 100f;
   
   [SerializeField, Range(10f, 50f), Tooltip("Energy cost for dash")]
   float dashEnergyCost = 25f;
   
   [SerializeField, Range(10f, 50f), Tooltip("Energy regenerated per second")]
   float energyRegenRate = 20f;
   
   [SerializeField, Range(0.1f, 2f), Tooltip("Cooldown between dashes")]
   float dashCooldown = 0.5f;
   ```

2. Add parameter validation in Start():
   ```csharp
   private void Start()
   {
       // Ensure energy starts at max
       currentEnergy = maxEnergy;
       
       // Validate parameters
       if (dashEnergyCost > maxEnergy)
       {
           Debug.LogWarning("Dash energy cost exceeds max energy!");
       }
   }
   ```

3. Create testing configuration:
   - Test with "Heavy Mech" feel: Lower moveSpeed (3f), higher jumpForce (15f)
   - Test with "Agile Mech" feel: Higher moveSpeed (7f), lower jumpForce (10f)
   - Test energy constraints: Lower maxEnergy (75f), higher dashEnergyCost (35f)

4. Document baseline "good feel" values in comments:
   ```csharp
   // BASELINE VALUES (tested feel):
   // moveSpeed = 5f (responsive but not twitchy)
   // jumpForce = 12f (weighty arc, good height)
   // dashForce = 18f (satisfying burst without being overpowered)
   // dashEnergyCost = 25f (allows 4 dashes at full energy)
   // energyRegenRate = 20f (5 seconds to fully regenerate)
   ```

5. Add runtime parameter adjustment support:
   ```csharp
   [Header("Debug Controls")]
   [SerializeField] bool enableRuntimeTuning = false;
   
   private void Update()
   {
       if (enableRuntimeTuning && Application.isEditor)
       {
           // Allow runtime adjustment via inspector changes
           // Values will be applied immediately for testing
       }
       // ... existing update code
   }
   ```

**Validation**:
- All movement parameters visible and clearly labeled in Inspector
- Parameter ranges prevent obviously broken values
- Tooltips explain what each parameter affects
- Movement feels "weighty but responsive" with baseline values
- Easy to test different parameter combinations
- No compiler warnings or errors
- Parameters save/load correctly in scene

**Next**: This enables comprehensive integration testing of all movement systems working together.

---

## Task T1.1.8: Integration Testing

**Context**: Final validation of all movement systems working together in Mech Salvager. This comprehensive testing ensures all mechanics integrate properly and handles edge cases that could break the player experience.

**Goal**: Create thorough test scenarios and verify all movement systems work cohesively without conflicts or bugs.

**Requirements**:
- Test level with various platform configurations
- Comprehensive test cases covering normal and edge case scenarios
- Validation of energy system integration
- Performance verification
- Documentation of any issues or limitations

**Assets to Create**:
- Test scene with multiple platform configurations
- Comprehensive test checklist
- Edge case validation scenarios
- Performance baseline documentation

**Implementation Steps**:
1. Create comprehensive test scene:
   - Rename current scene to "MovementTestScene"
   - Create platform layout:
     - Ground platform (wide, for basic movement)
     - Elevated platform (requires jumping)
     - Gap requiring dash to cross
     - Wall for collision testing
     - Narrow platforms for precision testing

2. Set up test environment:
   - Position Player at consistent spawn point
   - Add multiple Ground GameObjects at different heights
   - Create gap of exactly (dashForce / moveSpeed) units width
   - Add walls and boundaries for collision testing

3. Execute systematic test cases:
   
   **Basic Movement Tests:**
   - Walk left/right across entire ground platform
   - Change direction rapidly (test for stuttering)
   - Walk into walls (ensure player stops cleanly)
   
   **Jump System Tests:**
   - Jump from ground (should work)
   - Attempt jump while airborne (should fail)
   - Jump while moving horizontally (should maintain horizontal movement)
   - Land on different height platforms
   
   **Dash System Tests:**
   - Dash with full energy (should work)
   - Attempt multiple rapid dashes (should respect cooldown)
   - Dash with insufficient energy (should fail)
   - Dash in different directions (up, down, diagonal)
   - Dash while jumping (should work)
   
   **Energy System Tests:**
   - Drain energy completely through dashing
   - Wait for energy regeneration (time how long to full)
   - Dash at various energy levels
   
   **Integration Tests:**
   - Complete movement chain: walk → jump → dash → land
   - Dash across gap while jumping
   - Rapid direction changes during complex movement
   - Energy management during extended gameplay

4. Create automated validation script (optional):
   ```csharp
   public class MovementTestValidator : MonoBehaviour
   {
       public PlayerController player;
       
       [ContextMenu("Run All Tests")]
       public void RunAllTests()
       {
           StartCoroutine(AutomatedTestSequence());
       }
       
       private IEnumerator AutomatedTestSequence()
       {
           // Test basic movement
           // Test jump functionality  
           // Test dash system
           // Report results to console
       }
   }
   ```

5. Document test results:
   - Record baseline performance metrics
   - Note any bugs or unexpected behaviors
   - Document "feel" assessment
   - Create issue list for future improvements

**Validation**:
- Player can navigate entire test level using all movement mechanics
- No physics glitches, clipping, or stuck states
- Energy system properly gates dash usage
- All movement combinations work smoothly together
- Frame rate remains stable during complex movement
- Input feels responsive across all mechanics
- Ground detection works reliably on all platform types
- No console errors during extended testing

**Next**: Movement system is complete and ready for Phase 2 (Enemy AI & Movement) development.

---

## Summary

These 8 sequential prompts break down M1.1 into actionable, testable chunks that VS Code Copilot can execute systematically. Each prompt:

- **Builds on the previous**: Clear progression from setup → implementation → testing
- **Is completely actionable**: Specific Unity steps, exact component settings, concrete code
- **Has clear validation**: Testable success criteria for each step
- **Maintains context**: References overall goal while focusing on specific task
- **Specifies assets**: Exact file names, GameObject hierarchies, component configurations

This approach transforms the milestone table into a step-by-step implementation guide that an AI agent can follow to build the complete movement system.