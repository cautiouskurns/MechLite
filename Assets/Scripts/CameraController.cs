using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Following")]
    [SerializeField] private Transform player;
    
    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);
    [SerializeField, Range(1f, 15f)] private float followSpeed = 5f;
    
    [Header("Follow Constraints")]
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private bool followZ = false;
    
    [Header("Deadzone Settings")]
    [SerializeField, Range(0f, 3f)] private float deadZoneSize = 0.5f;
    [SerializeField] private bool showDeadZoneGizmo = true;
    
    [Header("Smoothing")]
    [SerializeField] private bool useSmoothDamp = false;
    [SerializeField, Range(0.1f, 2f)] private float smoothTime = 0.3f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    
    private Vector3 dampVelocity;
    
    private void Start()
    {
        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
                if (enableDebugLogs)
                    Debug.Log("CameraController: Auto-found player");
            }
        }
        
        if (player != null)
        {
            transform.position = player.position + offset;
            if (enableDebugLogs)
                Debug.Log($"CameraController: Initialized - DeadZone: {deadZoneSize}, Constraints: X{followX} Y{followY} Z{followZ}");
        }
    }
    
    private void LateUpdate()
    {
        if (player == null) return;
        
        Vector3 cameraWorldPos = transform.position - offset;
        float distance = Vector3.Distance(cameraWorldPos, player.position);
        
        if (distance < deadZoneSize)
        {
            if (enableDebugLogs)
                Debug.Log($"Player in deadzone - Distance: {distance:F2}");
            return;
        }
        
        Vector3 targetPos = new Vector3(
            followX ? player.position.x : cameraWorldPos.x,
            followY ? player.position.y : cameraWorldPos.y,
            followZ ? player.position.z : cameraWorldPos.z
        ) + offset;
        
        if (useSmoothDamp)
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref dampVelocity, smoothTime);
        else
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
    
    public void SetOffset(Vector3 newOffset) { offset = newOffset; }
    public Vector3 GetOffset() { return offset; }
    public void SetPlayer(Transform newPlayer) { player = newPlayer; }
    public void SetFollowSpeed(float speed) { followSpeed = Mathf.Clamp(speed, 1f, 15f); }
    public void SetDeadZoneSize(float size) { deadZoneSize = Mathf.Clamp(size, 0f, 3f); }
    public void SetAxisConstraints(bool x, bool y, bool z) { followX = x; followY = y; followZ = z; }
    
    public bool IsPlayerInDeadZone()
    {
        if (player == null) return false;
        Vector3 cameraWorldPos = transform.position - offset;
        return Vector3.Distance(cameraWorldPos, player.position) < deadZoneSize;
    }
    
    public Vector3 GetDeadZoneCenter()
    {
        return transform.position - offset;
    }
    
    public void SnapToTarget()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
            dampVelocity = Vector3.zero;
        }
    }
    
    private void OnValidate()
    {
        if (offset.z >= 0f)
            Debug.LogWarning("CameraController: Z offset should be negative for 2D!");
        if (!followX && !followY && !followZ)
            Debug.LogWarning("CameraController: At least one axis should be enabled!");
    }
    
    private void OnDrawGizmos()
    {
        if (player == null) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, player.position);
        
        Vector3 cameraWorldPos = transform.position - offset;
        Vector3 targetPos = new Vector3(
            followX ? player.position.x : cameraWorldPos.x,
            followY ? player.position.y : cameraWorldPos.y,
            followZ ? player.position.z : cameraWorldPos.z
        ) + offset;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPos, 0.3f);
        
        if (showDeadZoneGizmo && deadZoneSize > 0f)
        {
            Vector3 deadZoneCenter = GetDeadZoneCenter();
            bool playerInDeadZone = IsPlayerInDeadZone();
            
            Gizmos.color = playerInDeadZone ? Color.green : Color.red;
            Gizmos.DrawWireSphere(deadZoneCenter, deadZoneSize);
            
            Gizmos.color = new Color(playerInDeadZone ? 0f : 1f, playerInDeadZone ? 1f : 0f, 0f, 0.2f);
            Gizmos.DrawSphere(deadZoneCenter, deadZoneSize);
        }
        
        float distance = Vector3.Distance(transform.position, targetPos);
        Gizmos.color = distance > 1f ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        
        #if UNITY_EDITOR
        string constraints = $"XYZ: {(followX ? "✓" : "✗")}{(followY ? "✓" : "✗")}{(followZ ? "✓" : "✗")}";
        string deadZoneStatus = IsPlayerInDeadZone() ? "IN DEADZONE" : "Following";
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
            $"Camera Constraints\nConstraints: {constraints}\nDeadZone: {deadZoneSize:F1}\nStatus: {deadZoneStatus}");
        #endif
    }
}