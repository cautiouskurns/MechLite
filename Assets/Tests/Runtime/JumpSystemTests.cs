using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MechLite.Movement;
using MechLite.Configuration;

/// <summary>
/// Simple Unity tests for JumpSystem component
/// Tests basic "grounded + space = jump" functionality using real Unity components
/// </summary>
public class JumpSystemTests
{
    private GameObject testPlayer;
    private JumpSystem jumpSystem;
    private MovementController movementController;
    private GroundDetector groundDetector;
    private MovementConfigSO movementConfig;
    private PhysicsConfigSO physicsConfig;

    [SetUp]
    public void SetUp()
    {
        // Create test configurations
        movementConfig = ScriptableObject.CreateInstance<MovementConfigSO>();
        movementConfig.jumpForce = 10f;
        movementConfig.moveSpeed = 5f;
        movementConfig.acceleration = 10f;
        movementConfig.deceleration = 10f;
        movementConfig.coyoteTime = 0.15f;
        movementConfig.jumpBufferTime = 0.1f;
        movementConfig.airControlStrength = 0.1f;
        movementConfig.clampGroundedVelocity = true;

        physicsConfig = ScriptableObject.CreateInstance<PhysicsConfigSO>();
        physicsConfig.groundLayerMask = LayerMask.GetMask("Ground");
        physicsConfig.groundCheckDistance = 0.1f;
        physicsConfig.groundCheckOffset = Vector2.zero;

        // Create test player GameObject with all required components
        testPlayer = new GameObject("TestPlayer");
        testPlayer.transform.position = Vector3.zero;

        // Add required components
        var rb2d = testPlayer.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 1f;

        var collider = testPlayer.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        var spriteRenderer = testPlayer.AddComponent<SpriteRenderer>();

        // Expect all the initialization errors since configs aren't assigned in inspector during Awake()
        LogAssert.Expect(LogType.Error, "MovementController: MovementConfigSO is not assigned!");
        LogAssert.Expect(LogType.Error, "GroundDetector: PhysicsConfigSO is not assigned!");
        LogAssert.Expect(LogType.Error, "GroundDetector: MovementConfigSO is not assigned!");
        LogAssert.Expect(LogType.Error, "JumpSystem: MovementConfigSO is not assigned!");
        LogAssert.Expect(LogType.Error, "JumpSystem: IMovable component not found!");
        LogAssert.Expect(LogType.Error, "JumpSystem: IGroundDetector component not found!");

        // Add MovementController and assign config
        movementController = testPlayer.AddComponent<MovementController>();
        var movementConfigField = typeof(MovementController).GetField("movementConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        movementConfigField?.SetValue(movementController, movementConfig);

        // Add GroundDetector and assign config  
        groundDetector = testPlayer.AddComponent<GroundDetector>();
        var physicsConfigField = typeof(GroundDetector).GetField("physicsConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        physicsConfigField?.SetValue(groundDetector, physicsConfig);
        var groundMovementConfigField = typeof(GroundDetector).GetField("movementConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        groundMovementConfigField?.SetValue(groundDetector, movementConfig);

        // Add JumpSystem and assign config
        jumpSystem = testPlayer.AddComponent<JumpSystem>();
        var jumpConfigField = typeof(JumpSystem).GetField("movementConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        jumpConfigField?.SetValue(jumpSystem, movementConfig);

        // Initialize JumpSystem with required dependencies
        jumpSystem.Initialize(physicsConfig, null, movementController, groundDetector, null);
    }

    [TearDown]
    public void TearDown()
    {
        if (testPlayer != null)
        {
            Object.DestroyImmediate(testPlayer);
        }
        if (movementConfig != null)
        {
            Object.DestroyImmediate(movementConfig);
        }
        if (physicsConfig != null)
        {
            Object.DestroyImmediate(physicsConfig);
        }
    }

    [Test]
    public void JumpSystem_Initialize_ComponentsExist()
    {
        // Arrange & Act - already done in SetUp

        // Assert
        Assert.IsNotNull(jumpSystem, "JumpSystem component should exist");
        Assert.IsNotNull(movementController, "MovementController component should exist");
        Assert.IsNotNull(groundDetector, "GroundDetector component should exist");
        Assert.IsNotNull(movementConfig, "MovementConfig should be assigned");
    }

    [Test]
    public void CanJump_WhenGrounded_ReturnsTrue()
    {
        // Arrange - simulate being grounded by setting the ground state directly
        var isGroundedField = typeof(GroundDetector).GetField("isGrounded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        isGroundedField?.SetValue(groundDetector, true);

        // Act
        bool canJump = jumpSystem.CanJump();

        // Assert
        Assert.IsTrue(canJump, "Should be able to jump when grounded");
    }

    [Test]
    public void CanJump_WhenNotGrounded_ReturnsFalse()
    {
        // Arrange - simulate being airborne by setting the ground state directly
        var isGroundedField = typeof(GroundDetector).GetField("isGrounded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        isGroundedField?.SetValue(groundDetector, false);

        // Act
        bool canJump = jumpSystem.CanJump();

        // Assert
        Assert.IsFalse(canJump, "Should NOT be able to jump when not grounded");
    }

    [Test]
    public void ProcessJumpInput_GroundedAndPressed_ExecutesJump()
    {
        // Arrange - simulate being grounded
        var isGroundedField = typeof(GroundDetector).GetField("isGrounded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        isGroundedField?.SetValue(groundDetector, true);

        var rb2d = testPlayer.GetComponent<Rigidbody2D>();
        float initialYVelocity = rb2d.linearVelocity.y;

        // Act
        jumpSystem.ProcessJumpInput(true);

        // Assert
        Assert.Greater(rb2d.linearVelocity.y, initialYVelocity, "Y velocity should increase after jump");
        Assert.AreEqual(movementConfig.jumpForce, rb2d.linearVelocity.y, 0.01f, "Y velocity should equal jump force");
    }

    [Test]
    public void ProcessJumpInput_NotGroundedAndPressed_DoesNotExecuteJump()
    {
        // Arrange - simulate being airborne
        var isGroundedField = typeof(GroundDetector).GetField("isGrounded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        isGroundedField?.SetValue(groundDetector, false);

        var rb2d = testPlayer.GetComponent<Rigidbody2D>();
        float initialYVelocity = rb2d.linearVelocity.y;

        // Act
        jumpSystem.ProcessJumpInput(true);

        // Assert
        Assert.AreEqual(initialYVelocity, rb2d.linearVelocity.y, 0.01f, "Y velocity should remain unchanged when not grounded");
    }

    [Test]
    public void TryExecuteJump_WhenGrounded_ExecutesJump()
    {
        // Arrange - simulate being grounded
        var isGroundedField = typeof(GroundDetector).GetField("isGrounded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        isGroundedField?.SetValue(groundDetector, true);

        var rb2d = testPlayer.GetComponent<Rigidbody2D>();
        float initialYVelocity = rb2d.linearVelocity.y;

        // Act
        jumpSystem.TryExecuteJump();

        // Assert
        Assert.Greater(rb2d.linearVelocity.y, initialYVelocity, "Y velocity should increase after TryExecuteJump");
        Assert.AreEqual(movementConfig.jumpForce, rb2d.linearVelocity.y, 0.01f, "Y velocity should equal jump force");
    }
}
