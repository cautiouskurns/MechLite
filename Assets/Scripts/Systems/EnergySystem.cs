using UnityEngine;
using MechLite.Configuration;
using MechLite.Events;

namespace MechLite.Energy
{
    /// <summary>
    /// Manages energy for the player character
    /// Implements IEnergyUser interface and handles energy consumption/regeneration
    /// </summary>
    public class EnergySystem : MonoBehaviour, IEnergyUser
    {
        [Header("Configuration")]
        [SerializeField] private EnergyConfigSO energyConfig;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        
        // Energy state
        private float currentEnergy;
        private float lastConsumptionTime;
        
        // Properties from IEnergyUser interface
        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => energyConfig?.maxEnergy ?? 100f;
        public float EnergyPercent => currentEnergy / MaxEnergy;
        
        /// <summary>
        /// Initialize the energy system with configuration
        /// Used by tests and programmatic setup
        /// </summary>
        public void Initialize(EnergyConfigSO config)
        {
            energyConfig = config;
            currentEnergy = MaxEnergy;
        }
        
        private void Awake()
        {
            if (energyConfig == null)
            {
                Debug.LogError("EnergySystem: EnergyConfigSO is not assigned!");
            }
        }
        
        private void Start()
        {
            // Initialize energy to max
            currentEnergy = MaxEnergy;
            PublishEnergyEvent(0f, EnergyChangeReason.Initialization);
            
            if (enableDebugLogs)
            {
                Debug.Log($"EnergySystem: Initialized with {currentEnergy}/{MaxEnergy} energy");
            }
        }
        
        private void Update()
        {
            if (energyConfig?.autoRegenerate == true)
            {
                RegenerateEnergy();
            }
        }
        
        /// <summary>
        /// Check if there's enough energy for an action
        /// </summary>
        /// <param name="amount">Amount of energy required</param>
        /// <returns>True if enough energy is available</returns>
        public bool HasEnergy(float amount)
        {
            return currentEnergy >= amount;
        }
        
        /// <summary>
        /// Consume energy for an action
        /// </summary>
        /// <param name="amount">Amount of energy to consume</param>
        /// <returns>True if energy was successfully consumed</returns>
        public bool ConsumeEnergy(float amount)
        {
            if (!HasEnergy(amount))
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"EnergySystem: Insufficient energy - Required: {amount}, Available: {currentEnergy}");
                }
                return false;
            }
            
            float previousEnergy = currentEnergy;
            currentEnergy = Mathf.Max(0f, currentEnergy - amount);
            lastConsumptionTime = Time.time;
            
            PublishEnergyEvent(previousEnergy - currentEnergy, EnergyChangeReason.Consumption);
            
            if (enableDebugLogs)
            {
                Debug.Log($"EnergySystem: Consumed {amount} energy. Current: {currentEnergy}/{MaxEnergy}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Regenerate energy over time
        /// </summary>
        public void RegenerateEnergy()
        {
            if (energyConfig == null || currentEnergy >= MaxEnergy) return;
            
            // Check regen delay
            if (energyConfig.regenDelay > 0f && Time.time - lastConsumptionTime < energyConfig.regenDelay)
            {
                return;
            }
            
            float previousEnergy = currentEnergy;
            currentEnergy = Mathf.Min(MaxEnergy, currentEnergy + energyConfig.energyRegenRate * Time.deltaTime);
            
            // Only publish event if energy actually changed
            if (Mathf.Abs(currentEnergy - previousEnergy) > 0.01f)
            {
                PublishEnergyEvent(currentEnergy - previousEnergy, EnergyChangeReason.Regeneration);
            }
        }
        
        /// <summary>
        /// Force set energy to a specific value (for cheats/powerups)
        /// </summary>
        /// <param name="amount">Energy amount to set</param>
        public void SetEnergy(float amount)
        {
            float previousEnergy = currentEnergy;
            currentEnergy = Mathf.Clamp(amount, 0f, MaxEnergy);
            
            PublishEnergyEvent(currentEnergy - previousEnergy, EnergyChangeReason.ConfigurationChange);
            
            if (enableDebugLogs)
            {
                Debug.Log($"EnergySystem: Energy set to {currentEnergy}/{MaxEnergy}");
            }
        }
        
        /// <summary>
        /// Get energy as a normalized value (0-1)
        /// </summary>
        /// <returns>Energy percentage as float between 0 and 1</returns>
        public float GetEnergyNormalized()
        {
            return EnergyPercent;
        }
        
        private void PublishEnergyEvent(float energyDelta, EnergyChangeReason reason)
        {
            PlayerEventBus.PublishEnergyChanged(new EnergyChangedEvent(
                currentEnergy,
                MaxEnergy,
                energyDelta,
                reason
            ));
        }
        
        /// <summary>
        /// Validate configuration in editor
        /// </summary>
        private void OnValidate()
        {
            if (energyConfig != null && Application.isPlaying)
            {
                // Ensure current energy doesn't exceed max if config changed
                currentEnergy = Mathf.Min(currentEnergy, MaxEnergy);
            }
        }
    }
}
