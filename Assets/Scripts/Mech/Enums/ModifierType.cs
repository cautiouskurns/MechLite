namespace MechLite.Mech
{
    /// <summary>
    /// Types of stat modifications that can be applied to base values
    /// Determines the order and method of calculation for stat modifiers
    /// </summary>
    public enum ModifierType
    {
        /// <summary>
        /// Adds a flat value to the base stat (applied first)
        /// Example: +5 Health, +10 Damage
        /// </summary>
        Additive,
        
        /// <summary>
        /// Multiplies the current value by a percentage (applied after additive)
        /// Example: +20% (value = 1.2), -10% (value = 0.9)
        /// </summary>
        Multiplicative,
        
        /// <summary>
        /// Completely overrides the calculated value (applied last)
        /// Example: Set MoveSpeed to exactly 10, regardless of other modifiers
        /// </summary>
        Override
    }
}