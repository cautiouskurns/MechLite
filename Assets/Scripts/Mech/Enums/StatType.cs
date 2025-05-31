namespace MechLite.Mech
{
    /// <summary>
    /// Enumeration of all stat types available for mech customization and progression
    /// Used for type-safe stat identification and modifier targeting
    /// </summary>
    public enum StatType
    {
        // Health System
        Health,
        MaxHealth,
        
        // Energy System
        Energy,
        MaxEnergy,
        
        // Movement Stats
        MoveSpeed,
        JumpForce,
        DashForce,
        
        // Combat Stats
        Armor,
        Damage,
        CritChance
    }
}