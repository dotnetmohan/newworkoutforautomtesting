using AutomationTest.Core.Api;
using AutomationTest.Core.Models;
using AutomationTest.Tests.Constants;
using RestSharp;
using Reqnroll;
using System.Linq;

namespace AutomationTest.Tests.Steps
{
    [Binding]
    public class ProductsStepDefinitions
    {
        private readonly ProductsApiClient _apiClient;
        private RestResponse _lastResponse = null!;
        private ProductResponse _productsResponse = null!;
        private Product _productResponse = null!;

        public ProductsStepDefinitions()
        {
            _apiClient = new ProductsApiClient();
        }

        [When(@"I send a GET request to ""(.*)""")]
        public async Task WhenISendAGETRequestTo(string endpoint)
        {
            if (endpoint.Contains("search"))
            {
                _productsResponse = await _apiClient.SearchProductsAsync("phone");
            }
            else if (endpoint.Contains("/products/"))
            {
                _productResponse = await _apiClient.GetProductAsync(1);
            }
            else
            {
                _productsResponse = await _apiClient.GetAllProductsAsync();
            }
            _lastResponse = _apiClient.GetLastResponse();
        }

        [Then(@"the response status code should be (.*)")]
        public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
        {
            int actualStatusCode = (int)_lastResponse.StatusCode;
            Assert.True(expectedStatusCode == actualStatusCode, 
                AssertionMessages.StatusCodeMismatch(expectedStatusCode, actualStatusCode));
        }

        [Then(@"the response should contain a list of products")]
        public void ThenTheResponseShouldContainAListOfProducts()
        {
            Assert.True(_productsResponse != null, AssertionMessages.ProductsResponseNull);
            Assert.True(_productsResponse.Products != null, AssertionMessages.ProductsListNull);
            Assert.True(_productsResponse.Products.Any(), AssertionMessages.ProductsListEmpty);
        }

        [Then(@"the response should contain product details")]
        public void ThenTheResponseShouldContainProductDetails()
        {
            Assert.True(_productResponse != null, AssertionMessages.ProductResponseNull);
            Assert.True(_productResponse.Id > 0, AssertionMessages.ProductIdInvalid(_productResponse.Id));
            Assert.True(!string.IsNullOrEmpty(_productResponse.Title), AssertionMessages.ProductTitleEmpty);
            Assert.True(!string.IsNullOrEmpty(_productResponse.Description), AssertionMessages.ProductDescriptionEmpty);
        }

        [Then(@"the response should contain matching products")]
        public void ThenTheResponseShouldContainMatchingProducts()
        {
            Assert.True(_productsResponse != null, AssertionMessages.SearchResponseNull);
            Assert.True(_productsResponse.Products != null, AssertionMessages.SearchResultsNull);
            Assert.True(_productsResponse.Products.Any(), AssertionMessages.SearchResultsEmpty);
            Assert.True(_productsResponse.Products.Any(p => 
                p.Title.ToLower().Contains("phone") || p.Description.ToLower().Contains("phone")), 
                AssertionMessages.NoMatchingProducts);
        }
    }
}
