# Test Execution Order Guide

## How to Order Scenario Execution

### Method 1: Using @order Tags (Recommended - Already Implemented)

Add `@order:N` tags to your scenarios in the feature file:

```gherkin
@positive @auditHistory @order:1
Scenario: First test to run
    When I do something
    Then it works

@positive @auditHistory @order:2
Scenario: Second test to run
    When I do something else
    Then it also works
```

**The custom ScenarioOrderer is already configured in your project!**

### Method 2: Alphabetical Ordering (Simple)

Rename scenarios to start with numbers:

```gherkin
Scenario: 001 - First test
Scenario: 002 - Second test
Scenario: 010 - Tenth test
```

### Method 3: Separate Feature Files

Create separate feature files for different test groups:
- `01_PositiveTests.feature`
- `02_NegativeTests.feature`
- `03_ValidationTests.feature`

### Method 4: Run Specific Tests with Tags

Use test runner filters to execute in order:

```bash
# PowerShell
dotnet test --filter "Category=order:1"
dotnet test --filter "Category=order:2"

# Or run by test type in order
dotnet test --filter "Category=positive"
dotnet test --filter "Category=negative"
```

### Method 5: Configure Test Runner Settings

In your `.runsettings` file, you can configure parallel execution:

```xml
<RunSettings>
  <RunConfiguration>
    <MaxCpuCount>1</MaxCpuCount>  <!-- Sequential execution -->
  </RunConfiguration>
</RunSettings>
```

## Best Practices

1. **Use @order tags** for scenarios that must run in a specific sequence
2. **Keep tests independent** - each scenario should work on its own
3. **Use @order only when necessary** - for setup/teardown or data dependency scenarios
4. **Group related tests** - use tags like @smoke, @regression, @positive, @negative

## Example Usage in Your Project

```gherkin
@positive @auditHistory @order:1
Scenario: Setup - Create test data
    Given I create test audit records
    
@positive @auditHistory @order:2
Scenario: Retrieve the created data
    When I request audit history data
    Then I should see the test records

@positive @auditHistory @order:999
Scenario: Cleanup - Remove test data
    When I delete the test audit records
```

## Running Tests in Order

```bash
# Run all tests (will execute in order based on @order tags)
dotnet test

# Run only ordered tests
dotnet test --filter "Category=order"

# Run specific order
dotnet test --filter "FullyQualifiedName~order:1"
```

## Notes

- Tests without @order tags will run after all ordered tests
- Tests with the same order number will run alphabetically by scenario name
- The ScenarioOrderer is configured at assembly level in AssemblyInfo.cs
