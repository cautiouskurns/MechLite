using NUnit.Framework;
using UnityEngine;
using MechLite.Tests.Utilities;
using MechLite.Events;


namespace MechLite.Tests.Systems
{
    /// <summary>
    /// Unit tests for PlayerEventBus and event system
    /// </summary>
    public class PlayerEventBusTests
    {
        private EventCapture eventCapture;

        [SetUp]
        public void SetUp()
        {
            // Reset the singleton instance for clean testing
            PlayerEventBus.Instance?.ClearAllSubscriptions();
            eventCapture = new EventCapture();
        }

        [TearDown]
        public void TearDown()
        {
            eventCapture?.Unsubscribe();
            PlayerEventBus.Instance?.ClearAllSubscriptions();
        }

        #region Singleton Tests

        [Test]
        public void PlayerEventBus_IsSingleton()
        {
            var instance1 = PlayerEventBus.Instance;
            var instance2 = PlayerEventBus.Instance;

            Assert.AreSame(instance1, instance2, "PlayerEventBus should be a singleton");
            Assert.IsNotNull(instance1, "Singleton instance should not be null");
        }

        #endregion

        #region PlayerMovedEvent Tests

        [Test]
        public void PlayerMovedEvent_PublishesAndReceives()
        {
            eventCapture.Subscribe();
            var testEvent = new PlayerMovedEvent
            {
                Velocity = new Vector2(5f, 3f),
                IsGrounded = true,
                PlayerTransform = new GameObject("TestPlayer").transform
            };

            PlayerEventBus.Instance.PublishPlayerMoved(testEvent);

            Assert.AreEqual(1, eventCapture.MovedEvents.Count, "Should receive one PlayerMovedEvent");
            Assert.AreEqual(testEvent.Velocity, eventCapture.MovedEvents[0].Velocity, "Should preserve velocity data");
            Assert.AreEqual(testEvent.IsGrounded, eventCapture.MovedEvents[0].IsGrounded, "Should preserve grounded state");
            Assert.AreEqual(testEvent.PlayerTransform, eventCapture.MovedEvents[0].PlayerTransform, "Should preserve transform reference");

            Object.DestroyImmediate(testEvent.PlayerTransform.gameObject);
        }

        [Test]
        public void PlayerMovedEvent_MultipleSubscribers()
        {
            var capture1 = new EventCapture();
            var capture2 = new EventCapture();
            capture1.Subscribe();
            capture2.Subscribe();

            var testEvent = new PlayerMovedEvent
            {
                Velocity = Vector2.one,
                IsGrounded = false,
                PlayerTransform = new GameObject("TestPlayer").transform
            };

            PlayerEventBus.Instance.PublishPlayerMoved(testEvent);

            Assert.AreEqual(1, capture1.MovedEvents.Count, "First subscriber should receive event");
            Assert.AreEqual(1, capture2.MovedEvents.Count, "Second subscriber should receive event");

            capture1.Unsubscribe();
            capture2.Unsubscribe();
            Object.DestroyImmediate(testEvent.PlayerTransform.gameObject);
        }

        [Test]
        public void PlayerMovedEvent_UnsubscribeStopsReceiving()
        {
            eventCapture.Subscribe();
            eventCapture.Unsubscribe();

            var testEvent = new PlayerMovedEvent
            {
                Velocity = Vector2.zero,
                IsGrounded = true,
                PlayerTransform = new GameObject("TestPlayer").transform
            };

            PlayerEventBus.Instance.PublishPlayerMoved(testEvent);

            Assert.AreEqual(0, eventCapture.MovedEvents.Count, "Should not receive events after unsubscribing");

            Object.DestroyImmediate(testEvent.PlayerTransform.gameObject);
        }

        #endregion

        #region PlayerJumpedEvent Tests

        [Test]
        public void PlayerJumpedEvent_PublishesAndReceives()
        {
            eventCapture.Subscribe();
            var testEvent = new PlayerJumpedEvent
            {
                JumpForce = 15f,
                ResultingVelocity = new Vector2(0f, 15f),
                PlayerTransform = new GameObject("TestPlayer").transform
            };

            PlayerEventBus.Instance.PublishPlayerJumped(testEvent);

            Assert.AreEqual(1, eventCapture.JumpedEvents.Count, "Should receive one PlayerJumpedEvent");
            Assert.AreEqual(testEvent.JumpForce, eventCapture.JumpedEvents[0].JumpForce, "Should preserve jump force");
            Assert.AreEqual(testEvent.ResultingVelocity, eventCapture.JumpedEvents[0].ResultingVelocity, "Should preserve resulting velocity");
            Assert.AreEqual(testEvent.PlayerTransform, eventCapture.JumpedEvents[0].PlayerTransform, "Should preserve transform reference");

            Object.DestroyImmediate(testEvent.PlayerTransform.gameObject);
        }

        #endregion

        #region PlayerDashedEvent Tests

        [Test]
        public void PlayerDashedEvent_PublishesAndReceives()
        {
            eventCapture.Subscribe();
            var testEvent = new PlayerDashedEvent
            {
                Direction = Vector2.right,
                Force = 20f,
                ResultingVelocity = new Vector2(20f, 0f),
                PlayerTransform = new GameObject("TestPlayer").transform
            };

            PlayerEventBus.Instance.PublishPlayerDashed(testEvent);

            Assert.AreEqual(1, eventCapture.DashedEvents.Count, "Should receive one PlayerDashedEvent");
            Assert.AreEqual(testEvent.Direction, eventCapture.DashedEvents[0].Direction, "Should preserve direction");
            Assert.AreEqual(testEvent.Force, eventCapture.DashedEvents[0].Force, "Should preserve force");
            Assert.AreEqual(testEvent.ResultingVelocity, eventCapture.DashedEvents[0].ResultingVelocity, "Should preserve resulting velocity");
            Assert.AreEqual(testEvent.PlayerTransform, eventCapture.DashedEvents[0].PlayerTransform, "Should preserve transform reference");

            Object.DestroyImmediate(testEvent.PlayerTransform.gameObject);
        }

        #endregion

        #region EnergyChangedEvent Tests

        [Test]
        public void EnergyChangedEvent_PublishesAndReceives()
        {
            eventCapture.Subscribe();
            var testEvent = new EnergyChangedEvent
            {
                CurrentEnergy = 75f,
                MaxEnergy = 100f,
                EnergyPercent = 0.75f,
                PlayerTransform = new GameObject("TestPlayer").transform
            };

            PlayerEventBus.Instance.PublishEnergyChanged(testEvent);

            Assert.AreEqual(1, eventCapture.EnergyEvents.Count, "Should receive one EnergyChangedEvent");
            Assert.AreEqual(testEvent.CurrentEnergy, eventCapture.EnergyEvents[0].CurrentEnergy, "Should preserve current energy");
            Assert.AreEqual(testEvent.MaxEnergy, eventCapture.EnergyEvents[0].MaxEnergy, "Should preserve max energy");
            Assert.AreEqual(testEvent.EnergyPercent, eventCapture.EnergyEvents[0].EnergyPercent, "Should preserve energy percent");
            Assert.AreEqual(testEvent.PlayerTransform, eventCapture.EnergyEvents[0].PlayerTransform, "Should preserve transform reference");

            Object.DestroyImmediate(testEvent.PlayerTransform.gameObject);
        }

        #endregion

        #region GroundStateChangedEvent Tests

        [Test]
        public void GroundStateChangedEvent_PublishesAndReceives()
        {
            eventCapture.Subscribe();
            var testEvent = new GroundStateChangedEvent
            {
                IsGrounded = true,
                WasGrounded = false,
                PlayerTransform = new GameObject("TestPlayer").transform
            };

            PlayerEventBus.Instance.PublishGroundStateChanged(testEvent);

            Assert.AreEqual(1, eventCapture.GroundEvents.Count, "Should receive one GroundStateChangedEvent");
            Assert.AreEqual(testEvent.IsGrounded, eventCapture.GroundEvents[0].IsGrounded, "Should preserve grounded state");
            Assert.AreEqual(testEvent.WasGrounded, eventCapture.GroundEvents[0].WasGrounded, "Should preserve previous grounded state");
            Assert.AreEqual(testEvent.PlayerTransform, eventCapture.GroundEvents[0].PlayerTransform, "Should preserve transform reference");

            Object.DestroyImmediate(testEvent.PlayerTransform.gameObject);
        }

        #endregion

        #region Event Isolation Tests

        [Test]
        public void EventBus_IsolatesEventTypes()
        {
            eventCapture.Subscribe();

            // Publish different event types
            PlayerEventBus.Instance.PublishPlayerMoved(new PlayerMovedEvent
            {
                Velocity = Vector2.one,
                IsGrounded = true,
                PlayerTransform = new GameObject("TestPlayer1").transform
            });

            PlayerEventBus.Instance.PublishEnergyChanged(new EnergyChangedEvent
            {
                CurrentEnergy = 50f,
                MaxEnergy = 100f,
                EnergyPercent = 0.5f,
                PlayerTransform = new GameObject("TestPlayer2").transform
            });

            // Each event type should only have its own events
            Assert.AreEqual(1, eventCapture.MovedEvents.Count, "Should receive exactly one movement event");
            Assert.AreEqual(1, eventCapture.EnergyEvents.Count, "Should receive exactly one energy event");
            Assert.AreEqual(0, eventCapture.JumpedEvents.Count, "Should not receive jump events");
            Assert.AreEqual(0, eventCapture.DashedEvents.Count, "Should not receive dash events");
            Assert.AreEqual(0, eventCapture.GroundEvents.Count, "Should not receive ground events");

            Object.DestroyImmediate(eventCapture.MovedEvents[0].PlayerTransform.gameObject);
            Object.DestroyImmediate(eventCapture.EnergyEvents[0].PlayerTransform.gameObject);
        }

        #endregion

        #region Performance Tests

        [Test]
        public void EventBus_HandlesMultipleEvents()
        {
            eventCapture.Subscribe();
            var playerTransform = new GameObject("TestPlayer").transform;

            // Publish many events rapidly
            for (int i = 0; i < 1000; i++)
            {
                PlayerEventBus.Instance.PublishPlayerMoved(new PlayerMovedEvent
                {
                    Velocity = new Vector2(i, i),
                    IsGrounded = i % 2 == 0,
                    PlayerTransform = playerTransform
                });
            }

            Assert.AreEqual(1000, eventCapture.MovedEvents.Count, "Should receive all published events");

            // Verify data integrity of first and last events
            Assert.AreEqual(new Vector2(0, 0), eventCapture.MovedEvents[0].Velocity, "First event should have correct data");
            Assert.AreEqual(new Vector2(999, 999), eventCapture.MovedEvents[999].Velocity, "Last event should have correct data");

            Object.DestroyImmediate(playerTransform.gameObject);
        }

        [Test]
        public void EventBus_HandlesNoSubscribers()
        {
            // Don't subscribe to any events
            var testEvent = new PlayerMovedEvent
            {
                Velocity = Vector2.one,
                IsGrounded = true,
                PlayerTransform = new GameObject("TestPlayer").transform
            };

            Assert.DoesNotThrow(() => PlayerEventBus.Instance.PublishPlayerMoved(testEvent),
                "Should handle publishing with no subscribers gracefully");

            Object.DestroyImmediate(testEvent.PlayerTransform.gameObject);
        }

        #endregion

        #region Edge Cases

        [Test]
        public void EventBus_HandlesNullEventData()
        {
            eventCapture.Subscribe();

            // Test with null transform (should not crash)
            var testEvent = new PlayerMovedEvent
            {
                Velocity = Vector2.one,
                IsGrounded = true,
                PlayerTransform = null
            };

            Assert.DoesNotThrow(() => PlayerEventBus.Instance.PublishPlayerMoved(testEvent),
                "Should handle null transform gracefully");

            Assert.AreEqual(1, eventCapture.MovedEvents.Count, "Should still receive event with null transform");
            Assert.IsNull(eventCapture.MovedEvents[0].PlayerTransform, "Should preserve null transform");
        }

        [Test]
        public void EventBus_HandlesExtremeValues()
        {
            eventCapture.Subscribe();
            var playerTransform = new GameObject("TestPlayer").transform;

            var testEvent = new PlayerMovedEvent
            {
                Velocity = new Vector2(float.MaxValue, float.MinValue),
                IsGrounded = true,
                PlayerTransform = playerTransform
            };

            PlayerEventBus.Instance.PublishPlayerMoved(testEvent);

            Assert.AreEqual(testEvent.Velocity, eventCapture.MovedEvents[0].Velocity, "Should handle extreme velocity values");

            Object.DestroyImmediate(playerTransform.gameObject);
        }

        [Test]
        public void EventBus_ClearAllSubscriptions()
        {
            eventCapture.Subscribe();

            PlayerEventBus.Instance.ClearAllSubscriptions();

            var testEvent = new PlayerMovedEvent
            {
                Velocity = Vector2.one,
                IsGrounded = true,
                PlayerTransform = new GameObject("TestPlayer").transform
            };

            PlayerEventBus.Instance.PublishPlayerMoved(testEvent);

            Assert.AreEqual(0, eventCapture.MovedEvents.Count, "Should not receive events after clearing subscriptions");

            Object.DestroyImmediate(testEvent.PlayerTransform.gameObject);
        }

        #endregion
    }
}
