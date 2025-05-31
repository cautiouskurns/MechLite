using UnityEngine;
using System.Collections.Generic;
using MechLite.Events;

namespace MechLite.Mech
{
    /// <summary>
    /// Manages energy for mech systems with stats integration and consumer management
    /// Provides energy consumption validation, regeneration, and event broadcasting
    /// Integrates with MechStats for dynamic MaxEnergy values
    /// </summary>
    public class EnergyManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("Mech configuration containing energy settings")]
        private MechConfigSO mechConfig;
        
        [Header("Debug")]
        [SerializeField, Tooltip("Enable detailed debug logging")]
        private bool enableDebugLogs = false;
        
        // Dependencies
        private MechStats mechStats;
        
        // Energy state
        private float currentEnergy;
        private float lastConsumptionTime;
        
        // Energy consumers tracking
        private readonly List<IEnergyConsumer> registeredConsumers = new List<IEnergyConsumer>();
        
        // Public properties
        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => mechStats?.GetStat(StatType.MaxEnergy) ?? mechConfig?.baseMaxEnergy ?? 100f;
        public float EnergyPercent => MaxEnergy > 0 ? currentEnergy / MaxEnergy : 0f;
        public bool IsRegenerating => Time.time - lastConsumptionTime >= RegenDelay;
        
        // Configuration properties
        private float RegenRate => mechConfig?.energyRegenRate ?? 20f;
        private float RegenDelay => mechConfig?.energyRegenDelay ?? 1f;
        
        // Events
        public System.Action<float, float, float> OnEnergyChanged; // currentEnergy, maxEnergy, delta
        public System.Action<IEnergyConsumer, float> OnEnergyConsumed; // consumer, amount
        public System.Action<IEnergyConsumer> OnEnergyConsumptionFailed; // consumer
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (mechConfig == null)
            {
                Debug.LogError("EnergyManager: MechConfigSO is not assigned!");
            }
        }
        
        private void Update()
        {
            RegenerateEnergy();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the energy manager with configuration and stats system
        /// </summary>
        /// <param name="config">Mech configuration</param>
        /// <param name="stats">Stats system for dynamic MaxEnergy</param>
        public void Initialize(MechConfigSO config, MechStats stats)
        {
            mechConfig = config;
            mechStats = stats;
            
            // Subscribe to MaxEnergy stat changes
            if (mechStats != null)
            {
                mechStats.OnStatChanged += OnMaxEnergyStatChanged;
            }
            
            // Initialize energy to maximum
            ResetEnergyToMax();
            
            PublishEnergyEvent(0f, EnergyChangeReason.Initialization);
            
            if (enableDebugLogs)
            {
                Debug.Log($"EnergyManager: Initialized with {currentEnergy}/{MaxEnergy} energy");
            }
        }
        
        /// <summary>
        /// Reset energy to maximum capacity
        /// </summary>
        public void ResetEnergyToMax()
        {
            float previousEnergy = currentEnergy;
            currentEnergy = MaxEnergy;
            float delta = currentEnergy - previousEnergy;
            
            OnEnergyChanged?.Invoke(currentEnergy, MaxEnergy, delta);
            
            if (enableDebugLogs && delta != 0)
            {
                Debug.Log($"EnergyManager: Energy reset to max - {currentEnergy}/{MaxEnergy}");
            }
        }
        
        #endregion
        
        #region Energy Management
        
        /// <summary>
        /// Check if sufficient energy is available for a given cost
        /// </summary>
        /// <param name="amount">Energy cost to check</param>
        /// <returns>True if sufficient energy is available</returns>
        public bool HasEnergy(float amount)
        {
            return currentEnergy >= amount;
        }
        
        /// <summary>
        /// Attempt to consume energy if available
        /// </summary>
        /// <param name="amount">Amount of energy to consume</param>
        /// <param name="consumer">The consumer requesting energy (optional for tracking)</param>
        /// <returns>True if energy was successfully consumed</returns>
        public bool TryConsumeEnergy(float amount, IEnergyConsumer consumer = null)
        {
            if (!HasEnergy(amount))
            {
                if (enableDebugLogs)
                {
                    string consumerName = consumer?.ConsumerName ?? "Unknown";
                    Debug.Log($"EnergyManager: Insufficient energy for {consumerName} - Required: {amount}, Available: {currentEnergy}");
                }
                
                OnEnergyConsumptionFailed?.Invoke(consumer);
                consumer?.OnEnergyConsumptionFailed();
                return false;
            }
            
            float previousEnergy = currentEnergy;
            currentEnergy = Mathf.Max(0f, currentEnergy - amount);
            lastConsumptionTime = Time.time;
            
            float delta = previousEnergy - currentEnergy;
            OnEnergyChanged?.Invoke(currentEnergy, MaxEnergy, -delta);
            OnEnergyConsumed?.Invoke(consumer, amount);
            
            PublishEnergyEvent(-delta, EnergyChangeReason.Consumption);
            
            if (enableDebugLogs)
            {
                string consumerName = consumer?.ConsumerName ?? "Unknown";
                Debug.Log($"EnergyManager: {consumerName} consumed {amount} energy. Current: {currentEnergy}/{MaxEnergy}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Force set energy to a specific value (for debugging/testing)
        /// </summary>
        /// <param name="amount">Energy amount to set</param>
        public void SetEnergy(float amount)
        {
            float previousEnergy = currentEnergy;
            currentEnergy = Mathf.Clamp(amount, 0f, MaxEnergy);
            float delta = currentEnergy - previousEnergy;
            
            OnEnergyChanged?.Invoke(currentEnergy, MaxEnergy, delta);
            PublishEnergyEvent(delta, EnergyChangeReason.ConfigurationChange);
            
            if (enableDebugLogs)
            {
                Debug.Log($"EnergyManager: Energy set to {currentEnergy}/{MaxEnergy}");
            }
        }
        
        /// <summary>
        /// Regenerate energy over time based on configuration
        /// </summary>
        private void RegenerateEnergy()
        {
            if (currentEnergy >= MaxEnergy) return;
            
            // Check regeneration delay
            if (Time.time - lastConsumptionTime < RegenDelay) return;
            
            float previousEnergy = currentEnergy;
            float regenAmount = RegenRate * Time.deltaTime;
            currentEnergy = Mathf.Min(MaxEnergy, currentEnergy + regenAmount);
            
            // Only notify if energy actually changed significantly
            float delta = currentEnergy - previousEnergy;
            if (delta > 0.01f)
            {
                OnEnergyChanged?.Invoke(currentEnergy, MaxEnergy, delta);
                PublishEnergyEvent(delta, EnergyChangeReason.Regeneration);
            }
        }
        
        #endregion
        
        #region Consumer Management
        
        /// <summary>
        /// Register an energy consumer for tracking and management
        /// </summary>
        /// <param name="consumer">Consumer to register</param>
        public void RegisterConsumer(IEnergyConsumer consumer)
        {
            if (consumer == null) return;
            
            if (!registeredConsumers.Contains(consumer))
            {
                registeredConsumers.Add(consumer);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"EnergyManager: Registered consumer '{consumer.ConsumerName}'");
                }
            }
        }
        
        /// <summary>
        /// Unregister an energy consumer
        /// </summary>
        /// <param name="consumer">Consumer to unregister</param>
        public void UnregisterConsumer(IEnergyConsumer consumer)
        {
            if (consumer == null) return;
            
            if (registeredConsumers.Remove(consumer))
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"EnergyManager: Unregistered consumer '{consumer.ConsumerName}'");
                }
            }
        }
        
        /// <summary>
        /// Get all registered energy consumers
        /// </summary>
        /// <returns>Read-only list of registered consumers</returns>
        public IReadOnlyList<IEnergyConsumer> GetRegisteredConsumers()
        {
            return registeredConsumers.AsReadOnly();
        }
        
        #endregion
        
        #region Event Handling
        
        private void OnMaxEnergyStatChanged(StatType statType, float newValue, float previousValue)
        {
            if (statType == StatType.MaxEnergy)
            {
                // Clamp current energy to new max if it exceeds
                if (currentEnergy > newValue)
                {
                    float previousEnergy = currentEnergy;
                    currentEnergy = newValue;
                    float delta = currentEnergy - previousEnergy;
                    
                    OnEnergyChanged?.Invoke(currentEnergy, MaxEnergy, delta);
                    
                    if (enableDebugLogs)
                    {
                        Debug.Log($"EnergyManager: MaxEnergy changed to {newValue}, current energy clamped to {currentEnergy}");
                    }
                }
            }
        }
        
        private void PublishEnergyEvent(float energyDelta, EnergyChangeReason reason)
        {
            // Check if PlayerEventBus exists to publish events (maintaining compatibility)
            var eventData = new EnergyChangedEvent(currentEnergy, MaxEnergy, energyDelta, reason);
            
            // Use reflection to check if PlayerEventBus exists and publish if available
            var eventBusType = System.Type.GetType("MechLite.Events.PlayerEventBus");
            if (eventBusType != null)
            {
                var publishMethod = eventBusType.GetMethod("PublishEnergyChanged");
                publishMethod?.Invoke(null, new object[] { eventData });
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Unsubscribe from stat changes
            if (mechStats != null)
            {
                mechStats.OnStatChanged -= OnMaxEnergyStatChanged;
            }
            
            // Clear all registered consumers
            registeredConsumers.Clear();
        }
        
        #endregion
        
        #region Validation
        
        private void OnValidate()
        {
            if (mechConfig != null && Application.isPlaying)
            {
                // Ensure current energy doesn't exceed max if config changed
                currentEnergy = Mathf.Min(currentEnergy, MaxEnergy);
            }
        }
        
        #endregion
    }
}
