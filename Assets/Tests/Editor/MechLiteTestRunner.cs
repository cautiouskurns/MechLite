using UnityEditor;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;

namespace MechLite.Tests
{
    /// <summary>
    /// Custom test runner for the MechLite Player Movement System test suite
    /// </summary>
    public class MechLiteTestRunner
    {
        [MenuItem("MechLite/Run All Tests")]
        public static void RunAllTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var filter = new Filter()
            {
                testMode = TestMode.PlayMode | TestMode.EditMode,
                testNames = new string[] { },
                groupNames = new string[] { },
                categoryNames = new string[] { },
                assemblyNames = new string[] { "MechLite.Tests", "MechLite.EditModeTests" }
            };
            
            testRunnerApi.Execute(new ExecutionSettings(filter));
        }
        
        [MenuItem("MechLite/Run Unit Tests Only")]
        public static void RunUnitTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var filter = new Filter()
            {
                testMode = TestMode.PlayMode,
                testNames = new string[] { },
                groupNames = new string[] { "MechLite.Tests.Systems" },
                categoryNames = new string[] { },
                assemblyNames = new string[] { "MechLite.Tests" }
            };
            
            testRunnerApi.Execute(new ExecutionSettings(filter));
        }
        
        [MenuItem("MechLite/Run Integration Tests Only")]
        public static void RunIntegrationTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var filter = new Filter()
            {
                testMode = TestMode.PlayMode,
                testNames = new string[] { },
                groupNames = new string[] { "MechLite.Tests.Integration" },
                categoryNames = new string[] { },
                assemblyNames = new string[] { "MechLite.Tests" }
            };
            
            testRunnerApi.Execute(new ExecutionSettings(filter));
        }
        
        [MenuItem("MechLite/Run Configuration Tests Only")]
        public static void RunConfigurationTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var filter = new Filter()
            {
                testMode = TestMode.EditMode,
                testNames = new string[] { },
                groupNames = new string[] { "MechLite.Tests.Editor" },
                categoryNames = new string[] { },
                assemblyNames = new string[] { "MechLite.EditModeTests" }
            };
            
            testRunnerApi.Execute(new ExecutionSettings(filter));
        }
    }
}
