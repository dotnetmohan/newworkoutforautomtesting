using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Reqnroll;
using System.Collections.Concurrent;

namespace AutomationTest.Tests.Support
{
    [Binding]
    public class Hooks
    {
        private static ExtentReports? _extent;
        private static ExtentSparkReporter? _sparkReporter;
        
        // Thread-safe dictionaries for parallel execution
        private static readonly ConcurrentDictionary<string, ExtentTest> _features = new();
        private static readonly ConcurrentDictionary<string, ExtentTest> _scenarios = new();
        private static readonly ConcurrentDictionary<string, DateTime> _scenarioStartTimes = new();

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Create output directory if it doesn't exist
            var reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResults");
            Directory.CreateDirectory(reportPath);

            var htmlReportPath = Path.Combine(reportPath, "ExtentReport.html");

            // Initialize ExtentSparkReporter
            _sparkReporter = new ExtentSparkReporter(htmlReportPath);

            // Configure Spark Reporter
            var config = _sparkReporter.Config;
            config.DocumentTitle = "Automation Test Report";
            config.ReportName = "API Test Execution Results";
            config.Encoding = "UTF-8";
            
            // Timeline settings
            config.TimelineEnabled = true;
            
            // Initialize ExtentReports
            _extent = new ExtentReports();
            _extent.AttachReporter(_sparkReporter);

            // Add system/environment information
            _extent.AddSystemInfo("Application", "Audit API Automation");
            _extent.AddSystemInfo("Environment", "Test");
            _extent.AddSystemInfo("User", Environment.UserName);
            _extent.AddSystemInfo("Machine", Environment.MachineName);
            _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
            _extent.AddSystemInfo(".NET Version", Environment.Version.ToString());
            _extent.AddSystemInfo("Test Framework", "Reqnroll + RestSharp");
            _extent.AddSystemInfo("Execution Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            Console.WriteLine($"ExtentSparkReporter initialized. Report will be saved to: {htmlReportPath}");
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            if (_extent == null) return;

            var featureTitle = featureContext.FeatureInfo.Title;
            var featureDescription = featureContext.FeatureInfo.Description;

            var feature = _extent.CreateTest(featureTitle, featureDescription);
            
            // Add feature tags
            foreach (var tag in featureContext.FeatureInfo.Tags)
            {
                feature.AssignCategory(tag);
            }

            _features.TryAdd(featureTitle, feature);
            Console.WriteLine($"Feature started: {featureTitle}");
        }

        [BeforeScenario]
        public static void BeforeScenario(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var featureTitle = featureContext.FeatureInfo.Title;
            var scenarioTitle = scenarioContext.ScenarioInfo.Title;
            var scenarioKey = $"{featureTitle}::{scenarioTitle}";

            if (_features.TryGetValue(featureTitle, out var feature))
            {
                var scenario = feature.CreateNode(scenarioTitle);
                
                // Add scenario tags
                foreach (var tag in scenarioContext.ScenarioInfo.Tags)
                {
                    scenario.AssignCategory(tag);
                }

                // Add scenario description if available
                if (!string.IsNullOrEmpty(scenarioContext.ScenarioInfo.Description))
                {
                    scenario.Info(scenarioContext.ScenarioInfo.Description);
                }

                _scenarios.TryAdd(scenarioKey, scenario);
                _scenarioStartTimes.TryAdd(scenarioKey, DateTime.UtcNow);
                
                Console.WriteLine($"Scenario started: {scenarioTitle}");
            }
        }

        [AfterStep]
        public static void AfterStep(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var featureTitle = featureContext.FeatureInfo.Title;
            var scenarioTitle = scenarioContext.ScenarioInfo.Title;
            var scenarioKey = $"{featureTitle}::{scenarioTitle}";

            if (_scenarios.TryGetValue(scenarioKey, out var scenario))
            {
                var stepType = scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
                var stepText = scenarioContext.StepContext.StepInfo.Text;

                if (scenarioContext.TestError == null)
                {
                    scenario.CreateNode($"{stepType} {stepText}").Pass("Step passed");
                }
                else
                {
                    var error = scenarioContext.TestError;
                    scenario.CreateNode($"{stepType} {stepText}")
                        .Fail($"<pre>{error.Message}</pre>")
                        .Fail($"<pre>{error.StackTrace}</pre>");
                }
            }
        }

        [AfterScenario]
        public static void AfterScenario(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var featureTitle = featureContext.FeatureInfo.Title;
            var scenarioTitle = scenarioContext.ScenarioInfo.Title;
            var scenarioKey = $"{featureTitle}::{scenarioTitle}";

            if (_scenarios.TryGetValue(scenarioKey, out var scenario))
            {
                // Calculate duration
                if (_scenarioStartTimes.TryGetValue(scenarioKey, out var startTime))
                {
                    var duration = DateTime.UtcNow - startTime;
                    scenario.Info($"Duration: {duration.TotalSeconds:F2} seconds");
                }

                // Set final scenario status
                if (scenarioContext.TestError != null)
                {
                    scenario.Fail(scenarioContext.TestError.Message);
                    Console.WriteLine($"Scenario failed: {scenarioTitle}");
                }
                else
                {
                    scenario.Pass("Scenario passed");
                    Console.WriteLine($"Scenario passed: {scenarioTitle}");
                }

                // Clean up
                _scenarioStartTimes.TryRemove(scenarioKey, out _);
            }
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            // Flush the report
            _extent?.Flush();
            
            Console.WriteLine("ExtentSparkReporter: Report generated successfully");
            
            // Clear dictionaries
            _features.Clear();
            _scenarios.Clear();
            _scenarioStartTimes.Clear();
        }
    }
}
