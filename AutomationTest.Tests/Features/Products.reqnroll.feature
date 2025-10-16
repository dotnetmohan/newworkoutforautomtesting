Feature: Products API Testing
    As an API consumer
    I want to test the Products API endpoints
    So that I can ensure they work correctly

Scenario: 001 - Get all products
    When I send a GET request to "/products"
    Then the response status code should be 200
    And the response should contain a list of products

Scenario: 002 - Get a specific product
    When I send a GET request to "/products/1"
    Then the response status code should be 200
    And the response should contain product details

Scenario: 003 - Search for products
    When I send a GET request to "/products/search?q=phone"
    Then the response status code should be 200
    And the response should contain matching products
