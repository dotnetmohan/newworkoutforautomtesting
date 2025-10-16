Feature: User API
    As a system user
    I want to validate user data
    So that I can ensure user information is retrievable

Scenario: 001 - Retrieve user data
    When I request user data
    Then the response status code should be 200
    And the user response should be valid
