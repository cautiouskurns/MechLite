using UnityEngine;

namespace MechSalvager.Configuration
{
    /// <summary>
    /// ScriptableObject configuration for physics and collision detection
    /// Manages ground detection and other physics-related settings
    /// </summary>
    [CreateAssetMenu(fileName = "PhysicsConfig", menuName = "MechSalvager/Configuration/Physics Config")]
    public class PhysicsConfigSO : ScriptableObject
    {
        [Header("Ground Detection")]
        [SerializeField, Tooltip("Layers considered as ground for collision detection")]
        public LayerMask groundLayerMask = 1 << 8; // Layer 8 for Ground
        
        [SerializeField, Range(0.1f, 0.5f), Tooltip("How far to check for ground below player")]
        public float groundCheckDistance = 0.2f;
        
        [SerializeField, Range(0.01f, 0.1f), Tooltip("Size of ground detection sphere")]
        public float groundCheckRadius = 0.05f;
        
        [Header("Collision Settings")]
        [SerializeField, Tooltip("Use sphere cast instead of raycast for ground detection")]
        public bool useSphereCast = false;
        
        [SerializeField, Tooltip("Offset from collider bottom for ground check origin")]
        public Vector2 groundCheckOffset = Vector2.zero;
        
        [Header("Debug Visualization")]
        [SerializeField, Tooltip("Show ground detection gizmos in scene view")]
        public bool showGroundGizmos = true;
        
        [SerializeField, Tooltip("Color for grounded state visualization")]
        public Color groundedColor = Color.green;
        
        [SerializeField, Tooltip("Color for airborne state visualization")]
        public Color airborneColor = Color.red;
    }
}
