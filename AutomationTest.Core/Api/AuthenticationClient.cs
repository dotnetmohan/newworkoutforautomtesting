using AutomationTest.Core.Configuration;
using AutomationTest.Core.Models;
using RestSharp;
using System.Text.Json;

namespace AutomationTest.Core.Api
{
    public class AuthenticationClient
    {
        private readonly RestClient _client;

        public AuthenticationClient()
        {
            _client = new RestClient(ApiSettings.TokenUrl);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var request = new RestRequest("", Method.Get);
            request.AddHeader("ocp-apim-subscription-key", ApiSettings.SubscriptionKey);

            var response = await _client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                throw new Exception($"Failed to get access token. Status: {response.StatusCode}, Error: {response.ErrorMessage}");
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content!);
            return tokenResponse?.AccessToken ?? throw new Exception("Access token not found in response");
        }
    }
}
