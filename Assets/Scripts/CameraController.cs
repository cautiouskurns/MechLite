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
        
        Vector3 targetPos = new Vector3(
            cameraConfig.followX ? player.position.x : cameraWorldPos.x,
            cameraConfig.followY ? player.position.y : cameraWorldPos.y,
            cameraConfig.followZ ? player.position.z : cameraWorldPos.z
        ) + cameraConfig.offset;
        
        if (cameraConfig.useSmoothDamp)
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref dampVelocity, cameraConfig.smoothTime);
        else
            transform.position = Vector3.Lerp(transform.position, targetPos, cameraConfig.followSpeed * Time.deltaTime);
        
        ApplyBounds();
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
        ApplyBounds();
    }
    
    public CameraConfigSO GetConfiguration() { return cameraConfig; }
    public void SetPlayer(Transform newPlayer) { player = newPlayer; }
    
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
    
    public void SnapToTarget()
    {
        if (player != null && cameraConfig != null)
        {
            transform.position = player.position + cameraConfig.offset;
            ApplyBounds();
            dampVelocity = Vector3.zero;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (cameraConfig == null) return;
        
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, player.position);
        }
        
        if (player != null && cameraConfig.showDeadZoneGizmo && cameraConfig.deadZoneSize > 0f)
        {
            Vector3 deadZoneCenter = transform.position - cameraConfig.offset;
            bool playerInDeadZone = IsPlayerInDeadZone();
            
            Gizmos.color = playerInDeadZone ? Color.green : Color.red;
            Gizmos.DrawWireSphere(deadZoneCenter, cameraConfig.deadZoneSize);
        }
        
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
    }
}