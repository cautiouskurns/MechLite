using UnityEngine;
using MechSalvager.Movement;

namespace MechLite.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IGroundDetector for testing
    /// </summary>
    public class MockGroundDetector : MonoBehaviour, IGroundDetector
    {
        [SerializeField] private bool isGrounded = true;
        [SerializeField] private float coyoteTimeRemaining = 0f;
        [SerializeField] private bool wasRecentlyGrounded = false;

        public bool IsGrounded => isGrounded;
        public float CoyoteTimeRemaining => coyoteTimeRemaining;
        public bool WasRecentlyGrounded => wasRecentlyGrounded;

        public void CheckGrounded()
        {
            // Mock implementation - ground state controlled manually in tests
        }

        public void UpdateGroundTiming()
        {
            if (coyoteTimeRemaining > 0)
            {
                coyoteTimeRemaining = Mathf.Max(0, coyoteTimeRemaining - Time.deltaTime);
            }
        }

        public bool CanPerformGroundAction()
        {
            return IsGrounded || WasRecentlyGrounded || CoyoteTimeRemaining > 0;
        }

        // Test utilities
        public void SetGrounded(bool grounded)
        {
            isGrounded = grounded;
            if (!grounded)
            {
                wasRecentlyGrounded = true;
                coyoteTimeRemaining = 0.1f; // Default coyote time
            }
        }

        public void SetCoyoteTime(float time) => coyoteTimeRemaining = time;
        public void SetWasRecentlyGrounded(bool recent) => wasRecentlyGrounded = recent;
        public void ResetGroundState() 
        {
            isGrounded = true;
            coyoteTimeRemaining = 0f;
            wasRecentlyGrounded = false;
        }
    }
}