using UnityEngine;

namespace MechLite.Configuration
{
    /// <summary>
    /// ScriptableObject configuration for energy system parameters
    /// Manages all energy-related settings including dash costs and regeneration
    /// </summary>
    [CreateAssetMenu(fileName = "EnergyConfig", menuName = "MechSalvager/Configuration/Energy Config")]
    public class EnergyConfigSO : ScriptableObject
    {
        [Header("Energy Capacity")]
        [SerializeField, Range(50f, 200f), Tooltip("Maximum energy capacity")]
        public float maxEnergy = 100f;
        
        [SerializeField, Range(10f, 50f), Tooltip("Energy regenerated per second")]
        public float energyRegenRate = 20f;
        
        [Header("Energy Costs")]
        [SerializeField, Range(10f, 50f), Tooltip("Energy cost for dash")]
        public float dashEnergyCost = 25f;
        
        [Header("Regeneration Settings")]
        [SerializeField, Tooltip("Whether energy regenerates automatically")]
        public bool autoRegenerate = true;
        
        [SerializeField, Range(0f, 2f), Tooltip("Delay before regeneration starts after energy consumption")]
        public float regenDelay = 0f;
        
        /// <summary>
        /// Validates configuration values for logical consistency
        /// </summary>
        private void OnValidate()
        {
            if (dashEnergyCost > maxEnergy)
            {
                Debug.LogWarning($"EnergyConfig: Dash energy cost ({dashEnergyCost}) exceeds max energy ({maxEnergy})!");
            }
        }
    }
}
