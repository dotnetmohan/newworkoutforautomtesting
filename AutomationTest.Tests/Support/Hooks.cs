using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using AutomationTest.Core.Configuration;
using AutomationTest.Core.Services;
using Reqnroll;

namespace AutomationTest.Tests.Support
{
    [Binding]
    public class Hooks
    {
        private static ExtentReports _extent = null!;
        private static ExtentTest _feature = null!;
        private static ExtentTest _scenario = null!;
        private static LoggerService _logger = null!;
        private static DateTime _scenarioStartTime;
        
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
            _feature = _extent.CreateTest<Feature>(featureContext.FeatureInfo.Title);
            _logger.LogInformation("========== Feature Started: {FeatureName} ==========", featureContext.FeatureInfo.Title);
        }

        [BeforeScenario]
        public static void BeforeScenario(ScenarioContext scenarioContext)
        {
            _scenarioStartTime = DateTime.UtcNow;
            _scenario = _feature.CreateNode<Scenario>(scenarioContext.ScenarioInfo.Title);
            _logger.LogInformation("--- Scenario Started: {ScenarioName} ---", scenarioContext.ScenarioInfo.Title);
        }

        [AfterStep]
        public void AfterStep(ScenarioContext scenarioContext)
        {
            var stepType = scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
            var stepName = scenarioContext.StepContext.StepInfo.Text;

            if (scenarioContext.TestError == null)
            {
                _logger.LogDebug("Step Passed: {StepType} {StepName}", stepType, stepName);
                switch (stepType)
                {
                    case "Given":
                        _scenario.CreateNode<Given>(stepName);
                        break;
                    case "When":
                        _scenario.CreateNode<When>(stepName);
                        break;
                    case "Then":
                        _scenario.CreateNode<Then>(stepName);
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
                        _scenario.CreateNode<Given>(stepName).Fail(error.Message);
                        break;
                    case "When":
                        _scenario.CreateNode<When>(stepName).Fail(error.Message);
                        break;
                    case "Then":
                        _scenario.CreateNode<Then>(stepName).Fail(error.Message);
                        break;
                }
            }
        }

        [AfterScenario]
        public static void AfterScenario(ScenarioContext scenarioContext)
        {
            var duration = DateTime.UtcNow - _scenarioStartTime;
            var passed = scenarioContext.TestError == null;
            var featureName = scenarioContext.ScenarioInfo.Title;
            var scenarioName = scenarioContext.ScenarioInfo.Title;
            
            // Track test execution in Azure Application Insights
            _logger.TrackTestExecution(
                testName: featureName,
                scenario: scenarioName,
                passed: passed,
                duration: duration,
                errorMessage: scenarioContext.TestError?.Message
            );
            
            if (passed)
            {
                _logger.LogInformation("--- Scenario Passed: {ScenarioName} - Duration: {Duration}ms ---", 
                    scenarioName, duration.TotalMilliseconds);
            }
            else
            {
                _logger.LogError("--- Scenario Failed: {ScenarioName} - Duration: {Duration}ms - Error: {ErrorMessage} ---", 
                    scenarioName, duration.TotalMilliseconds, scenarioContext.TestError?.Message ?? "Unknown error");
            }
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
