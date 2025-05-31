using UnityEngine;

namespace MechLite.Mech
{
    /// <summary>
    /// Central controller for mech functionality in Mech Salvager
    /// Serves as the foundational hub coordinating all mech-related systems
    /// Implements IMechControllable interface for extensibility and testing
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class MechController : MonoBehaviour, IMechControllable
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("Base stat configuration for this mech")]
        private MechConfigSO mechConfig;
        
        [Header("Debug")]
        [SerializeField, Tooltip("Enable detailed debug logging for development")]
        private bool enableDebugLogs = false;
        
        // Component references (auto-discovered)
        private Rigidbody2D rb2d;
        private BoxCollider2D boxCollider;
        private SpriteRenderer spriteRenderer;
        
        // Stats system
        private MechStats stats;
        
        // Core state
        private bool isInitialized = false;
        
        // Public API
        public MechStats Stats => stats;
        
        #region IMechControllable Implementation
        
        public bool IsInitialized => isInitialized;
        public Vector3 Position => transform.position;
        
        public void Initialize()
        {
            if (enableDebugLogs)
                Debug.Log("MechController: Starting initialization...");
            
            if (!ValidateComponents())
            {
                Debug.LogError("MechController: Initialization failed - missing required components!");
                return;
            }
            
            // Initialize stats system
            InitializeStats();
            
            isInitialized = true;
            
            if (enableDebugLogs)
            {
                Debug.Log($"MechController: Successfully initialized on '{gameObject.name}'");
                LogComponentReferences();
                LogStatsConfiguration();
            }
        }
        
        public void Shutdown()
        {
            if (enableDebugLogs)
                Debug.Log("MechController: Shutting down...");
            
            // Unsubscribe from stats events
            if (stats != null)
                stats.OnStatChanged -= OnStatChanged;
            
            isInitialized = false;
            rb2d = null;
            boxCollider = null;
            spriteRenderer = null;
            stats = null;
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            DiscoverComponents();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void OnDestroy()
        {
            Shutdown();
        }
        
        #endregion
        
        #region Component Management
        
        private void DiscoverComponents()
        {
            rb2d = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (enableDebugLogs)
                Debug.Log("MechController: Component discovery completed");
        }
        
        private bool ValidateComponents()
        {
            bool allComponentsValid = true;
            
            if (rb2d == null)
            {
                Debug.LogError("MechController: Missing Rigidbody2D component! Required for physics-based movement.");
                allComponentsValid = false;
            }
            
            if (boxCollider == null)
            {
                Debug.LogError("MechController: Missing BoxCollider2D component! Required for collision detection.");
                allComponentsValid = false;
            }
            
            if (spriteRenderer == null)
            {
                Debug.LogError("MechController: Missing SpriteRenderer component! Required for visual representation.");
                allComponentsValid = false;
            }
            
            return allComponentsValid;
        }
        
        private void LogComponentReferences()
        {
            Debug.Log($"MechController Component References:");
            Debug.Log($"  - Rigidbody2D: {(rb2d != null ? "✓ Found" : "✗ Missing")}");
            Debug.Log($"  - BoxCollider2D: {(boxCollider != null ? "✓ Found" : "✗ Missing")}");
            Debug.Log($"  - SpriteRenderer: {(spriteRenderer != null ? "✓ Found" : "✗ Missing")}");
        }
        
        #endregion
        
        #region Stats System
        
        private void InitializeStats()
        {
            stats = new MechStats();
            stats.Initialize(mechConfig);
            
            // Subscribe to stat change events
            stats.OnStatChanged += OnStatChanged;
            
            if (enableDebugLogs)
                Debug.Log("MechController: Stats system initialized");
        }
        
        private void OnStatChanged(StatType statType, float oldValue, float newValue)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"MechController: Stat changed - {statType}: {oldValue:F2} → {newValue:F2}");
            }
            
            // Future: Trigger any stat-dependent updates here
            // e.g., health bar updates, movement speed changes, etc.
        }
        
        private void LogStatsConfiguration()
        {
            if (stats == null) return;
            
            Debug.Log("MechController Stats Configuration:");
            Debug.Log($"  - Health: {stats.GetStat(StatType.Health):F1}/{stats.GetStat(StatType.MaxHealth):F1}");
            Debug.Log($"  - Energy: {stats.GetStat(StatType.Energy):F1}/{stats.GetStat(StatType.MaxEnergy):F1}");
            Debug.Log($"  - MoveSpeed: {stats.GetStat(StatType.MoveSpeed):F1}");
            Debug.Log($"  - Damage: {stats.GetStat(StatType.Damage):F1}");
            Debug.Log($"  - Armor: {stats.GetStat(StatType.Armor):F1}");
        }
        
        #endregion
        
        #region Public API
        
        public Rigidbody2D GetRigidbody2D() => rb2d;
        public BoxCollider2D GetCollider2D() => boxCollider;
        public SpriteRenderer GetSpriteRenderer() => spriteRenderer;
        
        public void SetDebugLogging(bool enabled)
        {
            enableDebugLogs = enabled;
            if (enableDebugLogs)
                Debug.Log("MechController: Debug logging enabled");
        }
        
        /// <summary>
        /// Get current value of a specific stat
        /// </summary>
        public float GetStat(StatType statType)
        {
            return stats?.GetStat(statType) ?? 0f;
        }
        
        /// <summary>
        /// Add a stat modifier to the mech
        /// </summary>
        public void AddStatModifier(StatModifier modifier)
        {
            stats?.AddModifier(modifier);
        }
        
        /// <summary>
        /// Remove a stat modifier from the mech
        /// </summary>
        public bool RemoveStatModifier(StatModifier modifier)
        {
            return stats?.RemoveModifier(modifier) ?? false;
        }
        
        #endregion
        
        #region Debug Visualization
        
        private void OnDrawGizmos()
        {
            if (!isInitialized) return;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            
            #if UNITY_EDITOR
            string status = isInitialized ? "INITIALIZED" : "NOT INITIALIZED";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"MechController\n{status}");
            #endif
        }
        
        #endregion
    }
}