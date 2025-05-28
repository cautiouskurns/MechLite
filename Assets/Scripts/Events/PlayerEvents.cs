using UnityEngine;

namespace MechSalvager.Events
{
    /// <summary>
    /// Event data for player movement actions
    /// </summary>
    [System.Serializable]
    public struct PlayerMovedEvent
    {
        public Vector2 velocity;
        public Vector2 position;
        public float horizontalInput;
        public bool isGrounded;
        
        public PlayerMovedEvent(Vector2 velocity, Vector2 position, float horizontalInput, bool isGrounded)
        {
            this.velocity = velocity;
            this.position = position;
            this.horizontalInput = horizontalInput;
            this.isGrounded = isGrounded;
        }
    }
    
    /// <summary>
    /// Event data for player jump actions
    /// </summary>
    [System.Serializable]
    public struct PlayerJumpedEvent
    {
        public Vector2 jumpVelocity;
        public Vector2 position;
        public bool usedCoyoteTime;
        public bool usedJumpBuffer;
        
        public PlayerJumpedEvent(Vector2 jumpVelocity, Vector2 position, bool usedCoyoteTime, bool usedJumpBuffer)
        {
            this.jumpVelocity = jumpVelocity;
            this.position = position;
            this.usedCoyoteTime = usedCoyoteTime;
            this.usedJumpBuffer = usedJumpBuffer;
        }
    }
    
    /// <summary>
    /// Event data for player dash actions
    /// </summary>
    [System.Serializable]
    public struct PlayerDashedEvent
    {
        public Vector2 dashDirection;
        public float dashForce;
        public Vector2 position;
        public float energyConsumed;
        public float remainingEnergy;
        
        public PlayerDashedEvent(Vector2 dashDirection, float dashForce, Vector2 position, float energyConsumed, float remainingEnergy)
        {
            this.dashDirection = dashDirection;
            this.dashForce = dashForce;
            this.position = position;
            this.energyConsumed = energyConsumed;
            this.remainingEnergy = remainingEnergy;
        }
    }
    
    /// <summary>
    /// Event data for energy system changes
    /// </summary>
    [System.Serializable]
    public struct EnergyChangedEvent
    {
        public float currentEnergy;
        public float maxEnergy;
        public float energyPercent;
        public float energyDelta;
        public EnergyChangeReason reason;
        
        public EnergyChangedEvent(float currentEnergy, float maxEnergy, float energyDelta, EnergyChangeReason reason)
        {
            this.currentEnergy = currentEnergy;
            this.maxEnergy = maxEnergy;
            this.energyPercent = currentEnergy / maxEnergy;
            this.energyDelta = energyDelta;
            this.reason = reason;
        }
    }
    
    /// <summary>
    /// Reasons for energy changes
    /// </summary>
    public enum EnergyChangeReason
    {
        Consumption,
        Regeneration,
        Initialization,
        ConfigurationChange
    }
    
    /// <summary>
    /// Event data for ground state changes
    /// </summary>
    [System.Serializable]
    public struct GroundStateChangedEvent
    {
        public bool isGrounded;
        public bool wasGrounded;
        public Vector2 position;
        public float timeSinceLastGrounded;
        
        public GroundStateChangedEvent(bool isGrounded, bool wasGrounded, Vector2 position, float timeSinceLastGrounded)
        {
            this.isGrounded = isGrounded;
            this.wasGrounded = wasGrounded;
            this.position = position;
            this.timeSinceLastGrounded = timeSinceLastGrounded;
        }
    }
}
