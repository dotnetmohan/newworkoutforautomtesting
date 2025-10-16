# Quick Reference: Test Execution Commands

## Run Tests in Different Modes

### Parallel Execution (Default - Fastest)
```powershell
# Features run in parallel, scenarios sequential
dotnet test
```

### Sequential Execution (Slowest - For Debugging)
```powershell
# Everything runs one after another
dotnet test -- RunConfiguration.MaxCpuCount=1
```

### With Settings File
```powershell
# Use test.runsettings configuration
dotnet test --settings test.runsettings
```

### Limited Parallelism
```powershell
# Use 4 threads
dotnet test -- RunConfiguration.MaxCpuCount=4
```

## Filter Tests

### By Tag
```powershell
# Run only positive tests
dotnet test --filter "Category=positive"

# Run only negative tests
dotnet test --filter "Category=negative"

# Run only audit history tests
dotnet test --filter "Category=auditHistory"
```

### By Feature
```powershell
# Run only Audit feature tests
dotnet test --filter "FullyQualifiedName~Audit"

# Run only Products feature tests
dotnet test --filter "FullyQualifiedName~Products"

# Run only User feature tests
dotnet test --filter "FullyQualifiedName~User"
```

### By Scenario Number
```powershell
# Run only scenario 001
dotnet test --filter "FullyQualifiedName~001"
```

## Generate Reports

### With Logging
```powershell
# Generate TRX report
dotnet test --logger "trx;LogFileName=test-results.trx"

# Generate HTML report
dotnet test --logger "html;LogFileName=test-results.html"

# Multiple loggers
dotnet test --logger "trx" --logger "console;verbosity=detailed"
```

### Code Coverage
```powershell
# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Current Setup Status

✅ **Features run in parallel** (Audit || Products || User)
✅ **Scenarios run sequentially** (001 → 002 → 003)
✅ **Sequential numbering** applied to all features
✅ **Custom orderer** configured (supports @order:N tags)

## Recommended Commands

```powershell
# Development - Quick feedback
dotnet test --filter "Category=smoke"

# Full test run - Parallel (fastest)
dotnet test --settings test.runsettings

# Debugging - Sequential
dotnet test -- RunConfiguration.MaxCpuCount=1

# CI/CD - With reports
dotnet test --settings test.runsettings --logger "trx;LogFileName=test-results.trx"
```
