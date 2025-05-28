using UnityEngine;

namespace MechLite.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IDashable for testing
    /// </summary>
    public class MockDashSystem : MonoBehaviour, IDashable
    {
        [SerializeField] private bool canDash = true;
        [SerializeField] private float dashCooldownRemaining = 0f;

        public bool CanDash => canDash;
        public float DashCooldownRemaining => dashCooldownRemaining;

        public bool Dash(Vector2 direction)
        {
            if (!CanDash) return false;

            LastDashDirection = direction;
            DashCallCount++;
            canDash = false;
            dashCooldownRemaining = 1f; // Default cooldown
            return true;
        }

        public void UpdateDashCooldown()
        {
            if (dashCooldownRemaining > 0)
            {
                dashCooldownRemaining = Mathf.Max(0, dashCooldownRemaining - Time.deltaTime);
                if (dashCooldownRemaining <= 0)
                {
                    canDash = true;
                }
            }
        }

        // Test utilities
        public Vector2 LastDashDirection { get; private set; }
        public int DashCallCount { get; private set; }

        public void SetCanDash(bool canDashValue) => canDash = canDashValue;
        public void SetCooldownRemaining(float cooldown) => dashCooldownRemaining = cooldown;
        public void ResetDashState()
        {
            canDash = true;
            dashCooldownRemaining = 0f;
            DashCallCount = 0;
        }
    }
}
