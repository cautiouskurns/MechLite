using UnityEngine;

namespace MechLite.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IMovable for testing
    /// </summary>
    public class MockMovable : MonoBehaviour, IMovable
    {
        [SerializeField] private Vector2 velocity = Vector2.zero;
        [SerializeField] private bool isGrounded = true;

        public Vector2 Velocity => velocity;
        public bool IsGrounded => isGrounded;

        public void Move(Vector2 direction, float deltaTime)
        {
            // Mock implementation - just store the movement data
            LastMoveDirection = direction;
            LastMoveDeltaTime = deltaTime;
            MoveCallCount++;
        }

        public void Jump(float force)
        {
            LastJumpForce = force;
            JumpCallCount++;
            velocity = new Vector2(velocity.x, force);
        }

        public void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
            LastSetVelocity = newVelocity;
            SetVelocityCallCount++;
        }

        // Test utilities for verification
        public Vector2 LastMoveDirection { get; private set; }
        public float LastMoveDeltaTime { get; private set; }
        public float LastJumpForce { get; private set; }
        public Vector2 LastSetVelocity { get; private set; }
        public int MoveCallCount { get; private set; }
        public int JumpCallCount { get; private set; }
        public int SetVelocityCallCount { get; private set; }

        public void SetGrounded(bool grounded) => isGrounded = grounded;
        public void SetVelocityInternal(Vector2 vel) => velocity = vel;
        
        public void ResetCallCounts()
        {
            MoveCallCount = 0;
            JumpCallCount = 0;
            SetVelocityCallCount = 0;
        }
    }
}
