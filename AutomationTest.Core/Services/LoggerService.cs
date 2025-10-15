using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using AutomationTest.Core.Configuration;

namespace AutomationTest.Core.Services
{
    /// <summary>
    /// Centralized logging service using Azure Application Insights and Console logging
    /// </summary>
    public class LoggerService
    {
        private static LoggerService? _instance;
        private static readonly object _lock = new();
        private readonly ILogger _logger;
        private readonly TelemetryClient? _telemetryClient;

        private LoggerService()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                // Add console logging if enabled
                if (LoggingSettings.EnableConsoleLogging)
                {
                    builder.AddConsole();
                }

                // Add Azure Application Insights if enabled
                if (LoggingSettings.EnableAzureMonitor && !string.IsNullOrEmpty(LoggingSettings.InstrumentationKey))
                {
                    builder.AddApplicationInsights(
                        configureTelemetryConfiguration: (config) => 
                            config.ConnectionString = $"InstrumentationKey={LoggingSettings.InstrumentationKey}",
                        configureApplicationInsightsLoggerOptions: (options) => { });
                }

                // Set minimum log level
                builder.SetMinimumLevel(ParseLogLevel(LoggingSettings.MinimumLogLevel));
            });

            _logger = loggerFactory.CreateLogger<LoggerService>();

            // Initialize TelemetryClient for custom events/metrics
            if (LoggingSettings.EnableAzureMonitor && !string.IsNullOrEmpty(LoggingSettings.InstrumentationKey))
            {
                var config = TelemetryConfiguration.CreateDefault();
                config.ConnectionString = $"InstrumentationKey={LoggingSettings.InstrumentationKey}";
                _telemetryClient = new TelemetryClient(config);
            }
        }

        public static LoggerService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LoggerService();
                        }
                    }
                }
                return _instance;
            }
        }

        private LogLevel ParseLogLevel(string level)
        {
            return level.ToLower() switch
            {
                "trace" => LogLevel.Trace,
                "debug" => LogLevel.Debug,
                "information" => LogLevel.Information,
                "warning" => LogLevel.Warning,
                "error" => LogLevel.Error,
                "critical" => LogLevel.Critical,
                _ => LogLevel.Information
            };
        }

        #region Logging Methods

        public void LogTrace(string message, params object[] args)
        {
            _logger.LogTrace(message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _logger.LogError(message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
        }

        public void LogCritical(string message, params object[] args)
        {
            _logger.LogCritical(message, args);
        }

        public void LogCritical(Exception exception, string message, params object[] args)
        {
            _logger.LogCritical(exception, message, args);
        }

        #endregion

        #region Custom Telemetry Methods

        /// <summary>
        /// Track a custom event in Application Insights
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, string>? properties = null)
        {
            if (_telemetryClient != null)
            {
                _telemetryClient.TrackEvent(eventName, properties);
            }
        }

        /// <summary>
        /// Track API request/response
        /// </summary>
        public void TrackApiRequest(string apiName, string method, int statusCode, TimeSpan duration, bool success)
        {
            var properties = new Dictionary<string, string>
            {
                { "API", apiName },
                { "Method", method },
                { "StatusCode", statusCode.ToString() },
                { "Duration", duration.TotalMilliseconds.ToString() },
                { "Success", success.ToString() }
            };

            TrackEvent("API_Request", properties);
            
            LogInformation("API Request: {ApiName} {Method} - Status: {StatusCode} - Duration: {Duration}ms - Success: {Success}",
                apiName, method, statusCode, duration.TotalMilliseconds, success);
        }

        /// <summary>
        /// Track test execution
        /// </summary>
        public void TrackTestExecution(string testName, string scenario, bool passed, TimeSpan duration, string? errorMessage = null)
        {
            var properties = new Dictionary<string, string>
            {
                { "TestName", testName },
                { "Scenario", scenario },
                { "Passed", passed.ToString() },
                { "Duration", duration.TotalMilliseconds.ToString() }
            };

            if (!string.IsNullOrEmpty(errorMessage))
            {
                properties.Add("ErrorMessage", errorMessage);
            }

            TrackEvent("Test_Execution", properties);
            
            if (passed)
            {
                LogInformation("Test Passed: {TestName} - {Scenario} - Duration: {Duration}ms", 
                    testName, scenario, duration.TotalMilliseconds);
            }
            else
            {
                LogError("Test Failed: {TestName} - {Scenario} - Duration: {Duration}ms - Error: {ErrorMessage}", 
                    testName, scenario, duration.TotalMilliseconds, errorMessage);
            }
        }

        /// <summary>
        /// Flush all pending telemetry (call before application exits)
        /// </summary>
        public void Flush()
        {
            _telemetryClient?.Flush();
            // Allow time for flushing
            Task.Delay(1000).Wait();
        }

        #endregion
    }
}
