using UnityEngine;

/// <summary>
/// MCP Test GameObject - Demonstrates Unity MCP integration capabilities
/// This script showcases various Unity features that can be controlled via MCP
/// </summary>
public class MCPTestCube : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField, Range(0f, 360f), Tooltip("Rotation speed in degrees per second")]
    private float rotationSpeed = 45f;
    
    [SerializeField, Tooltip("Rotation axis (X, Y, Z)")]
    private Vector3 rotationAxis = Vector3.up;
    
    [Header("Color Animation")]
    [SerializeField, Tooltip("Enable color cycling")]
    private bool enableColorCycling = true;
    
    [SerializeField, Range(0.1f, 5f), Tooltip("Color change speed")]
    private float colorSpeed = 1f;
    
    [Header("Movement")]
    [SerializeField, Tooltip("Enable bobbing movement")]
    private bool enableBobbing = false;
    
    [SerializeField, Range(0.1f, 2f), Tooltip("Bobbing amplitude")]
    private float bobbingAmplitude = 0.5f;
    
    [SerializeField, Range(0.1f, 3f), Tooltip("Bobbing frequency")]
    private float bobbingFrequency = 1f;
    
    [Header("Debug")]
    [SerializeField, Tooltip("Enable debug logging")]
    private bool enableDebugLogs = true;
    
    // Private variables
    private MeshRenderer meshRenderer;
    private Material materialInstance;
    private Vector3 startPosition;
    private float timeOffset;
    
    void Start()
    {
        // Get components
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Create material instance for color changes
        if (meshRenderer != null)
        {
            materialInstance = new Material(meshRenderer.material);
            meshRenderer.material = materialInstance;
        }
        
        // Store starting position for bobbing
        startPosition = transform.position;
        
        // Random time offset for varied animation
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
        
        if (enableDebugLogs)
        {
            Debug.Log($"MCP Test Cube '{gameObject.name}' initialized successfully!");
            Debug.Log($"Position: {transform.position}, Rotation Speed: {rotationSpeed}°/s");
        }
    }
    
    void Update()
    {
        // Handle rotation
        if (rotationSpeed > 0f)
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        }
        
        // Handle color cycling
        if (enableColorCycling && materialInstance != null)
        {
            float hue = (Time.time * colorSpeed + timeOffset) % 1f;
            Color newColor = Color.HSVToRGB(hue, 0.8f, 1f);
            materialInstance.color = newColor;
        }
        
        // Handle bobbing movement
        if (enableBobbing)
        {
            float bobOffset = Mathf.Sin(Time.time * bobbingFrequency + timeOffset) * bobbingAmplitude;
            transform.position = startPosition + Vector3.up * bobOffset;
        }
    }
    
    /// <summary>
    /// Public method to test MCP script interaction
    /// </summary>
    public void TestMCPInteraction()
    {
        Debug.Log($"MCP Test: Interaction method called on {gameObject.name}!");
        
        // Flash the cube white briefly
        if (materialInstance != null)
        {
            StartCoroutine(FlashWhite());
        }
    }
    
    /// <summary>
    /// Toggle rotation on/off
    /// </summary>
    public void ToggleRotation()
    {
        rotationSpeed = rotationSpeed > 0f ? 0f : 45f;
        Debug.Log($"MCP Test: Rotation {(rotationSpeed > 0f ? "enabled" : "disabled")} on {gameObject.name}");
    }
    
    /// <summary>
    /// Set rotation speed via MCP
    /// </summary>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Clamp(speed, 0f, 360f);
        Debug.Log($"MCP Test: Rotation speed set to {rotationSpeed}°/s on {gameObject.name}");
    }
    
    /// <summary>
    /// Flash white for visual feedback
    /// </summary>
    private System.Collections.IEnumerator FlashWhite()
    {
        Color originalColor = materialInstance.color;
        materialInstance.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        materialInstance.color = originalColor;
    }
    
    void OnDestroy()
    {
        // Clean up material instance
        if (materialInstance != null)
        {
            DestroyImmediate(materialInstance);
        }
    }
    
    /// <summary>
    /// Display current status in scene view
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw rotation axis
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + rotationAxis.normalized * 2f);
        
        // Draw bobbing range if enabled
        if (enableBobbing)
        {
            Gizmos.color = Color.cyan;
            Vector3 pos = Application.isPlaying ? startPosition : transform.position;
            Gizmos.DrawWireSphere(pos + Vector3.up * bobbingAmplitude, 0.1f);
            Gizmos.DrawWireSphere(pos - Vector3.up * bobbingAmplitude, 0.1f);
        }
        
        #if UNITY_EDITOR
        // Show status text in scene view
        string status = $"MCP Test Cube\nRotation: {rotationSpeed}°/s\nColor Cycling: {enableColorCycling}\nBobbing: {enableBobbing}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, status);
        #endif
    }
}