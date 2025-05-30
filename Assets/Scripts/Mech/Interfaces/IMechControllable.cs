using UnityEngine;

namespace MechLite.Mech
{
    /// <summary>
    /// Core interface defining the contract for mech controllable behavior.
    /// Provides the foundation for all mech-related functionality and system coordination.
    /// </summary>
    public interface IMechControllable
    {
        /// <summary>
        /// Whether the mech controller has been successfully initialized
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Current world position of the mech
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// Initialize the mech controller and all its subsystems
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Shutdown the mech controller and cleanup resources
        /// </summary>
        void Shutdown();
    }
}