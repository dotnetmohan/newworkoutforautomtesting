# Audit API - Sequence Diagrams

## Positive Scenario: Retrieve Audit History List Successfully

This diagram shows the successful flow of retrieving audit history data.

```mermaid
sequenceDiagram
    participant Test as Test Runner
    participant Step as Step Definition
    participant Client as AuditApiClient
    participant Auth as AuthenticationClient
    participant API as Audit API
    participant Report as ExtentReport

    Test->>Step: Execute Scenario 001
    Note over Test,Step: "Retrieve audit history list successfully"
    
    Step->>Client: GetAuditHistoryListAsync()
    activate Client
    
    Client->>Auth: GetAccessTokenAsync()
    activate Auth
    Auth->>API: POST /token
    API-->>Auth: 200 OK (access_token)
    Auth-->>Client: Return access_token
    deactivate Auth
    
    Note over Client: Build request with headers:<br/>Authorization: Bearer {token}<br/>Accept: application/json<br/>ObjectId, Cored, Type
    
    Client->>API: GET /audit-history
    activate API
    
    Note over API: Process request<br/>Validate authentication<br/>Query database
    
    API-->>Client: 200 OK (JSON array)
    deactivate API
    
    Client-->>Step: RestResponse (Status: 200)
    deactivate Client
    
    Step->>Step: Assert status code = 200
    Step->>Step: Assert Content-Type = "application/json"
    Step->>Step: Assert response is JSON array
    Step->>Step: Validate each item has required fields
    Step->>Step: Validate GUIDs are valid
    Step->>Step: Validate timestamps are ISO 8601
    
    Step->>Report: CreateNode<Given>("request audit history")
    Step->>Report: CreateNode<Then>("status code = 200") ✅
    Step->>Report: CreateNode<Then>("validate response") ✅
    
    Step-->>Test: Scenario PASSED ✅
    
    Note over Test,Report: All assertions passed<br/>Report updated with success
```

---

## Negative Scenario: Retrieve Audit History Without Authentication

This diagram shows the flow when authentication fails (401 Unauthorized).

```mermaid
sequenceDiagram
    participant Test as Test Runner
    participant Step as Step Definition
    participant Client as AuditApiClient
    participant API as Audit API
    participant Report as ExtentReport

    Test->>Step: Execute Scenario 014
    Note over Test,Step: "Retrieve audit history without authentication"
    
    Step->>Client: GetAuditHistoryListAsync(useAuthentication: false)
    activate Client
    
    Note over Client: Build request WITHOUT Authorization header<br/>Only include:<br/>Accept: application/json<br/>ObjectId, Cored, Type
    
    Client->>API: GET /audit-history
    activate API
    
    Note over API: Process request<br/>Check Authorization header<br/>Header missing or invalid
    
    API-->>Client: 401 Unauthorized
    Note over API: Response body:<br/>{<br/>  "error": "Unauthorized",<br/>  "message": "Missing authentication"<br/>}
    deactivate API
    
    Client-->>Step: RestResponse (Status: 401)
    deactivate Client
    
    Step->>Step: Assert status code = 401 ✅
    
    Step->>Report: CreateNode<When>("request without authentication")
    Step->>Report: CreateNode<Then>("status code = 401") ✅
    
    Step-->>Test: Scenario PASSED ✅
    
    Note over Test,Report: Expected failure occurred<br/>Security validation confirmed
```

---

## Alternative Negative Scenario: Retrieve Audit History with Insufficient Permissions

This diagram shows the flow when user lacks proper permissions (403 Forbidden).

```mermaid
sequenceDiagram
    participant Test as Test Runner
    participant Step as Step Definition
    participant Client as AuditApiClient
    participant API as Audit API
    participant Report as ExtentReport

    Test->>Step: Execute Scenario 015
    Note over Test,Step: "Retrieve audit history with insufficient permissions"
    
    Step->>Client: GetAuditHistoryWithInsufficientPermissionsAsync()
    activate Client
    
    Note over Client: Build request with INVALID token<br/>Authorization: Bearer insufficient_permissions_token<br/>Accept: application/json<br/>ObjectId, Cored, Type
    
    Client->>API: GET /audit-history
    activate API
    
    Note over API: Process request<br/>Validate token<br/>Check user permissions<br/>User lacks required permissions
    
    API-->>Client: 403 Forbidden
    Note over API: Response body:<br/>{<br/>  "error": "Forbidden",<br/>  "message": "Insufficient permissions"<br/>}
    deactivate API
    
    Client-->>Step: RestResponse (Status: 403)
    deactivate Client
    
    Step->>Step: Assert status code = 403 ✅
    
    Step->>Report: CreateNode<When>("request with insufficient permissions")
    Step->>Report: CreateNode<Then>("status code = 403") ✅
    
    Step-->>Test: Scenario PASSED ✅
    
    Note over Test,Report: Authorization check working correctly<br/>Security validation confirmed
```

---

## Detailed Flow: Retrieve Audit History by ID (Positive with Full Details)

This diagram shows a more detailed flow including all components.

```mermaid
sequenceDiagram
    participant Test as Test Runner (xUnit)
    participant Hook as Hooks (Before/After)
    participant Step as AuditStepDefinitions
    participant Client as AuditApiClient
    participant Helper as CreateRequestWithCommonHeadersAsync()
    participant Auth as AuthenticationClient
    participant API as Audit API Server
    participant Report as ExtentReport

    Test->>Hook: [BeforeScenario]
    Hook->>Report: CreateNode<Scenario>("003 - Retrieve by ID")
    Hook-->>Test: Scenario initialized
    
    Test->>Step: Execute "Given an existing audit history id"
    Step->>Step: Set _existingAuditHistoryId = "guid-123"
    Step->>Report: CreateNode<Given>("existing audit history id")
    
    Test->>Step: Execute "When I request audit history by id"
    activate Step
    
    Step->>Client: GetAuditHistoryByIdAsync("guid-123", true)
    activate Client
    
    Note over Client: Build URL from template<br/>AuditHistoryIdApiUrl.Replace("{AuditHistoryId}", "guid-123")
    
    Client->>Helper: CreateRequestWithCommonHeadersAsync(url, Method.Get, true)
    activate Helper
    
    Helper->>Auth: GetAccessTokenAsync()
    activate Auth
    Auth->>API: POST /token
    Note over Auth,API: Body: {client_id, client_secret, scope}
    API-->>Auth: 200 OK {access_token, expires_in}
    Auth-->>Helper: "Bearer eyJhbGciOi..."
    deactivate Auth
    
    Note over Helper: Add all common headers:<br/>✓ Accept: application/json<br/>✓ Authorization: Bearer token<br/>✓ ObjectId: 5C8C2E10...<br/>✓ Cored: 6C8C2E10...<br/>✓ Type: TeamTest
    
    Helper-->>Client: RestRequest (fully configured)
    deactivate Helper
    
    Client->>API: GET /audit-history/guid-123
    activate API
    
    Note over API: 1. Validate authentication token<br/>2. Validate GUID format<br/>3. Query database by ID<br/>4. Check if record exists<br/>5. Build response
    
    API-->>Client: 200 OK
    Note over API: Response body:<br/>{<br/>  "AuditId": "guid-123",<br/>  "createdAt": "2024-10-18T10:00:00Z",<br/>  "createdBy": "user-guid",<br/>  "PDescription": "...",<br/>  "TDescription": "...",<br/>  "details": "...",<br/>  "ImpKey": "imp-guid"<br/>}
    deactivate API
    
    Client-->>Step: RestResponse
    deactivate Client
    
    Step->>Step: Store _lastResponse
    Step->>Report: CreateNode<When>("request by ID")
    deactivate Step
    
    Test->>Step: Execute "Then the response status code should be 200"
    Step->>Step: Assert _lastResponse.StatusCode == 200 ✅
    Step->>Report: CreateNode<Then>("status code = 200") ✅
    
    Test->>Step: Execute "And the response body should include required fields"
    Step->>Step: Parse JSON and validate fields ✅
    Step->>Report: CreateNode<Then>("validate fields") ✅
    
    Test->>Step: Execute "And the AuditId should match requested id"
    Step->>Step: Assert response.AuditId == "guid-123" ✅
    Step->>Report: CreateNode<Then>("ID matches") ✅
    
    Test->>Hook: [AfterScenario]
    Hook->>Report: Mark scenario as PASSED ✅
    Hook-->>Test: Cleanup completed
    
    Note over Test,Report: Total Duration: 250ms<br/>Status: PASSED ✅<br/>All assertions successful
```

---

## Error Flow: Retrieve Audit History by Non-Existent ID (Negative)

```mermaid
sequenceDiagram
    participant Test as Test Runner
    participant Step as Step Definition
    participant Client as AuditApiClient
    participant Auth as AuthenticationClient
    participant API as Audit API
    participant Report as ExtentReport

    Test->>Step: Execute Scenario 012
    Note over Test,Step: "Retrieve by ID that does not exist"
    
    Step->>Step: Set auditHistoryId = "00000000-0000-0000-0000-000000000000"
    
    Step->>Client: GetAuditHistoryByIdAsync("00000000-...", true)
    activate Client
    
    Client->>Auth: GetAccessTokenAsync()
    Auth->>API: POST /token
    API-->>Auth: 200 OK (token)
    Auth-->>Client: access_token
    
    Client->>API: GET /audit-history/00000000-0000-0000-0000-000000000000
    activate API
    
    Note over API: 1. Validate token ✅<br/>2. Parse GUID ✅<br/>3. Query database<br/>4. No record found ❌
    
    API-->>Client: 404 Not Found
    Note over API: Response:<br/>{<br/>  "error": "Not Found",<br/>  "message": "Audit history not found"<br/>}
    deactivate API
    
    Client-->>Step: RestResponse (Status: 404)
    deactivate Client
    
    Step->>Step: Assert status code = 404 ✅
    
    Step->>Report: CreateNode<When>("request non-existent ID")
    Step->>Report: CreateNode<Then>("status code = 404") ✅
    
    Step-->>Test: Scenario PASSED ✅
    
    Note over Test,Report: Negative test successful<br/>Error handling verified
```

---

## Legend

### Participants
- **Test Runner**: xUnit test execution engine
- **Step Definition**: BDD step implementation (Given/When/Then)
- **AuditApiClient**: Core API client for Audit endpoints
- **AuthenticationClient**: Handles OAuth token retrieval
- **Audit API**: Target API server
- **ExtentReport**: HTML report generation

### Status Indicators
- ✅ Success / Assertion passed
- ❌ Failure / Record not found
- 🔒 Authentication/Authorization check

### HTTP Status Codes
- **200 OK**: Successful request
- **401 Unauthorized**: Missing or invalid authentication
- **403 Forbidden**: Valid auth but insufficient permissions
- **404 Not Found**: Resource doesn't exist

---

## Key Takeaways

### Positive Scenario Benefits
1. ✅ Validates happy path functionality
2. ✅ Confirms authentication flow works
3. ✅ Verifies response structure and data
4. ✅ Tests end-to-end integration

### Negative Scenario Benefits
1. ✅ Validates error handling
2. ✅ Confirms security measures work
3. ✅ Tests API resilience
4. ✅ Verifies proper HTTP status codes

### Framework Features Highlighted
1. 🔄 Reusable API clients
2. 🔐 Centralized authentication
3. 📊 Comprehensive reporting
4. 🎯 BDD test structure
5. ✨ Clean separation of concerns
