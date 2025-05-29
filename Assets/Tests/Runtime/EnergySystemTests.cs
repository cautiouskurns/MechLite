using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MechLite.Energy;
using MechLite.Configuration;

/// <summary>
/// Basic unit tests for the EnergySystem
/// Focuses on core functionality without complex event testing or mocks
/// </summary>
public class EnergySystemTests
{
    private GameObject testGameObject;
    private EnergySystem energySystem;
    private EnergyConfigSO testConfig;
    
    [SetUp]
    public void SetUp()
    {
        // Create test configuration first
        testConfig = ScriptableObject.CreateInstance<EnergyConfigSO>();
        testConfig.maxEnergy = 100f;
        testConfig.energyRegenRate = 20f;
        testConfig.dashEnergyCost = 25f;
        testConfig.autoRegenerate = true;
        testConfig.regenDelay = 0.1f;
        
        // Create test game object and add EnergySystem component
        // Note: This will log an expected error in Awake() since config isn't assigned yet
        testGameObject = new GameObject("TestEnergySystem");
        
        // Expect the error message from Awake() when config is not assigned
        LogAssert.Expect(LogType.Error, "EnergySystem: EnergyConfigSO is not assigned!");
        energySystem = testGameObject.AddComponent<EnergySystem>();
        
        // Initialize the energy system with config
        energySystem.Initialize(testConfig);
    }
    
    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            Object.DestroyImmediate(testGameObject);
        }
        if (testConfig != null)
        {
            Object.DestroyImmediate(testConfig);
        }
    }
    
    [Test]
    public void EnergySystem_Initialization_SetsMaxEnergy()
    {
        // Test that energy system initializes with max energy
        Assert.AreEqual(100f, energySystem.MaxEnergy, "Max energy should be set from config");
        Assert.AreEqual(100f, energySystem.CurrentEnergy, "Current energy should start at max");
        Assert.AreEqual(1f, energySystem.EnergyPercent, 0.01f, "Energy percent should be 100%");
    }
    
    [Test]
    public void HasEnergy_WithSufficientEnergy_ReturnsTrue()
    {
        // Test HasEnergy returns true when there's enough energy
        Assert.IsTrue(energySystem.HasEnergy(25f), "Should have enough energy for dash cost");
        Assert.IsTrue(energySystem.HasEnergy(50f), "Should have enough energy for half capacity");
        Assert.IsTrue(energySystem.HasEnergy(100f), "Should have enough energy for full capacity");
    }
    
    [Test]
    public void HasEnergy_WithInsufficientEnergy_ReturnsFalse()
    {
        // Consume most energy first
        energySystem.ConsumeEnergy(90f);
        
        // Test HasEnergy returns false when there's not enough energy
        Assert.IsFalse(energySystem.HasEnergy(25f), "Should not have enough energy for dash after consuming 90");
        Assert.IsTrue(energySystem.HasEnergy(5f), "Should still have enough energy for small amount");
    }
    
    [Test]
    public void ConsumeEnergy_WithSufficientEnergy_ReducesCurrentEnergy()
    {
        // Test energy consumption reduces current energy
        bool consumed = energySystem.ConsumeEnergy(25f);
        
        Assert.IsTrue(consumed, "Energy consumption should succeed");
        Assert.AreEqual(75f, energySystem.CurrentEnergy, "Current energy should be reduced by consumed amount");
        Assert.AreEqual(0.75f, energySystem.EnergyPercent, 0.01f, "Energy percent should be 75%");
    }
    
    [Test]
    public void ConsumeEnergy_WithInsufficientEnergy_DoesNotConsumeAndReturnsFalse()
    {
        // First consume most energy
        energySystem.ConsumeEnergy(90f);
        
        // Try to consume more than available
        bool consumed = energySystem.ConsumeEnergy(25f);
        
        Assert.IsFalse(consumed, "Energy consumption should fail when insufficient");
        Assert.AreEqual(10f, energySystem.CurrentEnergy, "Current energy should remain unchanged");
    }
}
