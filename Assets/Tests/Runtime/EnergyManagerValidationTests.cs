using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using MechLite.Mech;

namespace MechLite.Tests
{
    /// <summary>
    /// Comprehensive validation tests for the Mech Energy Management System
    /// Tests energy consumption, regeneration, stats integration, and consumer management
    /// </summary>
    public class EnergyManagerValidationTests
    {
        private GameObject testGameObject;
        private EnergyManager energyManager;
        private MechConfigSO testConfig;
        private MechStats testStats;
        private TestEnergyConsumer testConsumer;
        
        [SetUp]
        public void SetUp()
        {
            // Create test configuration
            testConfig = ScriptableObject.CreateInstance<MechConfigSO>();
            testConfig.baseMaxEnergy = 100f;
            testConfig.energyRegenRate = 20f;
            testConfig.energyRegenDelay = 1f;
            
            // Create test stats system
            testStats = new MechStats();
            testStats.Initialize(testConfig);
            
            // Create test game object and add EnergyManager
            testGameObject = new GameObject("TestEnergyManager");
            energyManager = testGameObject.AddComponent<EnergyManager>();
            
            // Create test consumer
            testConsumer = new TestEnergyConsumer("TestConsumer", 25f);
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
                Object.DestroyImmediate(testGameObject);
            if (testConfig != null)
                Object.DestroyImmediate(testConfig);
        }
        
        #region Initialization Tests
        
        [Test]
        public void EnergyManager_Initialization_StartsAtMaxEnergy()
        {
            // Act
            energyManager.Initialize(testConfig, testStats);
            
            // Assert
            Assert.AreEqual(100f, energyManager.MaxEnergy, "MaxEnergy should match stats system");
            Assert.AreEqual(100f, energyManager.CurrentEnergy, "CurrentEnergy should start at max");
            Assert.AreEqual(1f, energyManager.EnergyPercent, 0.01f, "EnergyPercent should be 100%");
        }
        
        [Test]
        public void EnergyManager_WithoutStatsSystem_UsesConfigValues()
        {
            // Act
            energyManager.Initialize(testConfig, null);
            
            // Assert
            Assert.AreEqual(100f, energyManager.MaxEnergy, "MaxEnergy should fallback to config value");
            Assert.AreEqual(100f, energyManager.CurrentEnergy, "CurrentEnergy should start at max");
        }
        
        #endregion
        
        #region Energy Consumption Tests
        
        [Test]
        public void TryConsumeEnergy_WithSufficientEnergy_ReturnsTrue()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            
            // Act
            bool consumed = energyManager.TryConsumeEnergy(25f, testConsumer);
            
            // Assert
            Assert.IsTrue(consumed, "Energy consumption should succeed");
            Assert.AreEqual(75f, energyManager.CurrentEnergy, "Current energy should be reduced");
            Assert.AreEqual(0.75f, energyManager.EnergyPercent, 0.01f, "Energy percent should be 75%");
        }
        
        [Test]
        public void TryConsumeEnergy_WithInsufficientEnergy_ReturnsFalse()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            energyManager.TryConsumeEnergy(90f); // Consume most energy
            
            // Act
            bool consumed = energyManager.TryConsumeEnergy(25f, testConsumer);
            
            // Assert
            Assert.IsFalse(consumed, "Energy consumption should fail");
            Assert.AreEqual(10f, energyManager.CurrentEnergy, "Current energy should remain unchanged");
            Assert.IsTrue(testConsumer.ConsumptionFailedCalled, "Consumer should be notified of failure");
        }
        
        [Test]
        public void HasEnergy_WithSufficientEnergy_ReturnsTrue()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            
            // Act & Assert
            Assert.IsTrue(energyManager.HasEnergy(25f), "Should have sufficient energy");
            Assert.IsTrue(energyManager.HasEnergy(100f), "Should have energy for full capacity");
        }
        
        [Test]
        public void HasEnergy_WithInsufficientEnergy_ReturnsFalse()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            energyManager.TryConsumeEnergy(90f);
            
            // Act & Assert
            Assert.IsFalse(energyManager.HasEnergy(25f), "Should not have sufficient energy");
            Assert.IsTrue(energyManager.HasEnergy(5f), "Should have energy for small amount");
        }
        
        #endregion
        
        #region Energy Regeneration Tests
        
        [Test]
        public IEnumerator EnergyManager_Regeneration_WaitsForDelay()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            energyManager.TryConsumeEnergy(50f);
            float energyAfterConsumption = energyManager.CurrentEnergy;
            
            // Act - Wait less than regen delay
            yield return new WaitForSeconds(0.5f);
            
            // Assert - Should not have regenerated yet
            Assert.AreEqual(energyAfterConsumption, energyManager.CurrentEnergy, 0.1f, 
                "Energy should not regenerate before delay period");
            Assert.IsFalse(energyManager.IsRegenerating, "Should not be regenerating yet");
        }
        
        [Test]
        public IEnumerator EnergyManager_Regeneration_StartsAfterDelay()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            energyManager.TryConsumeEnergy(50f);
            
            // Act - Wait for regen delay + small buffer
            yield return new WaitForSeconds(1.2f);
            
            // Assert
            Assert.IsTrue(energyManager.IsRegenerating, "Should be regenerating after delay");
            Assert.Greater(energyManager.CurrentEnergy, 50f, "Energy should have started regenerating");
        }
        
        [Test]
        public IEnumerator EnergyManager_Regeneration_StopsAtMaxEnergy()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            energyManager.TryConsumeEnergy(20f); // Small consumption for quick test
            
            // Act - Wait for full regeneration
            yield return new WaitForSeconds(3f);
            
            // Assert
            Assert.AreEqual(100f, energyManager.CurrentEnergy, 0.1f, "Energy should cap at max");
            Assert.AreEqual(1f, energyManager.EnergyPercent, 0.01f, "Energy percent should be 100%");
        }
        
        #endregion
        
        #region Stats Integration Tests
        
        [Test]
        public void EnergyManager_MaxEnergyStatChange_UpdatesMaxEnergy()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            
            // Act
            testStats.AddModifier(new StatModifier(StatType.MaxEnergy, 50f, ModifierType.Additive, "Test Upgrade"));
            
            // Assert
            Assert.AreEqual(150f, energyManager.MaxEnergy, "MaxEnergy should reflect stat changes");
        }
        
        [Test]
        public void EnergyManager_MaxEnergyDecrease_ClampsCurrentEnergy()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            
            // Act
            testStats.AddModifier(new StatModifier(StatType.MaxEnergy, -50f, ModifierType.Additive, "Debuff"));
            
            // Assert
            Assert.AreEqual(50f, energyManager.MaxEnergy, "MaxEnergy should be reduced");
            Assert.LessOrEqual(energyManager.CurrentEnergy, energyManager.MaxEnergy, 
                "Current energy should be clamped to new max");
        }
        
        #endregion
        
        #region Consumer Management Tests
        
        [Test]
        public void RegisterConsumer_AddsConsumerToList()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            
            // Act
            energyManager.RegisterConsumer(testConsumer);
            
            // Assert
            var consumers = energyManager.GetRegisteredConsumers();
            Assert.IsTrue(consumers.Contains(testConsumer), "Consumer should be registered");
            Assert.AreEqual(1, consumers.Count, "Should have exactly one consumer");
        }
        
        [Test]
        public void UnregisterConsumer_RemovesConsumerFromList()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            energyManager.RegisterConsumer(testConsumer);
            
            // Act
            energyManager.UnregisterConsumer(testConsumer);
            
            // Assert
            var consumers = energyManager.GetRegisteredConsumers();
            Assert.IsFalse(consumers.Contains(testConsumer), "Consumer should be unregistered");
            Assert.AreEqual(0, consumers.Count, "Should have no consumers");
        }
        
        [Test]
        public void RegisterConsumer_DuplicateConsumer_DoesNotAddTwice()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            
            // Act
            energyManager.RegisterConsumer(testConsumer);
            energyManager.RegisterConsumer(testConsumer); // Register again
            
            // Assert
            var consumers = energyManager.GetRegisteredConsumers();
            Assert.AreEqual(1, consumers.Count, "Should not register duplicate consumers");
        }
        
        #endregion
        
        #region Event System Tests
        
        [Test]
        public void EnergyManager_EnergyConsumption_FiresEvents()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            bool energyChangedFired = false;
            bool energyConsumedFired = false;
            
            energyManager.OnEnergyChanged += (current, max, delta) => energyChangedFired = true;
            energyManager.OnEnergyConsumed += (consumer, amount) => energyConsumedFired = true;
            
            // Act
            energyManager.TryConsumeEnergy(25f, testConsumer);
            
            // Assert
            Assert.IsTrue(energyChangedFired, "OnEnergyChanged event should fire");
            Assert.IsTrue(energyConsumedFired, "OnEnergyConsumed event should fire");
        }
        
        [Test]
        public void EnergyManager_FailedConsumption_FiresFailureEvent()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            energyManager.TryConsumeEnergy(90f); // Consume most energy
            bool failureEventFired = false;
            
            energyManager.OnEnergyConsumptionFailed += (consumer) => failureEventFired = true;
            
            // Act
            energyManager.TryConsumeEnergy(25f, testConsumer);
            
            // Assert
            Assert.IsTrue(failureEventFired, "OnEnergyConsumptionFailed event should fire");
        }
        
        #endregion
        
        #region Configuration Tests
        
        [Test]
        public void SetEnergy_ClampsToValidRange()
        {
            // Arrange
            energyManager.Initialize(testConfig, testStats);
            
            // Act & Assert
            energyManager.SetEnergy(-10f);
            Assert.AreEqual(0f, energyManager.CurrentEnergy, "Energy should not go below 0");
            
            energyManager.SetEnergy(150f);
            Assert.AreEqual(100f, energyManager.CurrentEnergy, "Energy should not exceed max");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Test implementation of IEnergyConsumer for validation testing
    /// </summary>
    public class TestEnergyConsumer : IEnergyConsumer
    {
        public string ConsumerName { get; private set; }
        public float EnergyCost { get; private set; }
        public bool CanConsumeEnergy { get; set; } = true;
        public bool ConsumptionFailedCalled { get; private set; }
        
        public TestEnergyConsumer(string name, float cost)
        {
            ConsumerName = name;
            EnergyCost = cost;
        }
        
        public bool TryConsumeEnergy(float availableEnergy)
        {
            return availableEnergy >= EnergyCost;
        }
        
        public void OnEnergyConsumptionFailed()
        {
            ConsumptionFailedCalled = true;
        }
    }
}
