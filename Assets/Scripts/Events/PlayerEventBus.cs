using System;
using UnityEngine;

namespace MechSalvager.Events
{
    /// <summary>
    /// Simple event system for decoupled communication between player systems
    /// Uses Unity Events for inspector integration and type safety
    /// </summary>
    public static class PlayerEventBus
    {
        // Movement Events
        public static event Action<PlayerMovedEvent> OnPlayerMoved;
        public static event Action<PlayerJumpedEvent> OnPlayerJumped;
        public static event Action<PlayerDashedEvent> OnPlayerDashed;
        
        // Energy Events
        public static event Action<EnergyChangedEvent> OnEnergyChanged;
        
        // Ground Events
        public static event Action<GroundStateChangedEvent> OnGroundStateChanged;
        
        // Movement Event Publishers
        public static void PublishPlayerMoved(PlayerMovedEvent eventData)
        {
            OnPlayerMoved?.Invoke(eventData);
        }
        
        public static void PublishPlayerJumped(PlayerJumpedEvent eventData)
        {
            OnPlayerJumped?.Invoke(eventData);
        }
        
        public static void PublishPlayerDashed(PlayerDashedEvent eventData)
        {
            OnPlayerDashed?.Invoke(eventData);
        }
        
        // Energy Event Publishers
        public static void PublishEnergyChanged(EnergyChangedEvent eventData)
        {
            OnEnergyChanged?.Invoke(eventData);
        }
        
        // Ground Event Publishers
        public static void PublishGroundStateChanged(GroundStateChangedEvent eventData)
        {
            OnGroundStateChanged?.Invoke(eventData);
        }
        
        /// <summary>
        /// Clear all event subscriptions (useful for cleanup)
        /// </summary>
        public static void ClearAllEvents()
        {
            OnPlayerMoved = null;
            OnPlayerJumped = null;
            OnPlayerDashed = null;
            OnEnergyChanged = null;
            OnGroundStateChanged = null;
        }
        
        /// <summary>
        /// Debug method to log current event subscription counts
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void LogEventSubscriptions()
        {
            Debug.Log($"PlayerEventBus Subscriptions - " +
                     $"PlayerMoved: {OnPlayerMoved?.GetInvocationList().Length ?? 0}, " +
                     $"PlayerJumped: {OnPlayerJumped?.GetInvocationList().Length ?? 0}, " +
                     $"PlayerDashed: {OnPlayerDashed?.GetInvocationList().Length ?? 0}, " +
                     $"EnergyChanged: {OnEnergyChanged?.GetInvocationList().Length ?? 0}, " +
                     $"GroundStateChanged: {OnGroundStateChanged?.GetInvocationList().Length ?? 0}");
        }
    }
}
