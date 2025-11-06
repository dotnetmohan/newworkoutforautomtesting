# Dependency Injection (DI) Implementation Plan

## 📋 Current State Analysis

### Current Issues & Tight Coupling

#### 1. **API Clients** - Hard Dependencies
```csharp
// Current implementation in AuditApiClient.cs
public AuditApiClient()
{
    _client = new RestClient();                    // ❌ Tightly coupled
    _authClient = new AuthenticationClient();      // ❌ Tightly coupled
    _logger = LoggerService.Instance;              // ❌ Singleton pattern
}
```

**Problems:**
- ❌ Cannot mock dependencies for unit testing
- ❌ Hard to test in isolation
- ❌ Violates Dependency Inversion Principle
- ❌ Difficult to swap implementations

#### 2. **Step Definitions** - Direct Instantiation
```csharp
// Current implementation in AuditStepDefinitions.cs
public AuditStepDefinitions()
{
    _apiClient = new AuditApiClient();            // ❌ Tightly coupled
    _logger = LoggerService.Instance;             // ❌ Singleton pattern
}
```

**Problems:**
- ❌ Cannot inject mock API clients for testing
- ❌ Step definitions are hard to unit test
- ❌ No flexibility to change implementations

#### 3. **LoggerService** - Singleton Pattern
```csharp
// Current implementation
private static LoggerService? _instance;
public static LoggerService Instance
{
    get
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                _instance ??= new LoggerService();
            }
        }
        return _instance;
    }
}
```

**Problems:**
- ❌ Global state makes testing difficult
- ❌ Cannot easily swap logger implementations
- ❌ Hard to control lifecycle

---

## 🎯 Proposed Dependency Injection Architecture

### Benefits of Implementing DI

✅ **Better Testability** - Easy to mock dependencies
✅ **Loose Coupling** - Components depend on abstractions, not concrete types
✅ **SOLID Principles** - Especially Dependency Inversion
✅ **Flexibility** - Easy to swap implementations
✅ **Maintainability** - Cleaner, more readable code
✅ **Lifecycle Management** - Container manages object lifetimes

---

## 📐 Recommended Implementation Plan

### Phase 1: Create Interfaces (Abstractions)

#### 1.1 IAuthenticationClient Interface
```csharp
namespace AutomationTest.Core.Api
{
    public interface IAuthenticationClient
    {
        Task<string> GetAccessTokenAsync();
    }
}
```

#### 1.2 IAuditApiClient Interface
```csharp
namespace AutomationTest.Core.Api
{
    public interface IAuditApiClient
    {
        RestResponse GetLastResponse();
        Task<string> GetAuditDataAsync(AuditRequest auditRequest);
        Task<RestResponse> GetAuditHistoryListAsync(
            string? tableDescription = null,
            string? createdBy = null,
            string? createdAtFrom = null,
            string? createdAtTo = null,
            int? page = null,
            int? size = null,
            string? sortBy = null,
            string? sortOrder = null,
            bool useAuthentication = true,
            string? acceptHeader = "application/json");
        Task<RestResponse> GetAuditHistoryByIdAsync(string auditHistoryId, bool useAuthentication = true);
        Task<RestResponse> GetAuditHistoryWithInsufficientPermissionsAsync();
    }
}
```

#### 1.3 ILoggerService Interface
```csharp
namespace AutomationTest.Core.Services
{
    public interface ILoggerService
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception? exception, string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void TrackApiRequest(string apiName, string method, int statusCode, TimeSpan duration, bool success);
        void TrackTestExecution(string testName, string scenario, bool passed, TimeSpan duration, string? errorMessage = null);
        void Flush();
    }
}
```

#### 1.4 IProductsApiClient Interface
```csharp
namespace AutomationTest.Core.Api
{
    public interface IProductsApiClient
    {
        RestResponse GetLastResponse();
        Task<RestResponse> GetProductsAsync(string endpoint);
    }
}
```

#### 1.5 IUserApiClient Interface
```csharp
namespace AutomationTest.Core.Api
{
    public interface IUserApiClient
    {
        RestResponse GetLastResponse();
        Task<string> GetUserDataAsync();
    }
}
```

---

### Phase 2: Refactor Classes to Use Constructor Injection

#### 2.1 Update AuditApiClient
```csharp
public class AuditApiClient : IAuditApiClient
{
    private readonly RestClient _client;
    private readonly IAuthenticationClient _authClient;
    private readonly ILoggerService _logger;
    private RestResponse? _lastResponse;
    private string? _lastAccessToken;

    // ✅ Constructor Injection
    public AuditApiClient(
        IAuthenticationClient authClient,
        ILoggerService logger,
        RestClient? restClient = null)
    {
        _authClient = authClient ?? throw new ArgumentNullException(nameof(authClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = restClient ?? new RestClient();
        _logger.LogInformation("AuditApiClient initialized");
    }
    
    // ... rest of the implementation
}
```

#### 2.2 Update AuthenticationClient
```csharp
public class AuthenticationClient : IAuthenticationClient
{
    private readonly RestClient _client;
    private readonly ILoggerService _logger;
    private string? _cachedToken;
    private DateTime _tokenExpiry;

    // ✅ Constructor Injection
    public AuthenticationClient(
        ILoggerService logger,
        RestClient? restClient = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = restClient ?? new RestClient(ApiSettings.TokenUrl);
        _logger.LogInformation("AuthenticationClient initialized");
    }
    
    // ... rest of the implementation
}
```

#### 2.3 Update LoggerService (Remove Singleton)
```csharp
public class LoggerService : ILoggerService
{
    private readonly ILogger _logger;
    private readonly TelemetryClient? _telemetryClient;

    // ✅ Constructor Injection (no more singleton)
    public LoggerService(ILogger<LoggerService> logger, TelemetryClient? telemetryClient = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryClient = telemetryClient;
    }
    
    // ... rest of the implementation
}
```

#### 2.4 Update Step Definitions
```csharp
[Binding]
public class AuditStepDefinitions
{
    private readonly IAuditApiClient _apiClient;
    private readonly ILoggerService _logger;
    private AuditRequest _auditRequest = new();
    private RestResponse _lastResponse = null!;
    // ... other fields

    // ✅ Constructor Injection via Reqnroll
    public AuditStepDefinitions(
        IAuditApiClient apiClient,
        ILoggerService logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // ... step implementations
}
```

---

### Phase 3: Setup Dependency Injection Container

#### 3.1 Install Required NuGet Packages
```xml
<!-- Add to AutomationTest.Tests.csproj -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="Reqnroll.Microsoft.Extensions.DependencyInjection" Version="1.0.0" />
```

#### 3.2 Create DI Configuration Class
```csharp
using Microsoft.Extensions.DependencyInjection;
using AutomationTest.Core.Api;
using AutomationTest.Core.Services;
using Reqnroll.Microsoft.Extensions.DependencyInjection;
using RestSharp;

namespace AutomationTest.Tests.Support
{
    public static class DependencyInjectionConfig
    {
        [ScenarioDependencies]
        public static IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();

            // Register Logger
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                // Add other logging providers as needed
            });

            // Register LoggerService
            services.AddSingleton<ILoggerService, LoggerService>();

            // Register RestClient (can be singleton)
            services.AddSingleton<RestClient>();

            // Register API Clients (scoped per scenario)
            services.AddScoped<IAuthenticationClient, AuthenticationClient>();
            services.AddScoped<IAuditApiClient, AuditApiClient>();
            services.AddScoped<IProductsApiClient, ProductsApiClient>();
            services.AddScoped<IUserApiClient, UserApiClient>();

            return services;
        }
    }
}
```

---

## 📊 Implementation Comparison

### Before DI (Current)
```csharp
// Tightly coupled
public class AuditStepDefinitions
{
    public AuditStepDefinitions()
    {
        _apiClient = new AuditApiClient();  // Hard dependency
        _logger = LoggerService.Instance;   // Singleton
    }
}

// Hard to test
[Test]
public void TestAuditStep()
{
    var steps = new AuditStepDefinitions();
    // Cannot mock _apiClient or _logger
}
```

### After DI (Proposed)
```csharp
// Loosely coupled
public class AuditStepDefinitions
{
    public AuditStepDefinitions(
        IAuditApiClient apiClient,
        ILoggerService logger)
    {
        _apiClient = apiClient;  // Injected dependency
        _logger = logger;        // Injected dependency
    }
}

// Easy to test
[Test]
public void TestAuditStep()
{
    var mockApiClient = new Mock<IAuditApiClient>();
    var mockLogger = new Mock<ILoggerService>();
    var steps = new AuditStepDefinitions(mockApiClient.Object, mockLogger.Object);
    // Can fully control behavior
}
```

---

## 🎯 Recommended Changes by File

### Core Library Changes

| File | Changes Required | Complexity |
|------|------------------|------------|
| `IAuthenticationClient.cs` | ✅ Create new interface | Low |
| `IAuditApiClient.cs` | ✅ Create new interface | Low |
| `IProductsApiClient.cs` | ✅ Create new interface | Low |
| `IUserApiClient.cs` | ✅ Create new interface | Low |
| `ILoggerService.cs` | ✅ Create new interface | Low |
| `AuthenticationClient.cs` | 🔄 Add constructor injection | Medium |
| `AuditApiClient.cs` | 🔄 Add constructor injection | Medium |
| `ProductsApiClient.cs` | 🔄 Add constructor injection | Medium |
| `UserApiClient.cs` | 🔄 Add constructor injection | Medium |
| `LoggerService.cs` | 🔄 Remove singleton, add DI | Medium |

### Test Project Changes

| File | Changes Required | Complexity |
|------|------------------|------------|
| `DependencyInjectionConfig.cs` | ✅ Create new file | Medium |
| `AuditStepDefinitions.cs` | 🔄 Update constructor | Low |
| `ProductsStepDefinitions.cs` | 🔄 Update constructor | Low |
| `UserStepDefinitions.cs` | 🔄 Update constructor | Low |
| `AutomationTest.Tests.csproj` | 📦 Add NuGet packages | Low |

---

## 💡 Additional Benefits

### 1. Environment-Specific Configuration
```csharp
// Easy to configure different environments
services.AddScoped<IAuditApiClient>(sp =>
{
    var env = Environment.GetEnvironmentVariable("TEST_ENV");
    return env == "MOCK" 
        ? new MockAuditApiClient() 
        : new AuditApiClient(sp.GetRequiredService<IAuthenticationClient>(), 
                            sp.GetRequiredService<ILoggerService>());
});
```

### 2. Decorator Pattern
```csharp
// Add caching layer without changing existing code
services.Decorate<IAuditApiClient, CachedAuditApiClient>();
```

### 3. Interceptors/Middleware
```csharp
// Add logging, retry logic, etc.
services.AddScoped<IAuditApiClient>(sp =>
{
    var client = new AuditApiClient(...);
    return new RetryDecorator(client, maxRetries: 3);
});
```

---

## 🚀 Implementation Steps

### Step 1: Create Interfaces
1. Create `Interfaces` folder in `AutomationTest.Core`
2. Add all interface files
3. Commit changes

### Step 2: Update Core Classes
1. Update `AuthenticationClient` to implement interface and use DI
2. Update `AuditApiClient` to implement interface and use DI
3. Update `ProductsApiClient` to implement interface and use DI
4. Update `UserApiClient` to implement interface and use DI
5. Update `LoggerService` to implement interface and use DI
6. Run tests to ensure no breaking changes
7. Commit changes

### Step 3: Setup DI Container
1. Add NuGet packages to test project
2. Create `DependencyInjectionConfig.cs`
3. Update step definitions to use DI
4. Run tests to verify everything works
5. Commit changes

### Step 4: Cleanup
1. Remove any unused code
2. Update documentation
3. Final testing
4. Commit changes

---

## ⚠️ Considerations & Risks

### Potential Issues
1. **Learning Curve**: Team needs to understand DI concepts
2. **Initial Complexity**: More setup code required
3. **Breaking Changes**: Existing tests might need updates
4. **Reqnroll Integration**: Need to ensure proper DI integration

### Mitigation Strategies
1. ✅ Implement gradually (phase by phase)
2. ✅ Keep backward compatibility during transition
3. ✅ Comprehensive testing after each phase
4. ✅ Team training on DI patterns
5. ✅ Good documentation

---

## 📈 Success Metrics

After DI implementation, you should see:
- ✅ 100% testable components (with mocks)
- ✅ Reduced coupling between classes
- ✅ Easier to add new features
- ✅ Better code organization
- ✅ Simplified unit testing

---

## 🔍 My Recommendation

### Priority: **HIGH** ⭐⭐⭐

**Why Implement DI?**
1. Your framework is growing - DI will make it more maintainable
2. Better testability for unit tests (not just integration tests)
3. Follows industry best practices
4. Makes the framework more professional and enterprise-ready
5. Easier onboarding for new team members

### Recommended Approach
**Start with Phase 1 & 2** (Interfaces + Core refactoring)
- Lower risk
- Immediate benefits
- Can be done incrementally
- Doesn't require Reqnroll DI plugin initially

**Then move to Phase 3** (Full DI Container)
- After team is comfortable with interfaces
- When ready to fully commit to DI

---

## ❓ Questions for You

Before we proceed with implementation, please confirm:

1. **Are you comfortable with the DI pattern?** 
   - Would you like me to provide training material first?

2. **Do you want to implement all at once or incrementally?**
   - Option A: Full implementation (2-3 days work)
   - Option B: Incremental (1 week, lower risk)

3. **Do you want to keep backward compatibility?**
   - Keep old constructors temporarily with `[Obsolete]` attribute?

4. **Testing strategy?**
   - Create unit tests alongside DI implementation?

5. **Which API client should we start with?**
   - Recommendation: Start with `AuditApiClient` (most complex)

---

## 📝 Next Steps

**If you approve, I will:**
1. Create all interface files
2. Refactor one API client as a proof of concept
3. Show you the results
4. Get your feedback
5. Proceed with remaining changes

**Please let me know:**
- ✅ Do you want to proceed?
- 🎯 Which approach (full vs incremental)?
- 💬 Any concerns or questions?
