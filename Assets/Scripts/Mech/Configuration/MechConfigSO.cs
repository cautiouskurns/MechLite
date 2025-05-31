using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MechLite.Mech
{
    /// <summary>
    /// ScriptableObject configuration for base mech statistics
    /// Defines the starting values for all mech stats before modifiers are applied
    /// </summary>
    [CreateAssetMenu(fileName = "MechConfig", menuName = "MechSalvager/Configuration/Mech Config")]
    public class MechConfigSO : ScriptableObject
    {
        [Header("Health System")]
        [SerializeField, Range(50f, 500f), Tooltip("Base health points")]
        public float baseHealth = 100f;
        
        [SerializeField, Range(50f, 500f), Tooltip("Maximum health capacity")]
        public float baseMaxHealth = 100f;
        
        [Header("Energy System")]
        [SerializeField, Range(50f, 200f), Tooltip("Starting energy amount")]
        public float baseEnergy = 100f;
        
        [SerializeField, Range(50f, 200f), Tooltip("Maximum energy capacity")]
        public float baseMaxEnergy = 100f;
        
        [Header("Movement Stats")]
        [SerializeField, Range(1f, 15f), Tooltip("Base horizontal movement speed")]
        public float baseMoveSpeed = 5f;
        
        [SerializeField, Range(5f, 25f), Tooltip("Base jump force")]
        public float baseJumpForce = 8f;
        
        [SerializeField, Range(10f, 35f), Tooltip("Base dash force")]
        public float baseDashForce = 18f;
        
        [Header("Combat Stats")]
        [SerializeField, Range(0f, 50f), Tooltip("Base armor rating (damage reduction)")]
        public float baseArmor = 0f;
        
        [SerializeField, Range(10f, 100f), Tooltip("Base damage output")]
        public float baseDamage = 25f;
        
        [SerializeField, Range(0f, 100f), Tooltip("Base critical hit chance (percentage)")]
        public float baseCritChance = 5f;
        
        /// <summary>
        /// Get base value for a specific stat type
        /// </summary>
        /// <param name="statType">The stat to get the base value for</param>
        /// <returns>Base value for the stat</returns>
        public float GetBaseStat(StatType statType)
        {
            return statType switch
            {
                StatType.Health => baseHealth,
                StatType.MaxHealth => baseMaxHealth,
                StatType.Energy => baseEnergy,
                StatType.MaxEnergy => baseMaxEnergy,
                StatType.MoveSpeed => baseMoveSpeed,
                StatType.JumpForce => baseJumpForce,
                StatType.DashForce => baseDashForce,
                StatType.Armor => baseArmor,
                StatType.Damage => baseDamage,
                StatType.CritChance => baseCritChance,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Get all base stats as a dictionary
        /// </summary>
        /// <returns>Dictionary mapping stat types to their base values</returns>
        public Dictionary<StatType, float> GetAllBaseStats()
        {
            var stats = new Dictionary<StatType, float>();
            
            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)).Cast<StatType>())
            {
                stats[statType] = GetBaseStat(statType);
            }
            
            return stats;
        }
        
        /// <summary>
        /// Validate configuration values for logical consistency
        /// </summary>
        private void OnValidate()
        {
            // Ensure current values don't exceed maximums
            if (baseHealth > baseMaxHealth)
            {
                Debug.LogWarning($"MechConfig: Base health ({baseHealth}) exceeds max health ({baseMaxHealth})!");
                baseHealth = baseMaxHealth;
            }
            
            if (baseEnergy > baseMaxEnergy)
            {
                Debug.LogWarning($"MechConfig: Base energy ({baseEnergy}) exceeds max energy ({baseMaxEnergy})!");
                baseEnergy = baseMaxEnergy;
            }
            
            // Validate reasonable ranges
            if (baseCritChance > 100f)
            {
                Debug.LogWarning($"MechConfig: Crit chance ({baseCritChance}%) exceeds 100%!");
                baseCritChance = 100f;
            }
            
            if (baseMoveSpeed <= 0f)
            {
                Debug.LogWarning($"MechConfig: Move speed must be positive!");
                baseMoveSpeed = 1f;
            }
        }
        
        /// <summary>
        /// Get a description of this configuration
        /// </summary>
        public string GetConfigDescription()
        {
            return $"Mech Config: HP {baseMaxHealth}, Energy {baseMaxEnergy}, Speed {baseMoveSpeed}, Damage {baseDamage}";
        }
    }
}