# ExtentReport Feature Grouping Fix

## Problem
When running tests in parallel, all scenarios were appearing under a single feature in the ExtentReport instead of being grouped under their respective features (Audit, Products, User).

## Root Cause
The original code used static variables (`_feature`, `_scenario`, `_scenarioStartTime`) that were shared across all parallel test threads. When multiple features ran simultaneously, they would overwrite each other's context, causing scenarios to be misplaced in the report.

## Solution
Updated `Hooks.cs` to use **thread-safe concurrent dictionaries** to manage features and scenarios:

### Changes Made:

1. **Replaced static variables** with thread-safe collections:
   ```csharp
   // OLD (not thread-safe)
   private static ExtentTest _feature;
   private static ExtentTest _scenario;
   private static DateTime _scenarioStartTime;
   
   // NEW (thread-safe)
   private static readonly ConcurrentDictionary<string, ExtentTest> _features = new();
   private static readonly ConcurrentDictionary<string, ExtentTest> _scenarios = new();
   private static readonly ConcurrentDictionary<string, DateTime> _scenarioStartTimes = new();
   ```

2. **Updated BeforeFeature** to store features by title:
   ```csharp
   var featureTitle = featureContext.FeatureInfo.Title;
   var feature = _extent.CreateTest<Feature>(featureTitle);
   _features.TryAdd(featureTitle, feature);
   ```

3. **Updated BeforeScenario** to link scenarios to correct feature:
   ```csharp
   var scenarioKey = $"{featureTitle}::{scenarioTitle}";
   if (_features.TryGetValue(featureTitle, out var feature))
   {
       var scenario = feature.CreateNode<Scenario>(scenarioTitle);
       _scenarios.TryAdd(scenarioKey, scenario);
   }
   ```

4. **Updated AfterStep** to find correct scenario:
   ```csharp
   var scenarioKey = $"{featureTitle}::{scenarioTitle}";
   if (_scenarios.TryGetValue(scenarioKey, out var scenario))
   {
       // Add step to correct scenario
   }
   ```

5. **Updated AfterScenario** to clean up resources:
   ```csharp
   _scenarios.TryRemove(scenarioKey, out _);
   _scenarioStartTimes.TryRemove(scenarioKey, out _);
   ```

## Result
Now when tests run in parallel, the ExtentReport correctly shows:

```
📊 ExtentReport.html
├── 📁 Audit API
│   ├── ✅ 001 - Retrieve audit history list successfully
│   ├── ✅ 002 - Retrieve audit history list when there are no records
│   └── ✅ 003 - Retrieve audit history by id successfully
│
├── 📁 Products API Testing
│   ├── ✅ 001 - Get all products
│   ├── ✅ 002 - Get a specific product
│   └── ✅ 003 - Search for products
│
└── 📁 User API
    └── ✅ 001 - Retrieve user data
```

## Benefits
- ✅ Scenarios grouped under correct features
- ✅ Thread-safe for parallel execution
- ✅ No race conditions
- ✅ Better organized reports
- ✅ Easier to read and analyze results

## Testing
Run your tests to verify:

```powershell
# Run all tests in parallel
dotnet test

# Check the report
start .\AutomationTest.Tests\bin\Debug\net9.0\TestResults\ExtentReport.html
```

You should now see scenarios properly grouped under their respective features!
