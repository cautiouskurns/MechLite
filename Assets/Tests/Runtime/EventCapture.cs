using UnityEngine;
using System.Collections.Generic;

namespace MechLite.Tests.Utilities
{
    /// <summary>
    /// Test utility for capturing and verifying events
    /// </summary>
    public class EventCapture
    {
        private List<PlayerMovedEvent> movedEvents = new List<PlayerMovedEvent>();
        private List<PlayerJumpedEvent> jumpedEvents = new List<PlayerJumpedEvent>();
        private List<PlayerDashedEvent> dashedEvents = new List<PlayerDashedEvent>();
        private List<EnergyChangedEvent> energyEvents = new List<EnergyChangedEvent>();
        private List<GroundStateChangedEvent> groundEvents = new List<GroundStateChangedEvent>();

        public IReadOnlyList<PlayerMovedEvent> MovedEvents => movedEvents.AsReadOnly();
        public IReadOnlyList<PlayerJumpedEvent> JumpedEvents => jumpedEvents.AsReadOnly();
        public IReadOnlyList<PlayerDashedEvent> DashedEvents => dashedEvents.AsReadOnly();
        public IReadOnlyList<EnergyChangedEvent> EnergyEvents => energyEvents.AsReadOnly();
        public IReadOnlyList<GroundStateChangedEvent> GroundEvents => groundEvents.AsReadOnly();

        public void Subscribe()
        {
            PlayerEventBus.Instance.OnPlayerMoved += OnPlayerMoved;
            PlayerEventBus.Instance.OnPlayerJumped += OnPlayerJumped;
            PlayerEventBus.Instance.OnPlayerDashed += OnPlayerDashed;
            PlayerEventBus.Instance.OnEnergyChanged += OnEnergyChanged;
            PlayerEventBus.Instance.OnGroundStateChanged += OnGroundStateChanged;
        }

        public void Unsubscribe()
        {
            if (PlayerEventBus.Instance != null)
            {
                PlayerEventBus.Instance.OnPlayerMoved -= OnPlayerMoved;
                PlayerEventBus.Instance.OnPlayerJumped -= OnPlayerJumped;
                PlayerEventBus.Instance.OnPlayerDashed -= OnPlayerDashed;
                PlayerEventBus.Instance.OnEnergyChanged -= OnEnergyChanged;
                PlayerEventBus.Instance.OnGroundStateChanged -= OnGroundStateChanged;
            }
        }

        public void Clear()
        {
            movedEvents.Clear();
            jumpedEvents.Clear();
            dashedEvents.Clear();
            energyEvents.Clear();
            groundEvents.Clear();
        }

        private void OnPlayerMoved(PlayerMovedEvent evt) => movedEvents.Add(evt);
        private void OnPlayerJumped(PlayerJumpedEvent evt) => jumpedEvents.Add(evt);
        private void OnPlayerDashed(PlayerDashedEvent evt) => dashedEvents.Add(evt);
        private void OnEnergyChanged(EnergyChangedEvent evt) => energyEvents.Add(evt);
        private void OnGroundStateChanged(GroundStateChangedEvent evt) => groundEvents.Add(evt);
    }
}