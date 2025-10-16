Feature: Audit API
    As a system user
    I want to retrieve audit data
    So that I can track system activities

# POSITIVE CASES

@positive @auditHistory
Scenario: 001 - Retrieve audit history list successfully
    When I request audit history data
    Then the response status code should be 200
    And the Content-Type header should be "application/json"
    And the response body should be a JSON array
    And each item should include "AuditId","createdAt","createdBy","PDescription","TDescription","details","ImpKey"
    And "AuditId","createdBy","ImpKey" should be valid GUIDs for each item
    And "createdAt" should be a valid ISO 8601 UTC timestamp for each item
    And "PDescription","TDescription","details" should be non-empty strings for each item

@positive @auditHistory
Scenario: 002 - Retrieve audit history list when there are no records
    Given the audit history store is empty
    When I request audit history data
    Then the response status code should be 200
    And the response body should be an empty JSON array

@positive @auditHistory
Scenario: 003 - Retrieve audit history by id successfully
    Given an existing audit history id
    When I request audit history by id with that id
    Then the response status code should be 200
    And the response body should include "AuditId","createdAt","createdBy","PDescription","TDescription","details","ImpKey"
    And the "AuditId" in the response should match the requested id

@positive @auditHistory @filter
Scenario: 004 - List audit history filtered by tableDescription
    When I request audit history data filtered by tableDescription equals "Orders"
    Then the response status code should be 200
    And the response body should be a JSON array
    And each item's "TDescription" should equal "Orders"

@positive @auditHistory @filter
Scenario: 005 - List audit history filtered by createdBy
    Given a valid createdBy id
    When I request audit history data filtered by createdBy equals that id
    Then the response status code should be 200
    And each item's "createdBy" should match the requested id

@positive @auditHistory @filter
Scenario: 006 - List audit history filtered by createdAt date range
    When I request audit history data filtered by createdAt between "2024-01-01T00:00:00Z" and "2024-12-31T23:59:59Z"
    Then the response status code should be 200
    And each item's "createdAt" should be within the requested range

@positive @auditHistory @pagination
Scenario: 007 - List audit history with pagination
    When I request audit history data with page "2" and size "10"
    Then the response status code should be 200
    And the response should include exactly "10" items or fewer
    And pagination metadata should indicate page "2" and size "10"

@positive @auditHistory @sorting
Scenario: 008 - List audit history sorted by createdAt descending
    When I request audit history data sorted by "createdAt" in "desc" order
    Then the response status code should be 200
    And the items should be ordered by "createdAt" in descending order

@positive @auditHistory @validation
Scenario: 009 - Response uses expected JSON property names (camelCase)
    When I request audit history data
    Then the response status code should be 200
    And each item should include "AuditId","createdAt","createdBy","PDescription","TDescription","details","ImpKey"
    And no PascalCase properties like "CreatedAt" should be present

@positive @auditHistory @validation
Scenario: 010 - Validate GUIDs are not empty
    When I request audit history data
    Then the response status code should be 200
    And "AuditId","createdBy","ImpKey" should not be empty GUIDs for each item

@positive @auditHistory @validation
Scenario: 011 - Validate createdAt is not in the future
    When I request audit history data
    Then the response status code should be 200
    And each item's "createdAt" should not be in the future

# NEGATIVE CASES

@negative @auditHistory
Scenario: 012 - Retrieve audit history by id that does not exist
    When I request audit history by id with a non-existent auditHistoryId
    Then the response status code should be 404

@negative @auditHistory
Scenario: 013 - Retrieve audit history by id with an invalid id format
    When I request audit history by id with an invalid GUID format
    Then the response status code should be 400

@negative @auditHistory @authentication
Scenario: 014 - Retrieve audit history without authentication
    When I request audit history data without authentication
    Then the response status code should be 401

@negative @auditHistory @authorization
Scenario: 015 - Retrieve audit history with insufficient permissions
    When I request audit history data with insufficient permissions
    Then the response status code should be 403

@negative @auditHistory @pagination
Scenario: 016 - List audit history with invalid pagination parameters (size too large)
    When I request audit history data with page "1" and size "10000"
    Then the response status code should be 400

@negative @auditHistory @pagination
Scenario: 017 - List audit history with invalid pagination parameters (negative values)
    When I request audit history data with page "-1" and size "0"
    Then the response status code should be 400

@negative @auditHistory @pagination
Scenario: 018 - List audit history with page beyond available range
    When I request audit history data with page "9999" and size "50"
    Then the response status code should be 200
    And the response body should be an empty JSON array

@negative @auditHistory @filter
Scenario: 019 - List audit history with invalid createdBy filter
    When I request audit history data filtered by createdBy equals "not-a-guid"
    Then the response status code should be 400

@negative @auditHistory @filter
Scenario: 020 - List audit history with invalid createdAt range (from after to)
    When I request audit history data filtered by createdAt between "2025-12-31T00:00:00Z" and "2025-01-01T00:00:00Z"
    Then the response status code should be 400

@negative @auditHistory @sorting
Scenario: 021 - List audit history with invalid sort parameter
    When I request audit history data sorted by "unknownField" in "asc" order
    Then the response status code should be 400

@negative @auditHistory @contentNegotiation
Scenario: 022 - Content negotiation failure when requesting unsupported media type
    When I request audit history data with Accept header "application/xml"
    Then the response status code should be 406

# LEGACY TESTS
@validRequest @legacy
Scenario: 023 - Retrieve audit data with valid request
    Given I load audit request data from "AuditTestData.json" using "validRequest"
    When I send the audit data request
    Then the response status code should be 200
    And the audit response should be valid

@invalidRequest @legacy
Scenario: 024 - Validate authentication for audit API
    Given I load audit request data from "AuditTestData.json" using "invalidRequest"
    When I send the audit data request
    Then the response status code should be 401
