using AutomationTest.Core.Configuration;
using AutomationTest.Core.Models;
using RestSharp;
using System.Text.Json;

namespace AutomationTest.Core.Api
{
    public class UserApiClient
    {
        private readonly RestClient _client;
        private RestResponse? _lastResponse;
        private readonly AuthenticationClient _authClient;

        public UserApiClient()
        {
            _client = new RestClient(ApiSettings.AuditApiUrl.Replace("getAuditdata", "getUserdata"));
            _authClient = new AuthenticationClient();
        }

        public RestResponse GetLastResponse() => _lastResponse ?? throw new InvalidOperationException("No request made yet");

        public async Task<UserResponse> GetUserDataAsync()
        {
            var token = await _authClient.GetAccessTokenAsync();

            var request = new RestRequest("", Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {token}");

            _lastResponse = await _client.ExecuteAsync(request);

            if (!_lastResponse.IsSuccessful)
            {
                throw new Exception($"GetUserData failed. Status: {_lastResponse.StatusCode}");
            }

            var userResponse = JsonSerializer.Deserialize<UserResponse>(_lastResponse.Content!);
            return userResponse ?? throw new Exception("Invalid user response");
        }
    }
}
