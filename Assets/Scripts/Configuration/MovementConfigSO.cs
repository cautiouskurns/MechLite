using UnityEngine;

namespace MechLite.Configuration
{
    /// <summary>
    /// ScriptableObject configuration for movement parameters
    /// Centralizes all movement-related settings for easy balancing
    /// </summary>
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "MechSalvager/Configuration/Movement Config")]
    public class MovementConfigSO : ScriptableObject
    {
        [Header("Movement Settings")]
        [SerializeField, Range(1f, 10f), Tooltip("Maximum horizontal movement speed")]
        public float moveSpeed = 5f;
        
        [SerializeField, Range(5f, 25f), Tooltip("How quickly we reach max speed")]
        public float acceleration = 10f;
        
        [SerializeField, Range(5f, 25f), Tooltip("How quickly we stop when no input")]
        public float deceleration = 10f;
        
        [Header("Jump Settings")]
        [SerializeField, Range(5f, 20f), Tooltip("Upward force applied when jumping")]
        public float jumpForce = 8f;
        
        [SerializeField, Range(0.05f, 0.3f), Tooltip("Grace period after leaving ground")]
        public float coyoteTime = 0.15f;
        
        [SerializeField, Range(0.0f, 0.2f), Tooltip("Input buffer window for jump")]
        public float jumpBufferTime = 0.1f;
        
        [Header("Air Control")]
        [SerializeField, Range(0f, 1f), Tooltip("Air control strength as percentage of ground control")]
        public float airControlStrength = 0.1f;
        
        [Header("Physics Constraints")]
        [SerializeField, Tooltip("Whether to clamp velocity to max speed when grounded")]
        public bool clampGroundedVelocity = true;
    }
}
