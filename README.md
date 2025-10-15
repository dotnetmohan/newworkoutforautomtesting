# API Test Automation Framework

This project is a test automation framework for testing REST APIs, built to test multiple APIs including DummyJSON Products API and Audit API.

## Technologies Used

- .NET 8.0
- xUnit - Testing framework
- Reqnroll - BDD framework (fork of SpecFlow)
- RestSharp - REST API client
- ExtentReports - Test reporting
- System.Text.Json - JSON serialization/deserialization
- Azure Application Insights - Centralized logging and monitoring
- Microsoft.Extensions.Logging - Structured logging framework

## Supported APIs

### Products API
- Get all products
- Get product by ID
- Search products

### Audit API
- Authentication with token generation
- Retrieve audit data
- Retrieve audit history list with filters
- Retrieve audit history by ID
- Pagination support
- Sorting and filtering capabilities
- Headers management
- Error handling and validation
- Comprehensive positive and negative test scenarios

## Project Structure

```
AutomationTest/
├── AutomationTest.Core/              # Core functionality
│   ├── Api/                          # API clients
│   │   ├── ProductsApiClient.cs      # Products API implementation
│   │   ├── AuditApiClient.cs         # Audit API implementation
│   │   ├── UserApiClient.cs          # User API implementation
│   │   └── AuthenticationClient.cs   # Token generation client
│   ├── Configuration/                # Configuration settings
│   │   ├── ApiSettings.cs            # API endpoints and keys
│   │   └── LoggingSettings.cs        # Logging configuration
│   ├── Models/                       # Data models
│   │   ├── Product.cs                # Products API models
│   │   ├── AuditModels.cs            # Audit API models
│   │   └── UserModels.cs             # User API models
│   └── Services/                     # Business services
│       └── LoggerService.cs          # Centralized logging service
└── AutomationTest.Tests/             # Test project
    ├── Constants/                    # Test constants
    │   └── AssertionMessages.cs      # Centralized assertion messages
    ├── Features/                     # BDD feature files
    │   ├── Products.reqnroll.feature # Products API scenarios
    │   ├── Audit.reqnroll.feature    # Audit API scenarios
    │   └── User.reqnroll.feature     # User API scenarios
    ├── Steps/                        # Step definitions
    │   ├── ProductsStepDefinitions.cs # Products API steps
    │   ├── AuditStepDefinitions.cs   # Audit API steps
    │   └── UserStepDefinitions.cs    # User API steps
    ├── Helpers/                      # Test helpers
    │   └── TestDataReader.cs         # JSON test data reader
    └── Support/
        └── Hooks.cs                  # Test hooks and reporting
```

## Framework Features

### Architecture
- Modular and maintainable test structure
- Clear separation of concerns
- Reusable components
- Easy to extend for new APIs
- Configuration-driven approach

### Testing Capabilities
- BDD-style scenarios using Reqnroll
- JSON-based test data management
- External test data configuration
- Comprehensive test reporting with ExtentReports
- Centralized logging with Azure Application Insights
- Strong typing for API requests/responses
- Centralized assertion messages
- Automated test execution with xUnit
- Error handling and structured logging
- Performance tracking and monitoring

### API Testing Features
- Automatic token generation and management
- Header management for API requests
- Error handling and validation
- Response status code verification
- Response content validation
- Support for multiple API endpoints

### Best Practices
- Clean code principles
- SOLID principles
- Dependency injection
- Proper exception handling
- Centralized configuration
- Organized test structure

## Centralized Logging

The framework uses Azure Application Insights for centralized logging and monitoring.

### Features
- **Real-time Logging**: Console output during test execution
- **Azure Monitor Integration**: All logs sent to Azure Application Insights
- **Performance Tracking**: API response times and test durations
- **Error Tracking**: Automatic exception logging with full context
- **Custom Telemetry**: Track API requests and test executions
- **Queryable Data**: Use Kusto Query Language for insights

### Configuration

Set your Azure Application Insights instrumentation key in `LoggingSettings.cs`:

```csharp
LoggingSettings.InstrumentationKey = "YOUR_INSTRUMENTATION_KEY_HERE";
LoggingSettings.EnableAzureMonitor = true;
LoggingSettings.EnableConsoleLogging = true;
```

### What Gets Logged

- Test lifecycle (run, feature, scenario, step)
- API requests with timing and status codes
- Errors and exceptions with full stack traces
- Custom events and metrics
- Test execution results

### Viewing Logs

**Console**: Real-time output during test runs

**Azure Portal**: Navigate to Application Insights → Logs
```kusto
// View all API requests
customEvents
| where name == "API_Request"
| where timestamp > ago(1h)

// View failed tests
customEvents
| where name == "Test_Execution"
| where customDimensions.Passed == "False"
```

📖 **Full Documentation**: See [LOGGING.md](LOGGING.md) for complete guide

## Test Reporting

### ExtentReports

The framework generates HTML test reports using ExtentReports.
- Test scenario execution status
- Step-by-step execution details
- Execution time and timestamps
- Error details (if any)
- API request/response information

## Test Scenarios

### Products API Scenarios
- Getting all products
- Getting a single product by ID
- Searching products by keyword
- Validating response status codes
- Validating response content

