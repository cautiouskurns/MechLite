# MechLite Player Movement System - Test Suite

This comprehensive unit test suite validates the refactored M1.1 Player Movement System's modular architecture, interface implementations, configuration-driven behavior, and event communication systems.

## Test Structure

### Directory Organization
```
Assets/Tests/
├── Runtime/                    # PlayMode tests
│   ├── Systems/               # Individual system unit tests
│   ├── Integration/           # System integration tests
│   ├── Mocks/                # Mock implementations for isolated testing
│   ├── EventCapture.cs        # Event system test utilities
│   ├── TestConfigurationFactory.cs  # Test configuration creation
│   └── Tests.asmdef          # Runtime test assembly definition
├── Editor/                    # EditMode tests
│   ├── ConfigurationValidationTests.cs  # ScriptableObject validation
│   ├── MechLiteTestRunner.cs  # Custom test runner
│   └── EditModeTests.asmdef   # Editor test assembly definition
└── TestConfigurations/        # Test ScriptableObject assets
```

## Test Categories

### 1. Individual System Tests

#### EnergySystemTests.cs
- **Interface Contract Tests**: Validates IEnergyUser implementation
- **Energy Consumption Tests**: Verifies energy usage mechanics
- **Energy Regeneration Tests**: Tests automatic energy recovery
- **Event System Tests**: Validates EnergyChangedEvent publishing
- **Configuration Tests**: Ensures proper config integration
- **Edge Cases**: Handles extreme values and error conditions

#### MovementControllerTests.cs
- **Interface Contract Tests**: Validates IMovable implementation
- **Movement Tests**: Ground vs air movement behavior
- **Jump Tests**: Jump force application and event publishing
- **Velocity Control Tests**: Direct velocity manipulation
- **Physics Integration Tests**: Configuration-driven behavior
- **Edge Cases**: Extreme delta time and input values

#### GroundDetectorTests.cs
- **Interface Contract Tests**: Validates IGroundDetector implementation
- **Ground Detection Tests**: Raycast/spherecast validation
- **Coyote Time Tests**: Grace period mechanics
- **Ground Action Tests**: CanPerformGroundAction logic
- **Layer Mask Tests**: Ground layer filtering
- **Edge Cases**: Missing colliders and timing edge cases

#### DashSystemTests.cs
- **Interface Contract Tests**: Validates IDashable implementation
- **Dash Execution Tests**: Force application and energy consumption
- **Cooldown Tests**: Dash timing and availability
- **Direction Tests**: Input normalization and directional accuracy
- **Integration Tests**: Energy system coordination
- **Edge Cases**: Extreme directions and zero inputs

#### JumpSystemTests.cs
- **Jump Input Tests**: Basic jump execution
- **Jump Buffering Tests**: Input buffering mechanics
- **Coyote Time Integration**: Ground detector coordination
- **Event Integration Tests**: Event system coordination
- **Configuration Integration**: Config value usage
- **Edge Cases**: Zero forces and multiple jump prevention

### 2. Integration Tests

#### PlayerControllerIntegrationTests.cs
- **Initialization Integration**: All system startup coordination
- **Movement Integration**: System interaction during movement
- **Jump Integration**: Multi-system jump execution
- **Dash Integration**: Energy, cooldown, and physics coordination
- **Energy System Integration**: Cross-system energy management
- **Event System Integration**: Event publishing validation
- **Configuration Integration**: Config-driven behavior changes
- **Performance and Stability**: Stress testing and edge cases
- **Legacy API Compatibility**: Public interface validation

### 3. Event System Tests

#### PlayerEventBusTests.cs
- **Singleton Tests**: Singleton pattern validation
- **Event Type Tests**: Individual event type handling
- **Event Isolation Tests**: Type-specific event delivery
- **Performance Tests**: High-volume event handling
- **Edge Cases**: Null data and extreme values

### 4. Configuration Tests

#### ConfigurationValidationTests.cs
- **ScriptableObject Creation**: Instance creation validation
- **Default Value Validation**: Sensible default values
- **Range Validation**: Value constraint enforcement
- **Cross-Configuration Validation**: Inter-config consistency
- **Runtime Modification**: Dynamic value changes
- **Error Handling**: Extreme and invalid values

## Mock Systems

### Mock Implementations
- **MockEnergySystem**: IEnergyUser mock with controllable state
- **MockGroundDetector**: IGroundDetector mock with manual state control
- **MockMovable**: IMovable mock with call tracking
- **MockDashSystem**: IDashable mock with state control

### Test Utilities
- **EventCapture**: Captures and verifies published events
- **TestConfigurationFactory**: Creates test configs with known values

## Running Tests

### Unity Test Runner (Recommended)
1. Open **Window → General → Test Runner**
2. Switch between **PlayMode** and **EditMode** tabs
3. Click **Run All** or select specific test classes

### Custom Test Runner
Use the custom menu items for organized test execution:
- **MechLite → Run All Tests**: Executes complete test suite
- **MechLite → Run Unit Tests Only**: Individual system tests
- **MechLite → Run Integration Tests Only**: System interaction tests
- **MechLite → Run Configuration Tests Only**: ScriptableObject validation

### Command Line Execution
```bash
# Run all tests
Unity.exe -runTests -batchmode -projectPath "path/to/project" -testResults results.xml

# Run specific assembly
Unity.exe -runTests -batchmode -projectPath "path/to/project" -testPlatform playmode -assemblyNames "MechLite.Tests"
```

## Test Coverage

### Interface Coverage
- ✅ IMovable contract compliance
- ✅ IEnergyUser contract compliance  
- ✅ IDashable contract compliance
- ✅ IGroundDetector contract compliance

### System Coverage
- ✅ MovementController (100% public API)
- ✅ EnergySystem (100% public API)
- ✅ GroundDetector (100% public API)
- ✅ DashSystem (100% public API)
- ✅ JumpSystem (100% public API)
- ✅ PlayerController (100% public API)

### Event Coverage
- ✅ PlayerMovedEvent
- ✅ PlayerJumpedEvent
- ✅ PlayerDashedEvent
- ✅ EnergyChangedEvent
- ✅ GroundStateChangedEvent
- ✅ Event bus singleton behavior

### Configuration Coverage
- ✅ MovementConfigSO validation
- ✅ EnergyConfigSO validation
- ✅ DashConfigSO validation
- ✅ PhysicsConfigSO validation
- ✅ Cross-configuration consistency
- ✅ Runtime modification support

## Best Practices Demonstrated

### Test Design
- **Arrange-Act-Assert** pattern throughout
- **Isolated testing** with mock systems
- **Integration testing** for system interactions
- **Edge case coverage** for robustness
- **Performance validation** for stability

### Unity-Specific Testing
- **Coroutine testing** for time-based behavior
- **Physics integration** with FixedUpdate timing
- **GameObject lifecycle** management
- **ScriptableObject** validation
- **Event system** testing

### Maintainability
- **Clear test organization** by feature area
- **Descriptive test names** indicating purpose
- **Comprehensive setup/teardown** for clean states
- **Reusable utilities** for common operations
- **Documentation** for test understanding

## Troubleshooting

### Common Issues

#### Tests Fail Due to Missing References
Ensure all assembly references are correctly configured in `.asmdef` files.

#### Physics-Based Tests Are Inconsistent
Use `WaitForFixedUpdate()` for physics-dependent tests and ensure consistent test environments.

#### Event Tests Fail
Verify `PlayerEventBus` singleton is properly reset between tests using `ClearAllSubscriptions()`.

#### Mock Systems Don't Behave as Expected
Check that mock systems are properly initialized and their test utility methods are called correctly.

### Performance Considerations
- Tests run in both **PlayMode** and **EditMode**
- **Integration tests** may take longer due to full system setup
- **Performance tests** validate system stability under load
- **Event tests** verify high-volume event handling

## Contributing

When adding new tests:
1. Follow existing **naming conventions**
2. Include **comprehensive documentation**
3. Add both **positive and negative** test cases
4. Test **edge cases and error conditions**
5. Ensure **proper cleanup** in teardown methods
6. Update this **README** with new test categories

## Future Enhancements

### Planned Additions
- **Performance benchmarking** tests
- **Memory allocation** validation
- **Multiplayer synchronization** tests (if applicable)
- **Platform-specific** behavior validation
- **Automated regression** testing

### Test Data
- **Parameterized tests** for configuration variations
- **Golden file testing** for expected outputs
- **Fuzz testing** for input validation
- **Load testing** for performance bounds
