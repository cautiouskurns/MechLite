using UnityEngine;
using MechLite.Configuration;

public class CameraController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private CameraConfigSO cameraConfig;
    
    [Header("Target Following")]
    [SerializeField] private Transform player;
    
    private Vector3 dampVelocity;
    
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
            transform.position = player.position + cameraConfig.offset;
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
        
        Vector3 targetPos = new Vector3(
            cameraConfig.followX ? player.position.x : cameraWorldPos.x,
            cameraConfig.followY ? player.position.y : cameraWorldPos.y,
            cameraConfig.followZ ? player.position.z : cameraWorldPos.z
        ) + cameraConfig.offset;
        
        if (cameraConfig.useSmoothDamp)
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref dampVelocity, cameraConfig.smoothTime);
        else
            transform.position = Vector3.Lerp(transform.position, targetPos, cameraConfig.followSpeed * Time.deltaTime);
    }
    
    public void SetConfiguration(CameraConfigSO newConfig)
    {
        cameraConfig = newConfig;
        dampVelocity = Vector3.zero;
    }
    
    public CameraConfigSO GetConfiguration() { return cameraConfig; }
    public void SetPlayer(Transform newPlayer) { player = newPlayer; }
    
    public bool IsPlayerInDeadZone()
    {
        if (player == null || cameraConfig == null) return false;
        Vector3 cameraWorldPos = transform.position - cameraConfig.offset;
        return Vector3.Distance(cameraWorldPos, player.position) < cameraConfig.deadZoneSize;
    }
    
    public void SnapToTarget()
    {
        if (player != null && cameraConfig != null)
        {
            transform.position = player.position + cameraConfig.offset;
            dampVelocity = Vector3.zero;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (player == null || cameraConfig == null) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, player.position);
        
        if (cameraConfig.showDeadZoneGizmo && cameraConfig.deadZoneSize > 0f)
        {
            Vector3 deadZoneCenter = transform.position - cameraConfig.offset;
            bool playerInDeadZone = IsPlayerInDeadZone();
            
            Gizmos.color = playerInDeadZone ? Color.green : Color.red;
            Gizmos.DrawWireSphere(deadZoneCenter, cameraConfig.deadZoneSize);
        }
    }
}