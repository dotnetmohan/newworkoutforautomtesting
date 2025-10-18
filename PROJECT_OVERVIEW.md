# Automation Testing Framework - Project Overview

## 📋 Table of Contents
1. [Project Introduction](#project-introduction)
2. [Architecture Overview](#architecture-overview)
3. [Project Structure](#project-structure)
4. [Core Components](#core-components)
5. [API Clients](#api-clients)
6. [Test Framework Features](#test-framework-features)
7. [Reporting & Logging](#reporting--logging)
8. [Test Execution](#test-execution)
9. [Configuration Management](#configuration-management)
10. [Best Practices](#best-practices)
11. [Getting Started](#getting-started)

---

## 🎯 Project Introduction

### What is This Project?
A comprehensive **API Automation Testing Framework** built with:
- **.NET 9.0**
- **Reqnroll (SpecFlow successor)** - BDD framework
- **RestSharp** - API client library
- **ExtentReports** - HTML reporting
- **xUnit** - Test runner

### Key Objectives
- ✅ Automated API testing for Audit, Products, and User services
- ✅ BDD-style test scenarios using Gherkin syntax
- ✅ Centralized configuration and reusable components
- ✅ Comprehensive reporting and logging
- ✅ Support for parallel and sequential test execution

---

## 🏗️ Architecture Overview

### High-Level Architecture
```
┌─────────────────────────────────────────────────┐
│         Automation Test Solution                │
├─────────────────────────────────────────────────┤
│                                                 │
│  ┌──────────────────┐  ┌──────────────────┐   │
│  │ Core Library     │  │ Tests Project     │   │
│  ├──────────────────┤  ├──────────────────┤   │
│  │ • API Clients    │  │ • Features (.bdd) │   │
│  │ • Models         │  │ • Step Definitions│   │
│  │ • Configuration  │  │ • Hooks           │   │
│  │ • Services       │  │ • Test Data       │   │
│  └──────────────────┘  └──────────────────┘   │
│           │                      │              │
│           └──────────┬───────────┘              │
│                      ▼                           │
│            ┌──────────────────┐                 │
│            │  Target APIs     │                 │
│            ├──────────────────┤                 │
│            │ • Audit API      │                 │
│            │ • Products API   │                 │
│            │ • User API       │                 │
│            └──────────────────┘                 │
└─────────────────────────────────────────────────┘
```

### Technology Stack
| Component | Technology |
|-----------|-----------|
| **Framework** | .NET 9.0 |
| **BDD Tool** | Reqnroll |
| **API Client** | RestSharp |
| **Test Runner** | xUnit |
| **Reporting** | ExtentReports |
| **Language** | C# |

---

## 📁 Project Structure

### Solution Organization
```
AutomationTest.sln
│
├── AutomationTest.Core/              # Core Library
│   ├── Api/                          # API Client Classes
│   │   ├── AuditApiClient.cs
│   │   ├── AuthenticationClient.cs
│   │   ├── ProductsApiClient.cs
│   │   └── UserApiClient.cs
│   │
│   ├── Configuration/                # Configuration Classes
│   │   ├── ApiSettings.cs
│   │   └── LoggingSettings.cs
│   │
│   ├── Models/                       # Data Models
│   │   ├── AuditModels.cs
│   │   ├── Product.cs
│   │   └── UserModels.cs
│   │
│   └── Services/                     # Utility Services
│       └── LoggerService.cs
│
└── AutomationTest.Tests/             # Test Project
    ├── Features/                     # Gherkin Feature Files
    │   ├── Audit.reqnroll.feature
    │   ├── Products.reqnroll.feature
    │   └── User.reqnroll.feature
    │
    ├── Steps/                        # Step Definitions
    │   ├── AuditStepDefinitions.cs
    │   ├── ProductsStepDefinitions.cs
    │   └── UserStepDefinitions.cs
    │
    ├── Support/                      # Test Infrastructure
    │   ├── Hooks.cs
    │   └── ScenarioOrderer.cs
    │
    ├── Helpers/                      # Helper Classes
    │   └── TestDataReader.cs
    │
    ├── TestData/                     # Test Data Files
    │   └── AuditTestData.json
    │
    └── Constants/                    # Constants
        └── AssertionMessages.cs
```

---

## 🔧 Core Components

### 1. API Clients

#### Authentication Client
- **Purpose**: Handles OAuth token generation
- **Features**:
  - Retrieves access tokens from authentication endpoint
  - Caches tokens for reuse
  - Supports token refresh

#### Audit API Client
- **Purpose**: Interacts with Audit API
- **Endpoints**:
  - `POST /audit-history` - Create audit records
  - `GET /audit-history` - List audit history
  - `GET /audit-history/{id}` - Get by ID
- **Features**:
  - Centralized header management
  - Parameterized URL support
  - Authentication integration
  - Error handling

#### Products API Client
- **Purpose**: Manages product-related API calls
- **Endpoints**:
  - `GET /products` - List all products
  - `GET /products/{id}` - Get specific product
  - `GET /products/search` - Search products

#### User API Client
- **Purpose**: Handles user data operations
- **Endpoints**:
  - `GET /users` - Retrieve user data
  - User validation and verification

---

## 🎯 Test Framework Features

### 1. BDD with Reqnroll

#### Feature Files (Gherkin Syntax)
```gherkin
Feature: Audit API
    As a system user
    I want to retrieve audit data
    So that I can track system activities

Scenario: 001 - Retrieve audit history list successfully
    When I request audit history data
    Then the response status code should be 200
    And the response body should be a JSON array
```

#### Benefits
- ✅ Human-readable test specifications
- ✅ Collaboration between technical and non-technical team members
- ✅ Living documentation
- ✅ Reusable step definitions

### 2. Test Organization

#### Sequential Numbering
- All scenarios numbered (001, 002, 003...)
- Ensures predictable execution order
- Easy to track and reference

#### Tag-based Categorization
- `@positive` - Happy path scenarios
- `@negative` - Error handling scenarios
- `@validation` - Data validation tests
- `@filter` - Filtering functionality
- `@pagination` - Pagination tests
- `@sorting` - Sorting tests

### 3. Test Data Management

#### JSON-based Test Data
```json
{
  "validRequest": {
    "BillingId": "12345",
    "UserId": "user@example.com"
  }
}
```

#### Benefits
- ✅ Centralized test data
- ✅ Easy to maintain
- ✅ Supports multiple test scenarios
- ✅ Environment-specific configurations

---

## 📊 Reporting & Logging

### ExtentReports

#### Features
- 📈 **Visual HTML Reports**
  - Feature-wise test grouping
  - Scenario execution details
  - Step-by-step results
  - Pass/Fail statistics

#### Report Structure
```
📊 ExtentReport.html
├── 📁 Audit API
│   ├── ✅ 001 - Retrieve audit history list successfully
│   ├── ✅ 002 - Retrieve audit history list when there are no records
│   └── ❌ 003 - Retrieve audit history by id successfully
│
├── 📁 Products API Testing
│   ├── ✅ 001 - Get all products
│   └── ✅ 002 - Get a specific product
│
└── 📁 User API
    └── ✅ 001 - Retrieve user data
```

#### Key Advantages
- ✅ **Thread-safe** for parallel execution
- ✅ Scenarios grouped by feature
- ✅ Real-time test progress
- ✅ Detailed error messages
- ✅ Screenshots support (if needed)

### Logging Service (Optional)

#### Features
- Console logging
- File logging
- Azure Application Insights integration
- Structured logging with context

---

## ⚙️ Test Execution

### Execution Modes

#### 1. Parallel Execution (Default - Fastest)
```powershell
dotnet test
```
- Features run in parallel
- Scenarios within features run sequentially
- Optimal for CI/CD pipelines

#### 2. Sequential Execution (For Debugging)
```powershell
dotnet test -- RunConfiguration.MaxCpuCount=1
```
- Everything runs one after another
- Easier to debug
- No race conditions

#### 3. Custom Parallel Threads
```powershell
dotnet test -- RunConfiguration.MaxCpuCount=4
```
- Control number of parallel threads
- Balance between speed and resource usage

### Filtering Tests

#### By Category/Tag
```powershell
# Run only positive tests
dotnet test --filter "Category=positive"

# Run only audit tests
dotnet test --filter "Category=auditHistory"

# Run only negative tests
dotnet test --filter "Category=negative"
```

#### By Feature
```powershell
# Run only Audit feature
dotnet test --filter "FullyQualifiedName~Audit"

# Run only Products feature
dotnet test --filter "FullyQualifiedName~Products"
```

#### By Scenario Number
```powershell
# Run only scenario 001
dotnet test --filter "FullyQualifiedName~001"
```

### Test Execution Order

#### Custom ScenarioOrderer
- Uses `@order:N` tags for custom ordering
- Falls back to alphabetical/numerical ordering
- Supports setup/teardown scenarios

#### Example
```gherkin
@order:1
Scenario: Setup test data

@order:10
Scenario: Run main tests

@order:999
Scenario: Cleanup test data
```

---

## 🔐 Configuration Management

### API Settings

#### Centralized URL Management
```csharp
public static class ApiSettings
{
    public const string TokenUrl = "https://MyTest.com/gettoken";
    public const string AuditApiUrl = "https://MyTest.com/getAuditdata";
    public const string AuditHistoryApiUrl = "https://MyTest.com/getAuditdata/audit-history";
    public const string AuditHistoryIdApiUrl = "https://MyTest.com/getAuditdata({AuditHistoryId})";
    public const string SubscriptionKey = "testkey...";
}
```

#### Benefits
- ✅ Single source of truth
- ✅ Easy environment switching
- ✅ Parameterized URLs
- ✅ No hardcoded values in tests

### Request Header Management

#### Centralized Header Configuration
- Common headers applied automatically
- Authentication tokens managed centrally
- Content-Type based on HTTP method
- Custom headers when needed

---

## 🎓 Best Practices

### 1. Code Organization
✅ Separation of concerns (Core vs Tests)
✅ Reusable API clients
✅ Centralized configuration
✅ Helper classes for common operations

### 2. Test Design
✅ Independent test scenarios
✅ Descriptive scenario names
✅ Positive and negative test coverage
✅ Data-driven testing where applicable

### 3. Maintainability
✅ DRY (Don't Repeat Yourself) principle
✅ Meaningful variable names
✅ Comprehensive comments
✅ Version control best practices

### 4. Performance
✅ Parallel execution support
✅ Thread-safe implementations
✅ Efficient resource usage
✅ Proper cleanup after tests

### 5. Reporting
✅ Clear test results
✅ Detailed error messages
✅ Feature-wise grouping
✅ Historical test data tracking

---

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Git

### Setup Steps

#### 1. Clone Repository
```powershell
git clone https://github.com/dotnetmohan/newworkoutforautomtesting.git
cd newworkoutforautomtesting
```

#### 2. Restore Dependencies
```powershell
dotnet restore
```

#### 3. Build Solution
```powershell
dotnet build
```

#### 4. Run Tests
```powershell
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Generate report
dotnet test --logger "trx;LogFileName=test-results.trx"
```

#### 5. View Reports
```powershell
# ExtentReport location
start .\AutomationTest.Tests\bin\Debug\net9.0\TestResults\ExtentReport.html
```

### Quick Commands Reference

| Task | Command |
|------|---------|
| Run all tests | `dotnet test` |
| Run specific feature | `dotnet test --filter "FullyQualifiedName~Audit"` |
| Run positive tests only | `dotnet test --filter "Category=positive"` |
| Run sequential | `dotnet test -- RunConfiguration.MaxCpuCount=1` |
| Generate TRX report | `dotnet test --logger "trx"` |
| Build solution | `dotnet build` |
| Clean build | `dotnet clean` |

---

## 📈 Test Coverage

### Current Test Scenarios

#### Audit API - 24 Scenarios
- **Positive Tests**: 11 scenarios
  - Basic retrieval
  - Filtering (tableDescription, createdBy, date range)
  - Pagination
  - Sorting
  - Validation tests

- **Negative Tests**: 11 scenarios
  - Non-existent ID
  - Invalid formats
  - Authentication failures
  - Authorization issues
  - Invalid parameters

- **Legacy Tests**: 2 scenarios

#### Products API - 3 Scenarios
- Get all products
- Get specific product
- Search products

#### User API - 1 Scenario
- Retrieve user data

**Total**: 28 Test Scenarios

---

## 🔄 CI/CD Integration

### Azure DevOps Pipeline Example
```yaml
- task: VSTest@2
  inputs:
    testSelector: 'testAssemblies'
    testAssemblyVer2: '**\*Tests.dll'
    runSettingsFile: 'AutomationTest.Tests/test.runsettings'
```

### GitHub Actions Example
```yaml
- name: Run tests
  run: dotnet test --logger "trx;LogFileName=test-results.trx"
```

---

## 🎯 Key Achievements

### 1. Centralized API Management
- ✅ Reusable API clients
- ✅ Common header handling
- ✅ Parameterized URLs
- ✅ Authentication abstraction

### 2. Enhanced Reporting
- ✅ Thread-safe ExtentReports
- ✅ Feature-wise grouping
- ✅ Detailed step tracking
- ✅ Visual HTML reports

### 3. Test Execution Control
- ✅ Sequential scenario numbering
- ✅ Custom execution order support
- ✅ Parallel and sequential modes
- ✅ Tag-based filtering

### 4. Code Quality
- ✅ Clean architecture
- ✅ Separation of concerns
- ✅ DRY principle
- ✅ Comprehensive documentation

---

## 📚 Documentation Files

| File | Description |
|------|-------------|
| `README.md` | Project readme |
| `TEST_EXECUTION_ORDER.md` | Test ordering guide |
| `PARALLEL_EXECUTION_GUIDE.md` | Parallel execution details |
| `TEST_COMMANDS.md` | Quick command reference |
| `EXTENT_REPORT_FIX.md` | ExtentReport fix documentation |
| `PROJECT_OVERVIEW.md` | This comprehensive overview |

---

## 🔮 Future Enhancements

### Potential Improvements
- 🔄 Add API mocking for offline testing
- 📊 Integrate with test management tools (TestRail, Zephyr)
- 🔐 Enhanced security testing scenarios
- 📈 Performance testing integration
- 🌐 Multi-environment support (Dev, QA, Prod)
- 📧 Email notifications for test results
- 🐳 Docker containerization
- ☁️ Cloud-based test execution

---

## 👥 Team Collaboration

### How to Contribute
1. Create feature branch
2. Write tests following BDD approach
3. Ensure all tests pass
4. Submit pull request
5. Code review
6. Merge to main

### Code Standards
- Follow C# coding conventions
- Use meaningful names
- Add XML comments
- Write self-documenting code
- Include unit tests for new utilities

---

## 📞 Support & Contact

### Resources
- **Repository**: https://github.com/dotnetmohan/newworkoutforautomtesting
- **Documentation**: See `/docs` folder
- **Issues**: GitHub Issues

---

## 📝 Summary

This automation testing framework provides a **robust, scalable, and maintainable** solution for API testing with:

✅ **BDD approach** for clear test specifications
✅ **Centralized configuration** for easy management
✅ **Comprehensive reporting** for test visibility
✅ **Parallel execution** for faster feedback
✅ **Clean architecture** for long-term maintainability

The framework is production-ready and supports continuous integration, making it an ideal choice for modern development teams.

---

**Last Updated**: October 17, 2025
**Version**: 1.0
**Maintained By**: Automation Test Team
