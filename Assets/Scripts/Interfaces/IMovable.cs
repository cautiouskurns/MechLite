using UnityEngine;

namespace MechSalvager.Movement
{
    /// <summary>
    /// Interface for objects that can be moved in the game world
    /// Defines the contract for movement-related functionality
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// Current velocity of the movable object
        /// </summary>
        Vector2 Velocity { get; }
        
        /// <summary>
        /// Whether the object is currently grounded
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Move the object horizontally with the given input
        /// </summary>
        /// <param name="horizontalInput">Input value (-1 to 1)</param>
        void Move(float horizontalInput);
        
        /// <summary>
        /// Attempt to jump if conditions are met
        /// </summary>
        /// <returns>True if jump was executed</returns>
        bool Jump();
        
        /// <summary>
        /// Set the velocity directly (used for external forces like dash)
        /// </summary>
        /// <param name="velocity">New velocity to apply</param>
        void SetVelocity(Vector2 velocity);
    }
}
