using UnityEngine;

namespace MechLite.Mech
{
    /// <summary>
    /// Interface for systems that consume energy from the mech's energy management system
    /// Provides a contract for energy-dependent mech systems and abilities
    /// </summary>
    public interface IEnergyConsumer
    {
        /// <summary>
        /// Unique identifier for this energy consumer
        /// Used for debugging and tracking energy usage
        /// </summary>
        string ConsumerName { get; }
        
        /// <summary>
        /// Current energy cost for this consumer's primary action
        /// Can be dynamic based on upgrades, conditions, etc.
        /// </summary>
        float EnergyCost { get; }
        
        /// <summary>
        /// Whether this consumer is currently eligible to consume energy
        /// Checks cooldowns, prerequisites, and other conditions
        /// </summary>
        bool CanConsumeEnergy { get; }
        
        /// <summary>
        /// Attempt to perform an action that consumes energy
        /// The energy manager will validate energy availability before calling this
        /// </summary>
        /// <param name="availableEnergy">Current energy available for consumption</param>
        /// <returns>True if the action was performed successfully</returns>
        bool TryConsumeEnergy(float availableEnergy);
        
        /// <summary>
        /// Called when energy consumption fails due to insufficient energy
        /// Allows the consumer to provide user feedback or alternative behavior
        /// </summary>
        void OnEnergyConsumptionFailed();
    }
}
