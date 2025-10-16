# Example: How to Use Order Tags

Feature: Audit API with Execution Order
    This example shows how to control scenario execution order using @order:N tags

# SETUP - Run first
@setup @order:1
Scenario: Setup test environment
    Given the test environment is initialized
    When test data is created
    Then the setup should be successful

# POSITIVE TESTS - Run second
@positive @order:10
Scenario: Test basic functionality first
    When I test basic features
    Then they should work

@positive @order:11
Scenario: Test advanced functionality second
    When I test advanced features
    Then they should work

# NEGATIVE TESTS - Run third
@negative @order:20
Scenario: Test error handling
    When I test error scenarios
    Then errors should be handled properly

# CLEANUP - Run last
@cleanup @order:999
Scenario: Cleanup test environment
    Given tests are completed
    When I cleanup test data
    Then cleanup should be successful

# NOTES:
# - Lower numbers run first
# - Tests without @order run last (in alphabetical order)
# - Tests with same order number run alphabetically
# - Use gaps (1, 10, 20, etc.) to allow inserting tests later
