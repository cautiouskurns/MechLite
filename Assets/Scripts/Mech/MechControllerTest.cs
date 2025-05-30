using UnityEngine;
using MechLite.Mech;

public class MechControllerTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunTestSequence());
        }
    }
    
    private System.Collections.IEnumerator RunTestSequence()
    {
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log("=== MECH CONTROLLER TEST STARTING ===");
        
        var mechController = FindAnyObjectByType<MechController>();
        
        if (mechController == null)
        {
            Debug.LogError("TEST FAILED: No MechController found!");
            yield break;
        }
        
        Debug.Log($"Found MechController on: {mechController.gameObject.name}");
        Debug.Log($"Is Initialized: {mechController.IsInitialized}");
        Debug.Log($"Position: {mechController.Position}");
        Debug.Log($"Rigidbody2D: {(mechController.GetRigidbody2D() != null ? "Found" : "Missing")}");
        Debug.Log($"BoxCollider2D: {(mechController.GetCollider2D() != null ? "Found" : "Missing")}");
        Debug.Log($"SpriteRenderer: {(mechController.GetSpriteRenderer() != null ? "Found" : "Missing")}");
        
        Debug.Log("=== MECH CONTROLLER TEST COMPLETE ===");
    }
}