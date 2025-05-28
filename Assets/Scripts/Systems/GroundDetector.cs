using UnityEngine;
using MechSalvager.Configuration;
using MechSalvager.Events;

namespace MechSalvager.Movement
{
    /// <summary>
    /// Handles ground detection and coyote time mechanics
    /// Implements IGroundDetector interface for ground state management
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class GroundDetector : MonoBehaviour, IGroundDetector
    {
        [Header("Configuration")]
        [SerializeField] private PhysicsConfigSO physicsConfig;
        [SerializeField] private MovementConfigSO movementConfig; // For coyote time
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        
        // Component references
        private BoxCollider2D boxCollider;
        
        // Ground state
        private bool isGrounded;
        private float lastGroundedTime;
        
        // Properties from IGroundDetector interface
        public bool IsGrounded => isGrounded;
        public float CoyoteTimeRemaining => Mathf.Max(0f, (movementConfig?.coyoteTime ?? 0.15f) - (Time.time - lastGroundedTime));
        public bool WasRecentlyGrounded => CoyoteTimeRemaining > 0f;
        
        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            
            if (physicsConfig == null)
            {
                Debug.LogError("GroundDetector: PhysicsConfigSO is not assigned!");
            }
            
            if (movementConfig == null)
            {
                Debug.LogError("GroundDetector: MovementConfigSO is not assigned!");
            }
        }
        
        private void Update()
        {
            CheckGrounded();
            UpdateGroundTiming();
        }
        
        /// <summary>
        /// Check for ground collision and update ground state
        /// </summary>
        public void CheckGrounded()
        {
            if (physicsConfig == null || boxCollider == null) return;
            
            bool wasGrounded = isGrounded;
            
            // Calculate raycast origin
            Vector2 raycastOrigin = (Vector2)transform.position + 
                                   Vector2.down * (boxCollider.size.y / 2) + 
                                   physicsConfig.groundCheckOffset;
            
            // Perform ground check
            if (physicsConfig.useSphereCast)
            {
                // Use sphere cast for more forgiving ground detection
                RaycastHit2D hit = Physics2D.CircleCast(
                    raycastOrigin, 
                    physicsConfig.groundCheckRadius, 
                    Vector2.down, 
                    physicsConfig.groundCheckDistance, 
                    physicsConfig.groundLayerMask
                );
                isGrounded = hit.collider != null;
            }
            else
            {
                // Use standard raycast
                RaycastHit2D hit = Physics2D.Raycast(
                    raycastOrigin, 
                    Vector2.down, 
                    physicsConfig.groundCheckDistance, 
                    physicsConfig.groundLayerMask
                );
                isGrounded = hit.collider != null;
            }
            
            // Update ground timing
            if (isGrounded)
            {
                lastGroundedTime = Time.time;
            }
            
            // Publish ground state change event if state changed
            if (wasGrounded != isGrounded)
            {
                PublishGroundStateEvent(wasGrounded);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"GroundDetector: Ground state changed - isGrounded: {isGrounded}");
                }
            }
        }
        
        /// <summary>
        /// Update coyote time calculations
        /// </summary>
        public void UpdateGroundTiming()
        {
            // This method is called each frame to keep timing updated
            // The actual timing is handled by the CoyoteTimeRemaining property
        }
        
        /// <summary>
        /// Whether the entity can perform ground-based actions (considering coyote time)
        /// </summary>
        /// <returns>True if can perform ground actions</returns>
        public bool CanPerformGroundAction()
        {
            return isGrounded || WasRecentlyGrounded;
        }
        
        /// <summary>
        /// Get the current ground check position for external systems
        /// </summary>
        /// <returns>World position of ground check origin</returns>
        public Vector2 GetGroundCheckPosition()
        {
            if (boxCollider == null) return transform.position;
            
            return (Vector2)transform.position + 
                   Vector2.down * (boxCollider.size.y / 2) + 
                   physicsConfig.groundCheckOffset;
        }
        
        private void PublishGroundStateEvent(bool wasGrounded)
        {
            PlayerEventBus.PublishGroundStateChanged(new GroundStateChangedEvent(
                isGrounded,
                wasGrounded,
                transform.position,
                Time.time - lastGroundedTime
            ));
        }
        
        /// <summary>
        /// Debug visualization for ground detection
        /// </summary>
        private void OnDrawGizmos()
        {
            if (physicsConfig == null || !physicsConfig.showGroundGizmos || boxCollider == null) return;
            
            // Calculate positions
            Vector2 raycastOrigin = (Vector2)transform.position + 
                                   Vector2.down * (boxCollider.size.y / 2) + 
                                   physicsConfig.groundCheckOffset;
            Vector2 raycastEnd = raycastOrigin + Vector2.down * physicsConfig.groundCheckDistance;
            
            // Perform real-time check for accurate gizmo color
            bool isCurrentlyGrounded;
            if (physicsConfig.useSphereCast)
            {
                RaycastHit2D hit = Physics2D.CircleCast(
                    raycastOrigin, 
                    physicsConfig.groundCheckRadius, 
                    Vector2.down, 
                    physicsConfig.groundCheckDistance, 
                    physicsConfig.groundLayerMask
                );
                isCurrentlyGrounded = hit.collider != null;
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    raycastOrigin, 
                    Vector2.down, 
                    physicsConfig.groundCheckDistance, 
                    physicsConfig.groundLayerMask
                );
                isCurrentlyGrounded = hit.collider != null;
            }
            
            // Set gizmo color
            Gizmos.color = isCurrentlyGrounded ? physicsConfig.groundedColor : physicsConfig.airborneColor;
            
            // Draw visualization
            if (physicsConfig.useSphereCast)
            {
                // Draw sphere at origin and end
                Gizmos.DrawWireSphere(raycastOrigin, physicsConfig.groundCheckRadius);
                Gizmos.DrawWireSphere(raycastEnd, physicsConfig.groundCheckRadius);
                
                // Draw line connecting them
                Gizmos.DrawLine(raycastOrigin, raycastEnd);
            }
            else
            {
                // Draw standard raycast line
                Gizmos.DrawLine(raycastOrigin, raycastEnd);
                Gizmos.DrawWireSphere(raycastOrigin, 0.05f);
            }
            
            #if UNITY_EDITOR
            // Show debug info in scene view
            string debugText = $"Grounded: {(isCurrentlyGrounded ? "Yes" : "No")}\n" +
                              $"Coyote: {CoyoteTimeRemaining:F2}s";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, debugText);
            #endif
        }
    }
}
