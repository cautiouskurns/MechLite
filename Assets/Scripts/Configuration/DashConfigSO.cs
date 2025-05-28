using UnityEngine;

namespace MechLite.Configuration
{
    /// <summary>
    /// ScriptableObject configuration for dash system parameters
    /// Centralizes dash mechanics settings for easy balancing
    /// </summary>
    [CreateAssetMenu(fileName = "DashConfig", menuName = "MechSalvager/Configuration/Dash Config")]
    public class DashConfigSO : ScriptableObject
    {
        [Header("Dash Force")]
        [SerializeField, Range(10f, 35f), Tooltip("Horizontal force applied when dashing")]
        public float dashForce = 18f;
        
        [Header("Cooldown Settings")]
        [SerializeField, Range(0.1f, 2f), Tooltip("Cooldown between dashes")]
        public float dashCooldown = 0.5f;
        
        [Header("Direction Settings")]
        [SerializeField, Tooltip("Whether dash uses current input or last movement direction")]
        public bool useInputDirection = true;
        
        [SerializeField, Tooltip("Default dash direction when no input or movement history")]
        public Vector2 defaultDashDirection = Vector2.right;
        
        [Header("Physics Settings")]
        [SerializeField, Tooltip("Whether dash can be performed in air")]
        public bool allowAirDash = true;
        
        [SerializeField, Tooltip("Whether dash preserves vertical velocity")]
        public bool preserveVerticalVelocity = true;
        
        /// <summary>
        /// Validates configuration values for logical consistency
        /// </summary>
        private void OnValidate()
        {
            // Normalize default direction
            if (defaultDashDirection.magnitude > 0)
            {
                defaultDashDirection = defaultDashDirection.normalized;
            }
            else
            {
                defaultDashDirection = Vector2.right;
            }
        }
    }
}
