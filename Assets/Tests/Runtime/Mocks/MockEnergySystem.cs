using UnityEngine;

namespace MechLite.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IEnergyUser for testing
    /// </summary>
    public class MockEnergySystem : MonoBehaviour, IEnergyUser
    {
        [SerializeField] private float currentEnergy = 100f;
        [SerializeField] private float maxEnergy = 100f;

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float EnergyPercent => MaxEnergy > 0 ? CurrentEnergy / MaxEnergy : 0f;

        public bool HasEnergy(float amount) => currentEnergy >= amount;

        public bool ConsumeEnergy(float amount)
        {
            if (!HasEnergy(amount)) return false;
            
            currentEnergy = Mathf.Max(0, currentEnergy - amount);
            return true;
        }

        public void RegenerateEnergy(float amount)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        }

        // Test utilities
        public void SetEnergy(float energy) => currentEnergy = Mathf.Clamp(energy, 0, maxEnergy);
        public void SetMaxEnergy(float max) => maxEnergy = max;
        public void ResetToMax() => currentEnergy = maxEnergy;
        public void EmptyEnergy() => currentEnergy = 0;
    }
}