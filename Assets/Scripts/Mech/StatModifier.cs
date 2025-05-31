using System;

namespace MechLite.Mech
{
    /// <summary>
    /// Represents a single modification to a mech stat
    /// Contains all information needed to apply, track, and remove the modification
    /// </summary>
    [Serializable]
    public struct StatModifier : IEquatable<StatModifier>
    {
        /// <summary>
        /// The stat being modified
        /// </summary>
        public StatType statType;
        
        /// <summary>
        /// The value of the modification
        /// - Additive: Flat value to add (e.g., +5 Health)
        /// - Multiplicative: Multiplier value (e.g., 1.2 for +20%, 0.8 for -20%)
        /// - Override: Exact value to set
        /// </summary>
        public float value;
        
        /// <summary>
        /// How this modifier should be applied to the stat
        /// </summary>
        public ModifierType type;
        
        /// <summary>
        /// Source identifier for tracking and removal
        /// Examples: "Equipment.Armor.ChestPlate", "Ability.SpeedBoost", "Temporary.Buff"
        /// </summary>
        public string source;
        
        /// <summary>
        /// Create a new stat modifier
        /// </summary>
        /// <param name="statType">Which stat to modify</param>
        /// <param name="value">Modification value</param>
        /// <param name="type">Type of modification</param>
        /// <param name="source">Source identifier for tracking</param>
        public StatModifier(StatType statType, float value, ModifierType type, string source)
        {
            this.statType = statType;
            this.value = value;
            this.type = type;
            this.source = source ?? "Unknown";
        }
        
        /// <summary>
        /// Create an additive stat modifier
        /// </summary>
        public static StatModifier Additive(StatType statType, float value, string source)
        {
            return new StatModifier(statType, value, ModifierType.Additive, source);
        }
        
        /// <summary>
        /// Create a multiplicative stat modifier
        /// </summary>
        public static StatModifier Multiplicative(StatType statType, float multiplier, string source)
        {
            return new StatModifier(statType, multiplier, ModifierType.Multiplicative, source);
        }
        
        /// <summary>
        /// Create an override stat modifier
        /// </summary>
        public static StatModifier Override(StatType statType, float value, string source)
        {
            return new StatModifier(statType, value, ModifierType.Override, source);
        }
        
        /// <summary>
        /// Create a percentage-based multiplicative modifier
        /// </summary>
        /// <param name="statType">Stat to modify</param>
        /// <param name="percentage">Percentage change (e.g., 20 for +20%, -10 for -10%)</param>
        /// <param name="source">Source identifier</param>
        public static StatModifier Percentage(StatType statType, float percentage, string source)
        {
            return new StatModifier(statType, 1f + (percentage / 100f), ModifierType.Multiplicative, source);
        }
        
        public bool Equals(StatModifier other)
        {
            return statType == other.statType && 
                   Math.Abs(value - other.value) < 0.0001f && 
                   type == other.type && 
                   string.Equals(source, other.source);
        }
        
        public override bool Equals(object obj)
        {
            return obj is StatModifier other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine((int)statType, value, (int)type, source);
        }
        
        public static bool operator ==(StatModifier left, StatModifier right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(StatModifier left, StatModifier right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            string typeStr = type switch
            {
                ModifierType.Additive => value >= 0 ? $"+{value}" : value.ToString(),
                ModifierType.Multiplicative => $"×{value:F2}",
                ModifierType.Override => $"={value}",
                _ => value.ToString()
            };
            
            return $"{statType} {typeStr} ({source})";
        }
    }
}