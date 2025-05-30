using UnityEngine;

/// <summary>
/// Controls camera following behavior for the player in Mech Salvager
/// Implements direct position following with configurable offset
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target Following")]
    [SerializeField, Tooltip("The player GameObject to follow")]
    private Transform player;
    
    [Header("Camera Settings")]
    [SerializeField, Tooltip("3D offset from player position (X, Y, Z)")]
    private Vector3 offset = new Vector3(0f, 2f, -10f);
    
    [Header("Debug")]
    [SerializeField, Tooltip("Enable debug logging for camera behavior")]
    private bool enableDebugLogs = false;
    
    private void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
                if (enableDebugLogs)
                {
                    Debug.Log($"CameraController: Auto-found player at {player.position}");
                }
            }
            else
            {
                Debug.LogError("CameraController: No player found! Please assign player transform or add 'Player' tag to player GameObject.");
            }
        }
        
        // Initial position setup
        if (player != null)
        {
            transform.position = player.position + offset;
            
            if (enableDebugLogs)
            {
                Debug.Log($"CameraController: Initial setup complete. Camera at {transform.position}, Player at {player.position}, Offset: {offset}");
            }
        }
    }
    
    /// <summary>
    /// LateUpdate ensures camera updates after all player movement has been processed
    /// This prevents camera lag behind player movement
    /// </summary>
    private void LateUpdate()
    {
        // Safety check - ensure we have a valid player reference
        if (player == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("CameraController: Player reference is null in LateUpdate!");
            }
            return;
        }
        
        // Direct position following - player position + offset
        Vector3 targetPosition = player.position + offset;
        transform.position = targetPosition;
        
        // Debug logging for camera movement (only log when position changes significantly)
        if (enableDebugLogs && Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            Debug.Log($"CameraController: Following player - Camera: {transform.position}, Player: {player.position}");
        }
    }
    
    /// <summary>
    /// Update the camera offset at runtime (useful for testing different camera angles)
    /// </summary>
    /// <param name="newOffset">New offset vector to apply</param>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        
        if (enableDebugLogs)
        {
            Debug.Log($"CameraController: Offset changed to {offset}");
        }
    }
    
    /// <summary>
    /// Get the current camera offset
    /// </summary>
    /// <returns>Current offset vector</returns>
    public Vector3 GetOffset()
    {
        return offset;
    }
    
    /// <summary>
    /// Set a new player target for the camera to follow
    /// </summary>
    /// <param name="newPlayer">New player transform to follow</param>
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        
        if (enableDebugLogs)
        {
            Debug.Log($"CameraController: Player target changed to {(player != null ? player.name : "null")}");
        }
    }
    
    /// <summary>
    /// Validate that the camera setup is correct
    /// </summary>
    private void OnValidate()
    {
        // Ensure Z offset is negative for proper 2D camera positioning
        if (offset.z >= 0f)
        {
            Debug.LogWarning("CameraController: Z offset should be negative (e.g., -10) for proper 2D camera positioning!");
        }
    }
    
    /// <summary>
    /// Visualize camera following in Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (player == null) return;
        
        // Draw line from camera to player
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, player.position);
        
        // Draw target position (player + offset)
        Vector3 targetPos = player.position + offset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPos, 0.5f);
        
        #if UNITY_EDITOR
        // Show offset info in scene view
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
            $"Camera Follow\nOffset: {offset}\nTarget: {(player != null ? player.name : "None")}");
        #endif
    }
}