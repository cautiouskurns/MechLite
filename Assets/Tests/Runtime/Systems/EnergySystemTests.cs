using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using MechLite.Tests.Utilities;
using MechLite.Energy;
using MechLite.Tests.Mocks;


namespace MechLite.Tests.Systems
{
    /// <summary>
    /// Unit tests for EnergySystem component
    /// </summary>
    public class EnergySystemTests
    {
        private GameObject testObject;
        private EnergySystem energySystem;
        private EnergyConfigSO testConfig;
        private EventCapture eventCapture;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestEnergySystem");
            energySystem = testObject.AddComponent<EnergySystem>();
            testConfig = TestConfigurationFactory.CreateTestEnergyConfig();
            eventCapture = new EventCapture();
            eventCapture.Subscribe();
        }

        [TearDown]
        public void TearDown()
        {
            eventCapture.Unsubscribe();
            if (testObject != null)
                Object.DestroyImmediate(testObject);
            if (testConfig != null)
                Object.DestroyImmediate(testConfig);
        }

        #region Interface Contract Tests

        [Test]
        public void EnergySystem_ImplementsIEnergyUser()
        {
            Assert.IsTrue(energySystem is IEnergyUser, "EnergySystem should implement IEnergyUser interface");
        }

        [Test]
        public void CurrentEnergy_ReturnsCorrectValue()
        {
            energySystem.Initialize(testConfig);
            Assert.AreEqual(testConfig.maxEnergy, energySystem.CurrentEnergy, "CurrentEnergy should equal maxEnergy after initialization");
        }

        [Test]
        public void MaxEnergy_ReturnsConfigValue()
        {
            energySystem.Initialize(testConfig);
            Assert.AreEqual(testConfig.maxEnergy, energySystem.MaxEnergy, "MaxEnergy should return config value");
        }

        [Test]
        public void EnergyPercent_CalculatesCorrectly()
        {
            energySystem.Initialize(testConfig);
            Assert.AreEqual(1f, energySystem.EnergyPercent, 0.01f, "EnergyPercent should be 1.0 when at max energy");
            
            energySystem.ConsumeEnergy(50f);
            Assert.AreEqual(0.5f, energySystem.EnergyPercent, 0.01f, "EnergyPercent should be 0.5 when at half energy");
        }

        #endregion

        #region Energy Consumption Tests

        [Test]
        public void HasEnergy_ReturnsTrueWhenSufficientEnergy()
        {
            energySystem.Initialize(testConfig);
            Assert.IsTrue(energySystem.HasEnergy(50f), "Should have sufficient energy for 50 units");
            Assert.IsTrue(energySystem.HasEnergy(100f), "Should have sufficient energy for 100 units");
        }

        [Test]
        public void HasEnergy_ReturnsFalseWhenInsufficientEnergy()
        {
            energySystem.Initialize(testConfig);
            energySystem.ConsumeEnergy(75f);
            Assert.IsFalse(energySystem.HasEnergy(50f), "Should not have sufficient energy for 50 units after consuming 75");
        }

        [Test]
        public void ConsumeEnergy_SucceedsWhenSufficientEnergy()
        {
            energySystem.Initialize(testConfig);
            bool result = energySystem.ConsumeEnergy(30f);
            
            Assert.IsTrue(result, "ConsumeEnergy should return true when sufficient energy available");
            Assert.AreEqual(70f, energySystem.CurrentEnergy, "Current energy should be reduced by consumed amount");
        }

        [Test]
        public void ConsumeEnergy_FailsWhenInsufficientEnergy()
        {
            energySystem.Initialize(testConfig);
            energySystem.ConsumeEnergy(80f);
            bool result = energySystem.ConsumeEnergy(30f);
            
            Assert.IsFalse(result, "ConsumeEnergy should return false when insufficient energy");
            Assert.AreEqual(20f, energySystem.CurrentEnergy, "Energy should not be consumed when insufficient");
        }

        [Test]
        public void ConsumeEnergy_ClampsToZero()
        {
            energySystem.Initialize(testConfig);
            energySystem.ConsumeEnergy(150f); // More than max
            
            Assert.AreEqual(0f, energySystem.CurrentEnergy, "Energy should be clamped to zero");
        }

        #endregion

        #region Energy Regeneration Tests

        [Test]
        public void RegenerateEnergy_IncreasesCurrentEnergy()
        {
            energySystem.Initialize(testConfig);
            energySystem.ConsumeEnergy(50f);
            energySystem.RegenerateEnergy(20f);
            
            Assert.AreEqual(70f, energySystem.CurrentEnergy, "Energy should increase by regenerated amount");
        }

        [Test]
        public void RegenerateEnergy_ClampsToMaxEnergy()
        {
            energySystem.Initialize(testConfig);
            energySystem.ConsumeEnergy(10f);
            energySystem.RegenerateEnergy(50f); // More than needed to reach max
            
            Assert.AreEqual(testConfig.maxEnergy, energySystem.CurrentEnergy, "Energy should be clamped to max energy");
        }

        [UnityTest]
        public IEnumerator AutoRegeneration_RegeneratesOverTime()
        {
            energySystem.Initialize(testConfig);
            energySystem.ConsumeEnergy(40f);
            
            float initialEnergy = energySystem.CurrentEnergy;
            
            // Wait for regeneration delay plus some extra time
            yield return new WaitForSeconds(testConfig.regenerationDelay + 0.5f);
            
            Assert.Greater(energySystem.CurrentEnergy, initialEnergy, "Energy should regenerate over time");
        }

        [UnityTest]
        public IEnumerator AutoRegeneration_DoesNotExceedMaxEnergy()
        {
            energySystem.Initialize(testConfig);
            energySystem.ConsumeEnergy(10f);
            
            // Wait long enough for full regeneration
            yield return new WaitForSeconds(testConfig.regenerationDelay + 2f);
            
            Assert.AreEqual(testConfig.maxEnergy, energySystem.CurrentEnergy, 0.1f, "Energy should not exceed max during auto-regeneration");
        }

        #endregion

        #region Event System Tests

        [Test]
        public void ConsumeEnergy_PublishesEnergyChangedEvent()
        {
            energySystem.Initialize(testConfig);
            eventCapture.Clear();
            
            energySystem.ConsumeEnergy(25f);
            
            Assert.AreEqual(1, eventCapture.EnergyEvents.Count, "Should publish one EnergyChangedEvent");
            Assert.AreEqual(75f, eventCapture.EnergyEvents[0].CurrentEnergy, "Event should contain correct current energy");
            Assert.AreEqual(100f, eventCapture.EnergyEvents[0].MaxEnergy, "Event should contain correct max energy");
            Assert.AreEqual(0.75f, eventCapture.EnergyEvents[0].EnergyPercent, 0.01f, "Event should contain correct energy percent");
        }

        [Test]
        public void RegenerateEnergy_PublishesEnergyChangedEvent()
        {
            energySystem.Initialize(testConfig);
            energySystem.ConsumeEnergy(50f);
            eventCapture.Clear();
            
            energySystem.RegenerateEnergy(20f);
            
            Assert.AreEqual(1, eventCapture.EnergyEvents.Count, "Should publish one EnergyChangedEvent");
            Assert.AreEqual(70f, eventCapture.EnergyEvents[0].CurrentEnergy, "Event should contain correct current energy after regeneration");
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void Initialize_SetsConfigurationCorrectly()
        {
            energySystem.Initialize(testConfig);
            
            Assert.AreEqual(testConfig.maxEnergy, energySystem.MaxEnergy, "Max energy should match config");
            Assert.AreEqual(testConfig.maxEnergy, energySystem.CurrentEnergy, "Should start at max energy");
        }

        [Test]
        public void Initialize_WithNullConfig_HandlesGracefully()
        {
            Assert.DoesNotThrow(() => energySystem.Initialize(null), "Should handle null config gracefully");
        }

        #endregion

        #region Edge Cases

        [Test]
        public void ConsumeEnergy_WithZeroAmount_DoesNothing()
        {
            energySystem.Initialize(testConfig);
            float initialEnergy = energySystem.CurrentEnergy;
            
            bool result = energySystem.ConsumeEnergy(0f);
            
            Assert.IsTrue(result, "Consuming zero energy should succeed");
            Assert.AreEqual(initialEnergy, energySystem.CurrentEnergy, "Energy should remain unchanged");
        }

        [Test]
        public void ConsumeEnergy_WithNegativeAmount_DoesNothing()
        {
            energySystem.Initialize(testConfig);
            float initialEnergy = energySystem.CurrentEnergy;
            
            bool result = energySystem.ConsumeEnergy(-10f);
            
            Assert.IsFalse(result, "Consuming negative energy should fail");
            Assert.AreEqual(initialEnergy, energySystem.CurrentEnergy, "Energy should remain unchanged");
        }

        [Test]
        public void RegenerateEnergy_WithZeroAmount_DoesNothing()
        {
            energySystem.Initialize(testConfig);
            energySystem.ConsumeEnergy(20f);
            float energyAfterConsumption = energySystem.CurrentEnergy;
            
            energySystem.RegenerateEnergy(0f);
            
            Assert.AreEqual(energyAfterConsumption, energySystem.CurrentEnergy, "Energy should remain unchanged when regenerating zero");
        }

        #endregion
    }
}
