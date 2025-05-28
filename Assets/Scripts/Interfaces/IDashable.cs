using UnityEngine;

namespace MechSalvager.Movement
{
    /// <summary>
    /// Interface for objects that can perform dash mechanics
    /// Defines the contract for dash-related functionality
    /// </summary>
    public interface IDashable
    {
        /// <summary>
        /// Whether a dash can be performed right now
        /// </summary>
        bool CanDash { get; }
        
        /// <summary>
        /// Time remaining until next dash is available
        /// </summary>
        float DashCooldownRemaining { get; }
        
        /// <summary>
        /// Attempt to perform a dash in the specified direction
        /// </summary>
        /// <param name="direction">Direction to dash (-1 = left, 1 = right)</param>
        /// <returns>True if dash was executed</returns>
        bool Dash(float direction);
        
        /// <summary>
        /// Update dash cooldown timer
        /// </summary>
        void UpdateDashCooldown();
    }
}
