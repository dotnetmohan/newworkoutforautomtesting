# Parallel vs Sequential Test Execution Guide

## Current Setup

Your tests are configured to run:
- **Features in parallel** ✅ (Audit.feature || Products.feature || User.feature)
- **Scenarios sequentially** ✅ (001 → 002 → 003 within each feature)

This is the **recommended approach** for most test suites!

## Execution Modes

### Mode 1: Full Parallel (Fastest - Default)
Features and scenarios run in parallel

```bash
# Run with default settings (parallel)
dotnet test
```

### Mode 2: Features Parallel, Scenarios Sequential (Recommended)
Features run in parallel, but scenarios within each feature run in order

```bash
# Use the test.runsettings file
dotnet test --settings test.runsettings
```

### Mode 3: Fully Sequential (Slowest)
Everything runs one after another

```bash
# Sequential execution - no parallelism
dotnet test -- RunConfiguration.MaxCpuCount=1

# OR using runsettings
dotnet test --settings test.runsettings -- RunConfiguration.MaxCpuCount=1
```

### Mode 4: Limited Parallelism
Control the number of parallel threads

```bash
# Use only 2 threads
dotnet test -- RunConfiguration.MaxCpuCount=2
```

## Visual Representation

```
PARALLEL FEATURES (Default):
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│ Audit.feat  │  │ Product.feat│  │ User.feat   │
│  001 ↓      │  │  001 ↓      │  │  001 ↓      │
│  002 ↓      │  │  002 ↓      │  │             │
│  003 ↓      │  │  003 ↓      │  │             │
│  ...        │  │             │  │             │
└─────────────┘  └─────────────┘  └─────────────┘
    Run simultaneously (parallel features)
    But within each feature: 001 → 002 → 003 (sequential)

SEQUENTIAL (MaxCpuCount=1):
┌─────────────┐
│ Audit.feat  │
│  001 → 002  │
│  → 003 ...  │
└─────────────┘
      ↓
┌─────────────┐
│ Product.feat│
│  001 → 002  │
│  → 003      │
└─────────────┘
      ↓
┌─────────────┐
│ User.feat   │
│  001        │
└─────────────┘
```

## Configuration Options

### Option 1: Command Line
```bash
# Parallel (default)
dotnet test

# Sequential
dotnet test -- RunConfiguration.MaxCpuCount=1

# Limited threads
dotnet test -- RunConfiguration.MaxCpuCount=4
```

### Option 2: Using .runsettings File
```bash
# Use the test.runsettings file
dotnet test --settings test.runsettings

# Override specific settings
dotnet test --settings test.runsettings -- RunConfiguration.MaxCpuCount=1
```

### Option 3: Configure in Visual Studio
1. Test → Configure Run Settings → Select Solution Wide runsettings File
2. Choose `test.runsettings`

### Option 4: Environment Variable
```bash
# PowerShell
$env:VSTEST_HOST_DEBUG=1
dotnet test
```

## For CI/CD Pipelines

### Azure DevOps
```yaml
- task: VSTest@2
  inputs:
    testSelector: 'testAssemblies'
    testAssemblyVer2: |
      **\*Tests.dll
      !**\*TestAdapter.dll
      !**\obj\**
    runSettingsFile: 'AutomationTest.Tests/test.runsettings'
    overrideTestrunParameters: '-MaxCpuCount 4'
```

### GitHub Actions
```yaml
- name: Run tests
  run: dotnet test --settings AutomationTest.Tests/test.runsettings --logger "trx;LogFileName=test-results.trx"
  
- name: Run tests sequentially
  run: dotnet test -- RunConfiguration.MaxCpuCount=1
```

## Test Framework Specific

### xUnit (Most likely what you're using with Reqnroll)
```csharp
// In AssemblyInfo.cs or test class
[assembly: CollectionBehavior(DisableTestParallelization = true)] // Sequential
[assembly: CollectionBehavior(MaxParallelThreads = 4)] // Limited parallel
```

### NUnit
```csharp
[assembly: Parallelizable(ParallelScope.Fixtures)] // Features parallel
[assembly: Parallelizable(ParallelScope.None)] // Sequential
```

### MSTest
```csharp
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]
```

## Best Practices

1. **Use Parallel Features** - Faster execution, better resource utilization
2. **Keep Scenarios Sequential** - Maintains order within each feature (001, 002, 003)
3. **Avoid Shared State** - Each feature should be independent
4. **Use Tags for Smoke Tests** - Run quick tests first
   ```bash
   dotnet test --filter "Category=smoke"
   ```

## Troubleshooting

### If tests fail in parallel but pass sequentially:
- Check for shared resources (database, files, ports)
- Use test isolation techniques
- Add setup/teardown in Hooks.cs

### If tests are too slow:
- Increase parallelism: `dotnet test -- RunConfiguration.MaxCpuCount=8`
- Use faster test data
- Mock external dependencies

## Current Recommendation

Keep the current setup:
```bash
# Default - Features parallel, scenarios sequential
dotnet test --settings test.runsettings
```

This gives you:
- ✅ Fast execution (parallel features)
- ✅ Predictable order (sequential scenarios with 001, 002, 003)
- ✅ Easy debugging
- ✅ Minimal race conditions
