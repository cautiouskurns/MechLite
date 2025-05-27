using UnityEngine;

/// <summary>
/// Simple test script to verify MCP (Model Context Protocol) integration
/// This script demonstrates basic Unity functionality that can be controlled via MCP
/// </summary>
public class MCPTestScript : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private float rotationSpeed = 45f; // Degrees per second
    [SerializeField] private float bounceHeight = 1f;
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private bool enableBouncing = true;
    
    [Header("Test State")]
    [SerializeField] private int testCounter = 0;
    [SerializeField] private bool isActive = true;
    
    // Public properties for MCP access
    public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
    public float BounceHeight { get => bounceHeight; set => bounceHeight = value; }
    public float BounceSpeed { get => bounceSpeed; set => bounceSpeed = value; }
    public bool EnableRotation { get => enableRotation; set => enableRotation = value; }
    public bool EnableBouncing { get => enableBouncing; set => enableBouncing = value; }
    public int TestCounter { get => testCounter; set => testCounter = value; }
    public bool IsActive { get => isActive; set => isActive = value; }
    
    private Vector3 startPosition;
    private float time = 0f;
    
    void Start()
    {
        startPosition = transform.position;
        Debug.Log($"MCPTestScript: Started on GameObject '{gameObject.name}' at position {startPosition}");
    }
    
    void Update()
    {
        if (!isActive) return;
        
        time += Time.deltaTime;
        
        // Handle rotation
        if (enableRotation)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        
        // Handle bouncing
        if (enableBouncing)
        {
            float yOffset = Mathf.Sin(time * bounceSpeed) * bounceHeight;
            transform.position = startPosition + Vector3.up * yOffset;
        }
        
        // Increment test counter every second
        if (Time.frameCount % 60 == 0)
        {
            testCounter++;
        }
    }
    
    // Public methods for MCP testing
    public void ResetPosition()
    {
        transform.position = startPosition;
        Debug.Log("MCPTestScript: Position reset to start position");
    }
    
    public void ChangeColor(float r, float g, float b, float a = 1f)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(r, g, b, a);
            Debug.Log($"MCPTestScript: Color changed to ({r}, {g}, {b}, {a})");
        }
    }
    
    public void LogStatus()
    {
        Debug.Log($"MCPTestScript Status - Counter: {testCounter}, Active: {isActive}, " +
                 $"Rotation: {enableRotation}, Bouncing: {enableBouncing}, Position: {transform.position}");
    }
    
    public void ToggleActive()
    {
        isActive = !isActive;
        Debug.Log($"MCPTestScript: Active state toggled to {isActive}");
    }
}