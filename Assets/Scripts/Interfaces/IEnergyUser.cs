using UnityEngine;

namespace MechLite.Energy
{
    /// <summary>
    /// Interface for systems that manage and consume energy
    /// Defines the contract for energy-related functionality
    /// </summary>
    public interface IEnergyUser
    {
        /// <summary>
        /// Current energy amount
        /// </summary>
        float CurrentEnergy { get; }
        
        /// <summary>
        /// Maximum energy capacity
        /// </summary>
        float MaxEnergy { get; }
        
        /// <summary>
        /// Current energy as a percentage (0-1)
        /// </summary>
        float EnergyPercent { get; }
        
        /// <summary>
        /// Check if enough energy is available for a specific cost
        /// </summary>
        /// <param name="cost">Energy cost to check</param>
        /// <returns>True if energy is sufficient</returns>
        bool HasEnergy(float cost);
        
        /// <summary>
        /// Consume energy if available
        /// </summary>
        /// <param name="amount">Amount of energy to consume</param>
        /// <returns>True if energy was consumed successfully</returns>
        bool ConsumeEnergy(float amount);
        
        /// <summary>
        /// Regenerate energy over time
        /// </summary>
        void RegenerateEnergy();
    }
}
