using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using Reqnroll;
using System.Collections.Concurrent;

namespace AutomationTest.Tests.Support
{
    [Binding]
    public class Hooks
    {
        private static ExtentReports _extent = null!;
        
        // Use thread-safe collections for parallel execution
        private static readonly ConcurrentDictionary<string, ExtentTest> _features = new();
        private static readonly ConcurrentDictionary<string, ExtentTest> _scenarios = new();
        private static readonly ConcurrentDictionary<string, DateTime> _scenarioStartTimes = new();
        
        private static string ReportPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResults", "ExtentReport.html");

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            var reportDir = Path.GetDirectoryName(ReportPath);
            if (!string.IsNullOrEmpty(reportDir) && !Directory.Exists(reportDir))
                Directory.CreateDirectory(reportDir);

            var reporter = new ExtentHtmlReporter(ReportPath);
            _extent = new ExtentReports();
            _extent.AttachReporter(reporter);
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            var featureTitle = featureContext.FeatureInfo.Title;
            var feature = _extent.CreateTest<Feature>(featureTitle);
            _features.TryAdd(featureTitle, feature);
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
            
            // Clean up scenario from dictionary
            _scenarios.TryRemove(scenarioKey, out _);
            _scenarioStartTimes.TryRemove(scenarioKey, out _);
        }

        [AfterFeature]
        public static void AfterFeature(FeatureContext featureContext)
        {
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            _extent.Flush();
        }
    }
}
