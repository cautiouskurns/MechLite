using UnityEngine;

/// <summary>
/// Test script to verify Unity's Legacy Input Manager configuration for Mech Salvager
/// This script validates basic movement input (Horizontal axis, Space for jump, LeftShift for dash)
/// </summary>
public class InputTester : MonoBehaviour
{
    [Header("Input Test Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private float logInterval = 0.5f; // Log every half second to avoid spam
    
    private float lastLogTime;
    
    void Start()
    {
        if (enableDebugLogs)
        {
            Debug.Log("=== INPUT TESTER STARTED ===");
            Debug.Log("Testing Unity Legacy Input Manager configuration:");
            Debug.Log("- Horizontal Axis (A/D keys, Left/Right arrows)");
            Debug.Log("- Vertical Axis (W/S keys, Up/Down arrows)");
            Debug.Log("- Jump (Space key)");
            Debug.Log("- Dash (Left Shift key)");
            Debug.Log("Press keys to test input registration...");
        }
    }
    
    void Update()
    {
        if (!enableDebugLogs) return;
        
        // Test movement axes
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Test action keys
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift);
        bool jumpHeld = Input.GetKey(KeyCode.Space);
        bool dashHeld = Input.GetKey(KeyCode.LeftShift);
        
        // Log input values periodically or when keys are pressed
        bool shouldLog = Time.time - lastLogTime > logInterval;
        bool hasInput = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f || 
                       jumpPressed || dashPressed || jumpHeld || dashHeld;
        
        if (shouldLog && hasInput)
        {
            LogInputState(horizontal, vertical, jumpPressed, dashPressed, jumpHeld, dashHeld);
            lastLogTime = Time.time;
        }
        
        // Always log key presses immediately
        if (jumpPressed)
        {
            Debug.Log("✓ JUMP key pressed (Space) - GetKeyDown working");
        }
        
        if (dashPressed)
        {
            Debug.Log("✓ DASH key pressed (LeftShift) - GetKeyDown working");
        }
    }
    
    private void LogInputState(float horizontal, float vertical, bool jumpPressed, bool dashPressed, bool jumpHeld, bool dashHeld)
    {
        Debug.Log($"INPUT STATE - Horizontal: {horizontal:F2}, Vertical: {vertical:F2}, " +
                 $"Jump: {(jumpHeld ? "HELD" : "released")}, Dash: {(dashHeld ? "HELD" : "released")}");
    }
    
    void OnGUI()
    {
        if (!enableDebugLogs) return;
        
        // Create a simple on-screen display for input values
        GUI.Box(new Rect(10, 10, 300, 120), "Input Tester - Legacy Input Manager");
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool jumpHeld = Input.GetKey(KeyCode.Space);
        bool dashHeld = Input.GetKey(KeyCode.LeftShift);
        
        GUI.Label(new Rect(20, 35, 280, 20), $"Horizontal Axis: {horizontal:F2}");
        GUI.Label(new Rect(20, 55, 280, 20), $"Vertical Axis: {vertical:F2}");
        GUI.Label(new Rect(20, 75, 280, 20), $"Jump (Space): {(jumpHeld ? "PRESSED" : "released")}");
        GUI.Label(new Rect(20, 95, 280, 20), $"Dash (LeftShift): {(dashHeld ? "PRESSED" : "released")}");
    }
}