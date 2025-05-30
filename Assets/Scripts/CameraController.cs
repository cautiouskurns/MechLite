using UnityEngine;
using MechLite.Configuration;

public class CameraController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private CameraConfigSO cameraConfig;
    
    [Header("Target Following")]
    [SerializeField] private Transform player;
    
    // Smoothing variables
    private Vector3 dampVelocity;
    
    // Look-ahead variables
    private Vector3 lastPlayerPosition;
    private Vector3 currentLookAhead;
    private Vector3 playerVelocity;
    
    public void Initialize(CameraConfigSO config)
    {
        cameraConfig = config;
    }
    
    private void Start()
    {
        if (cameraConfig == null)
        {
            Debug.LogError("CameraController: CameraConfigSO is not assigned!");
            return;
        }
        
        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                player = playerGO.transform;
        }
        
        if (player != null)
        {
            // Initialize look-ahead tracking
            lastPlayerPosition = player.position;
            currentLookAhead = Vector3.zero;
            playerVelocity = Vector3.zero;
            
            transform.position = player.position + cameraConfig.offset;
            ApplyBounds();
            if (cameraConfig.enableDebugLogs)
                Debug.Log($"CameraController: Initialized with config '{cameraConfig.name}'");
        }
    }
    
    private void LateUpdate()
    {
        if (player == null || cameraConfig == null) return;
        
        Vector3 cameraWorldPos = transform.position - cameraConfig.offset;
        float distance = Vector3.Distance(cameraWorldPos, player.position);
        
        if (distance < cameraConfig.deadZoneSize) return;
        
        // Calculate look-ahead offset
        Vector3 lookAhead = CalculateLookAhead();
        
        Vector3 targetPos = new Vector3(
            cameraConfig.followX ? player.position.x : cameraWorldPos.x,
            cameraConfig.followY ? player.position.y : cameraWorldPos.y,
            cameraConfig.followZ ? player.position.z : cameraWorldPos.z
        ) + cameraConfig.offset + lookAhead;
        
        if (cameraConfig.useSmoothDamp)
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref dampVelocity, cameraConfig.smoothTime);
        else
            transform.position = Vector3.Lerp(transform.position, targetPos, cameraConfig.followSpeed * Time.deltaTime);
        
        ApplyBounds();
    }
    
    /// <summary>
    /// Calculate look-ahead offset based on player velocity
    /// </summary>
    private Vector3 CalculateLookAhead()
    {
        if (!cameraConfig.useLookAhead || cameraConfig.lookAheadDistance <= 0f)
        {
            currentLookAhead = Vector3.Lerp(currentLookAhead, Vector3.zero, cameraConfig.lookAheadSpeed * Time.deltaTime);
            return currentLookAhead;
        }
        
        // Calculate player velocity
        playerVelocity = (player.position - lastPlayerPosition) / Time.deltaTime;
        lastPlayerPosition = player.position;
        
        // Only apply look-ahead if player is moving fast enough
        Vector3 targetLookAhead = Vector3.zero;
        if (playerVelocity.magnitude > cameraConfig.velocityThreshold)
        {
            // Create look-ahead vector in movement direction
            Vector3 lookDirection = playerVelocity.normalized;
            
            // Respect axis constraints for look-ahead
            if (!cameraConfig.followX) lookDirection.x = 0f;
            if (!cameraConfig.followY) lookDirection.y = 0f;
            if (!cameraConfig.followZ) lookDirection.z = 0f;
            
            targetLookAhead = lookDirection * cameraConfig.lookAheadDistance;
        }
        
        // Smoothly transition to target look-ahead
        currentLookAhead = Vector3.Lerp(currentLookAhead, targetLookAhead, cameraConfig.lookAheadSpeed * Time.deltaTime);
        
        return currentLookAhead;
    }
    
    private void ApplyBounds()
    {
        if (cameraConfig == null || !cameraConfig.useBounds) return;
        
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, cameraConfig.boundsMin.x, cameraConfig.boundsMax.x);
        pos.y = Mathf.Clamp(pos.y, cameraConfig.boundsMin.y, cameraConfig.boundsMax.y);
        transform.position = pos;
    }
    
    public void SetConfiguration(CameraConfigSO newConfig)
    {
        cameraConfig = newConfig;
        dampVelocity = Vector3.zero;
        currentLookAhead = Vector3.zero; // Reset look-ahead when changing configs
        ApplyBounds();
    }
    
    public CameraConfigSO GetConfiguration() { return cameraConfig; }
    public void SetPlayer(Transform newPlayer) 
    { 
        player = newPlayer;
        if (player != null)
        {
            lastPlayerPosition = player.position;
            currentLookAhead = Vector3.zero;
        }
    }
    
    public bool IsPlayerInDeadZone()
    {
        if (player == null || cameraConfig == null) return false;
        Vector3 cameraWorldPos = transform.position - cameraConfig.offset;
        return Vector3.Distance(cameraWorldPos, player.position) < cameraConfig.deadZoneSize;
    }
    
    public bool IsCameraAtBounds()
    {
        if (cameraConfig == null || !cameraConfig.useBounds) return false;
        Vector3 pos = transform.position;
        return Mathf.Approximately(pos.x, cameraConfig.boundsMin.x) ||
               Mathf.Approximately(pos.x, cameraConfig.boundsMax.x) ||
               Mathf.Approximately(pos.y, cameraConfig.boundsMin.y) ||
               Mathf.Approximately(pos.y, cameraConfig.boundsMax.y);
    }
    
    /// <summary>
    /// Get current player velocity for external systems
    /// </summary>
    public Vector3 GetPlayerVelocity()
    {
        return playerVelocity;
    }
    
    /// <summary>
    /// Get current look-ahead offset
    /// </summary>
    public Vector3 GetCurrentLookAhead()
    {
        return currentLookAhead;
    }
    
    /// <summary>
    /// Check if look-ahead is currently active
    /// </summary>
    public bool IsLookAheadActive()
    {
        return cameraConfig != null && cameraConfig.useLookAhead && currentLookAhead.magnitude > 0.1f;
    }
    
    public void SnapToTarget()
    {
        if (player != null && cameraConfig != null)
        {
            transform.position = player.position + cameraConfig.offset;
            ApplyBounds();
            dampVelocity = Vector3.zero;
            currentLookAhead = Vector3.zero; // Reset look-ahead on snap
            lastPlayerPosition = player.position;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (cameraConfig == null) return;
        
        // Player connection line
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, player.position);
        }
        
        // Deadzone visualization
        if (player != null && cameraConfig.showDeadZoneGizmo && cameraConfig.deadZoneSize > 0f)
        {
            Vector3 deadZoneCenter = transform.position - cameraConfig.offset;
            bool playerInDeadZone = IsPlayerInDeadZone();
            
            Gizmos.color = playerInDeadZone ? Color.green : Color.red;
            Gizmos.DrawWireSphere(deadZoneCenter, cameraConfig.deadZoneSize);
        }
        
        // Look-ahead visualization
        if (player != null && cameraConfig.showLookAheadGizmo && cameraConfig.useLookAhead)
        {
            Vector3 cameraWorldPos = transform.position - cameraConfig.offset;
            Vector3 lookAheadTarget = cameraWorldPos + currentLookAhead;
            
            // Draw look-ahead vector
            if (currentLookAhead.magnitude > 0.1f)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(cameraWorldPos, lookAheadTarget);
                Gizmos.DrawWireSphere(lookAheadTarget, 0.2f);
            }
            
            // Draw velocity vector (scaled for visibility)
            if (playerVelocity.magnitude > cameraConfig.velocityThreshold)
            {
                Vector3 velocityEnd = player.position + playerVelocity * 0.5f; // Scale for visibility
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(player.position, velocityEnd);
                Gizmos.DrawWireSphere(velocityEnd, 0.1f);
            }
        }
        
        // Bounds visualization
        if (cameraConfig.useBounds)
        {
            Vector3 boundsCenter = new Vector3(
                (cameraConfig.boundsMin.x + cameraConfig.boundsMax.x) / 2f,
                (cameraConfig.boundsMin.y + cameraConfig.boundsMax.y) / 2f,
                transform.position.z
            );
            
            Vector3 boundsSize = new Vector3(
                cameraConfig.boundsMax.x - cameraConfig.boundsMin.x,
                cameraConfig.boundsMax.y - cameraConfig.boundsMin.y,
                1f
            );
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
            
            if (IsCameraAtBounds())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.3f);
            }
        }
        
        #if UNITY_EDITOR
        // Show debug info in scene view
        if (cameraConfig.enableDebugLogs && player != null)
        {
            string debugInfo = $"Camera Debug:\n" +
                              $"Velocity: {playerVelocity.magnitude:F2}\n" +
                              $"LookAhead: {currentLookAhead.magnitude:F2}\n" +
                              $"InDeadzone: {IsPlayerInDeadZone()}\n" +
                              $"AtBounds: {IsCameraAtBounds()}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, debugInfo);
        }
        #endif
    }
}