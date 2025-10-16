using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using AutomationTest.Core.Configuration;
using AutomationTest.Core.Services;
using Reqnroll;
using System.Collections.Concurrent;

namespace AutomationTest.Tests.Support
{
    [Binding]
    public class Hooks
    {
        private static ExtentReports _extent = null!;
        private static LoggerService _logger = null!;
        
        // Use thread-safe collections for parallel execution
        private static readonly ConcurrentDictionary<string, ExtentTest> _features = new();
        private static readonly ConcurrentDictionary<string, ExtentTest> _scenarios = new();
        private static readonly ConcurrentDictionary<string, DateTime> _scenarioStartTimes = new();
        
        private static string ReportPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResults", "ExtentReport.html");

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Initialize centralized logging
            _logger = LoggerService.Instance;
            _logger.LogInformation("========== Test Run Started ==========");
            _logger.LogInformation("Instrumentation Key Configured: {Configured}", !string.IsNullOrEmpty(LoggingSettings.InstrumentationKey));
            _logger.LogInformation("Azure Monitor Enabled: {Enabled}", LoggingSettings.EnableAzureMonitor);
            
            var reportDir = Path.GetDirectoryName(ReportPath);
            if (!string.IsNullOrEmpty(reportDir) && !Directory.Exists(reportDir))
                Directory.CreateDirectory(reportDir);

            var reporter = new ExtentHtmlReporter(ReportPath);
            _extent = new ExtentReports();
            _extent.AttachReporter(reporter);
            
            _logger.LogInformation("Extent Report initialized at: {ReportPath}", ReportPath);
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            var featureTitle = featureContext.FeatureInfo.Title;
            var feature = _extent.CreateTest<Feature>(featureTitle);
            _features.TryAdd(featureTitle, feature);
            _logger.LogInformation("========== Feature Started: {FeatureName} ==========", featureTitle);
        }

        [BeforeScenario]
        public static void BeforeScenario(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            var featureTitle = featureContext.FeatureInfo.Title;
            var scenarioTitle = scenarioContext.ScenarioInfo.Title;
            var scenarioKey = $"{featureTitle}::{scenarioTitle}";
            
            _scenarioStartTimes.TryAdd(scenarioKey, DateTime.UtcNow);
            
            if (_features.TryGetValue(featureTitle, out var feature))
            {
                var scenario = feature.CreateNode<Scenario>(scenarioTitle);
                _scenarios.TryAdd(scenarioKey, scenario);
            }
            
            _logger.LogInformation("--- Scenario Started: {ScenarioName} ---", scenarioTitle);
        }

        [AfterStep]
        public void AfterStep(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            var featureTitle = featureContext.FeatureInfo.Title;
            var scenarioTitle = scenarioContext.ScenarioInfo.Title;
            var scenarioKey = $"{featureTitle}::{scenarioTitle}";
            var stepType = scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
            var stepName = scenarioContext.StepContext.StepInfo.Text;

            if (!_scenarios.TryGetValue(scenarioKey, out var scenario))
                return;

            if (scenarioContext.TestError == null)
            {
                _logger.LogDebug("Step Passed: {StepType} {StepName}", stepType, stepName);
                switch (stepType)
                {
                    case "Given":
                        scenario.CreateNode<Given>(stepName);
                        break;
                    case "When":
                        scenario.CreateNode<When>(stepName);
                        break;
                    case "Then":
                        scenario.CreateNode<Then>(stepName);
                        break;
                }
            }
            else
            {
                var error = scenarioContext.TestError;
                _logger.LogError(error, "Step Failed: {StepType} {StepName} - Error: {ErrorMessage}", 
                    stepType, stepName, error.Message);
                    
                switch (stepType)
                {
                    case "Given":
                        scenario.CreateNode<Given>(stepName).Fail(error.Message);
                        break;
                    case "When":
                        scenario.CreateNode<When>(stepName).Fail(error.Message);
                        break;
                    case "Then":
                        scenario.CreateNode<Then>(stepName).Fail(error.Message);
                        break;
                }
            }
        }

        [AfterScenario]
        public static void AfterScenario(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            var featureTitle = featureContext.FeatureInfo.Title;
            var scenarioTitle = scenarioContext.ScenarioInfo.Title;
            var scenarioKey = $"{featureTitle}::{scenarioTitle}";
            
            var duration = TimeSpan.Zero;
            if (_scenarioStartTimes.TryGetValue(scenarioKey, out var startTime))
            {
                duration = DateTime.UtcNow - startTime;
            }
            
            var passed = scenarioContext.TestError == null;
            
            // Track test execution in Azure Application Insights
            _logger.TrackTestExecution(
                testName: featureTitle,
                scenario: scenarioTitle,
                passed: passed,
                duration: duration,
                errorMessage: scenarioContext.TestError?.Message
            );
            
            if (passed)
            {
                _logger.LogInformation("--- Scenario Passed: {ScenarioName} - Duration: {Duration}ms ---", 
                    scenarioTitle, duration.TotalMilliseconds);
            }
            else
            {
                _logger.LogError("--- Scenario Failed: {ScenarioName} - Duration: {Duration}ms - Error: {ErrorMessage} ---", 
                    scenarioTitle, duration.TotalMilliseconds, scenarioContext.TestError?.Message ?? "Unknown error");
            }
            
            // Clean up scenario from dictionary
            _scenarios.TryRemove(scenarioKey, out _);
            _scenarioStartTimes.TryRemove(scenarioKey, out _);
        }

        [AfterFeature]
        public static void AfterFeature(FeatureContext featureContext)
        {
            _logger.LogInformation("========== Feature Completed: {FeatureName} ==========", featureContext.FeatureInfo.Title);
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            _extent.Flush();
            _logger.LogInformation("========== Test Run Completed ==========");
            _logger.LogInformation("Extent Report generated at: {ReportPath}", ReportPath);
            
            // Flush all telemetry to Azure Application Insights
            _logger.Flush();
        }
    }
}
