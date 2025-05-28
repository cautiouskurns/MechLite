using UnityEngine;

namespace MechSalvager.Movement
{
    /// <summary>
    /// Interface for ground detection and coyote time mechanics
    /// Handles the complexity of ground state checking and timing systems
    /// </summary>
    public interface IGroundDetector
    {
        /// <summary>
        /// Whether the entity is currently on the ground
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Time remaining in the coyote time window (grace period after leaving ground)
        /// </summary>
        float CoyoteTimeRemaining { get; }
        
        /// <summary>
        /// Whether the entity was recently grounded (within coyote time)
        /// </summary>
        bool WasRecentlyGrounded { get; }
        
        /// <summary>
        /// Check for ground collision and update ground state
        /// </summary>
        void CheckGrounded();
        
        /// <summary>
        /// Update coyote time calculations
        /// Called each frame to track timing
        /// </summary>
        void UpdateGroundTiming();
        
        /// <summary>
        /// Whether the entity can perform ground-based actions (considering coyote time)
        /// </summary>
        bool CanPerformGroundAction();
    }
}
