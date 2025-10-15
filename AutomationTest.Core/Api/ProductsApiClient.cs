using RestSharp;
using System.Text.Json;
using AutomationTest.Core.Models;

namespace AutomationTest.Core.Api
{
    public class ProductsApiClient
    {
        private readonly RestClient _client;
        private const string BaseUrl = "https://dummyjson.com";
        private RestResponse? _lastResponse;

        public ProductsApiClient()
        {
            _client = new RestClient(BaseUrl);
        }

        public async Task<ProductResponse> GetAllProductsAsync()
        {
            var request = new RestRequest("/products");
            _lastResponse = await _client.ExecuteAsync(request);
            return JsonSerializer.Deserialize<ProductResponse>(_lastResponse.Content ?? throw new InvalidOperationException("Response content was null")) 
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        public async Task<Product> GetProductAsync(int id)
        {
            var request = new RestRequest($"/products/{id}");
            _lastResponse = await _client.ExecuteAsync(request);
            return JsonSerializer.Deserialize<Product>(_lastResponse.Content ?? throw new InvalidOperationException("Response content was null"))
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        public async Task<ProductResponse> SearchProductsAsync(string query)
        {
            var request = new RestRequest($"/products/search?q={query}");
            _lastResponse = await _client.ExecuteAsync(request);
            return JsonSerializer.Deserialize<ProductResponse>(_lastResponse.Content ?? throw new InvalidOperationException("Response content was null"))
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        public RestResponse GetLastResponse()
        {
            return _lastResponse ?? throw new InvalidOperationException("No response available");
        }
    }
}
