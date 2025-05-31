using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MechLite.Mech
{
    [Serializable]
    public class MechStats
    {
        private Dictionary<StatType, float> baseStats;
        private List<StatModifier> modifiers;
        private Dictionary<StatType, float> cachedStats;
        private bool isDirty = true;
        
        public event Action<StatType, float, float> OnStatChanged;
        
        public void Initialize(MechConfigSO config)
        {
            baseStats = new Dictionary<StatType, float>();
            modifiers = new List<StatModifier>();
            cachedStats = new Dictionary<StatType, float>();
            
            if (config != null)
            {
                baseStats = config.GetAllBaseStats();
            }
            else
            {
                LoadDefaultStats();
            }
            
            RecalculateAllStats();
        }
        
        public float GetStat(StatType statType)
        {
            if (isDirty)
            {
                RecalculateAllStats();
            }
            
            return cachedStats.TryGetValue(statType, out float value) ? value : 0f;
        }
        
        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
            isDirty = true;
            RecalculateAndNotify(modifier.statType);
        }
        
        public bool RemoveModifier(StatModifier modifier)
        {
            bool removed = modifiers.Remove(modifier);
            if (removed)
            {
                isDirty = true;
                RecalculateAndNotify(modifier.statType);
            }
            return removed;
        }
        
        private float CalculateStat(StatType statType)
        {
            float baseValue = baseStats.TryGetValue(statType, out float value) ? value : 0f;
            var statModifiers = modifiers.Where(m => m.statType == statType).ToList();
            
            if (statModifiers.Count == 0)
                return baseValue;
            
            float result = baseValue;
            
            foreach (var modifier in statModifiers.Where(m => m.type == ModifierType.Additive))
            {
                result += modifier.value;
            }
            
            foreach (var modifier in statModifiers.Where(m => m.type == ModifierType.Multiplicative))
            {
                result *= modifier.value;
            }
            
            var overrides = statModifiers.Where(m => m.type == ModifierType.Override);
            if (overrides.Any())
            {
                result = overrides.Last().value;
            }
            
            return result;
        }
        
        private void RecalculateAllStats()
        {
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                cachedStats[statType] = CalculateStat(statType);
            }
            isDirty = false;
        }
        
        private void RecalculateAndNotify(StatType statType)
        {
            float oldValue = cachedStats.TryGetValue(statType, out float cached) ? cached : 0f;
            float newValue = CalculateStat(statType);
            cachedStats[statType] = newValue;
            
            if (Math.Abs(oldValue - newValue) > 0.0001f)
            {
                OnStatChanged?.Invoke(statType, oldValue, newValue);
            }
        }
        
        private void LoadDefaultStats()
        {
            baseStats = new Dictionary<StatType, float>
            {
                { StatType.Health, 100f },
                { StatType.MaxHealth, 100f },
                { StatType.Energy, 100f },
                { StatType.MaxEnergy, 100f },
                { StatType.MoveSpeed, 5f },
                { StatType.JumpForce, 8f },
                { StatType.DashForce, 18f },
                { StatType.Armor, 0f },
                { StatType.Damage, 25f },
                { StatType.CritChance, 5f }
            };
        }
    }
}