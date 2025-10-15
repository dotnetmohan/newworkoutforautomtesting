namespace AutomationTest.Core.Configuration
{
    public static class LoggingSettings
    {
        /// <summary>
        /// Azure Application Insights Instrumentation Key
        /// </summary>
        public static string InstrumentationKey { get; set; } = "YOUR_INSTRUMENTATION_KEY_HERE";
        
        /// <summary>
        /// Enable/Disable Azure Application Insights logging
        /// </summary>
        public static bool EnableAzureMonitor { get; set; } = true;
        
        /// <summary>
        /// Enable/Disable Console logging
        /// </summary>
        public static bool EnableConsoleLogging { get; set; } = true;
        
        /// <summary>
        /// Minimum log level (Trace, Debug, Information, Warning, Error, Critical)
        /// </summary>
        public static string MinimumLogLevel { get; set; } = "Information";
    }
}
