using UnityEngine;

namespace MechLite.Configuration
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "MechSalvager/Configuration/Camera Config")]
    public class CameraConfigSO : ScriptableObject
    {
        [Header("Camera Positioning")]
        public Vector3 offset = new Vector3(0f, 2f, -10f);
        
        [Header("Following Behavior")]
        [Range(1f, 15f)] public float followSpeed = 5f;
        public bool useSmoothDamp = false;
        [Range(0.1f, 2f)] public float smoothTime = 0.3f;
        
        [Header("Follow Constraints")]
        public bool followX = true;
        public bool followY = true;
        public bool followZ = false;
        
        [Header("Deadzone Settings")]
        [Range(0f, 3f)] public float deadZoneSize = 0.5f;
        
        [Header("Camera Bounds")]
        public bool useBounds = false;
        public Vector2 boundsMin = new Vector2(-10f, -5f);
        public Vector2 boundsMax = new Vector2(10f, 5f);
        
        [Header("Debug")]
        public bool showDeadZoneGizmo = true;
        public bool enableDebugLogs = false;
        
        private void OnValidate()
        {
            if (offset.z >= 0f)
                Debug.LogWarning($"CameraConfig: Z offset should be negative for 2D!");
            if (!followX && !followY && !followZ)
                Debug.LogWarning($"CameraConfig: At least one axis should be enabled!");
            
            // Validate bounds
            if (useBounds)
            {
                if (boundsMin.x >= boundsMax.x)
                    Debug.LogWarning($"CameraConfig: Bounds min X should be less than max X!");
                if (boundsMin.y >= boundsMax.y)
                    Debug.LogWarning($"CameraConfig: Bounds min Y should be less than max Y!");
            }
        }
        
        public string GetConfigDescription()
        {
            string method = useSmoothDamp ? $"SmoothDamp ({smoothTime:F1}s)" : $"Lerp ({followSpeed:F1})";
            string axes = $"{(followX ? "X" : "")}{(followY ? "Y" : "")}{(followZ ? "Z" : "")}";
            string bounds = useBounds ? $"Bounds: ({boundsMin.x:F1},{boundsMin.y:F1}) to ({boundsMax.x:F1},{boundsMax.y:F1})" : "No Bounds";
            return $"{method}, Axes: {axes}, DeadZone: {deadZoneSize:F1}, {bounds}";
        }
    }
}