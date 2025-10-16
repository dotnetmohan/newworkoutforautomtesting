using AutomationTest.Core.Configuration;
using AutomationTest.Core.Models;
using AutomationTest.Core.Services;
using RestSharp;
using System.Text.Json;

namespace AutomationTest.Core.Api
{
    public class AuditApiClient
    {
        private readonly RestClient _client;
        private readonly AuthenticationClient _authClient;
        private readonly LoggerService _logger;
        private RestResponse? _lastResponse;
        private string? _lastAccessToken;

        public AuditApiClient()
        {
            _client = new RestClient();
            _authClient = new AuthenticationClient();
            _logger = LoggerService.Instance;
            _logger.LogInformation("AuditApiClient initialized");
        }

        public RestResponse GetLastResponse()
        {
            return _lastResponse ?? throw new InvalidOperationException("No request has been made yet.");
        }

        /// <summary>
        /// Creates a RestRequest with common headers added
        /// </summary>
        private async Task<RestRequest> CreateRequestWithCommonHeadersAsync(string url, Method method, bool useAuthentication = true, string acceptHeader = "application/json")
        {
            var request = new RestRequest(url, method);

            // Add Accept header
            if (!string.IsNullOrEmpty(acceptHeader))
            {
                request.AddHeader("Accept", acceptHeader);
            }

            // Add Content-Type header for POST/PUT/PATCH requests
            if (method == Method.Post || method == Method.Put || method == Method.Patch)
            {
                request.AddHeader("Content-Type", "application/json");
            }

            // Add authentication token if required
            if (useAuthentication)
            {
                var accessToken = await _authClient.GetAccessTokenAsync();
                _lastAccessToken = accessToken;
                request.AddHeader("Authorization", $"Bearer {accessToken}");
            }

            // Add mandatory headers for all API requests
            request.AddHeader("ObjectId", "5C8C2E10-FCB5-4C0C-8344-88F315E31206");
            request.AddHeader("Cored", "6C8C2E10-FCB5-4C0C-8344-88F315E31206");
            request.AddHeader("Type", "TeamTest");

            return request;
        }

        public async Task<string> GetAuditDataAsync(AuditRequest auditRequest)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Requesting audit data with BillingId: {BillingId}", auditRequest.BillingId);
            
            try
            {
                var request = await CreateRequestWithCommonHeadersAsync(ApiSettings.AuditApiUrl, Method.Post);
                
                // Add request body
                request.AddJsonBody(auditRequest);

                _lastResponse = await _client.ExecuteAsync(request);
                
                var duration = DateTime.UtcNow - startTime;
                var statusCode = (int)_lastResponse.StatusCode;
                
                _logger.TrackApiRequest("AuditData", "POST", statusCode, duration, _lastResponse.IsSuccessful);
                
                if (!_lastResponse.IsSuccessful)
                {
                    var errorMsg = $"API request failed. Status: {_lastResponse.StatusCode}, Error: {_lastResponse.ErrorMessage}";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                _logger.LogInformation("Successfully retrieved audit data. Response length: {Length}", _lastResponse.Content?.Length ?? 0);
                return _lastResponse.Content!;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Error getting audit data: {ErrorMessage}", ex.Message);
                _logger.TrackApiRequest("AuditData", "POST", 0, duration, false);
                throw;
            }
        }

        public async Task<RestResponse> GetAuditHistoryListAsync(
            string? tableDescription = null,
            string? createdBy = null,
            string? createdAtFrom = null,
            string? createdAtTo = null,
            int? page = null,
            int? size = null,
            string? sortBy = null,
            string? sortOrder = null,
            bool useAuthentication = true,
            string? acceptHeader = "application/json")
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Requesting audit history list with filters - TableDescription: {TableDescription}, CreatedBy: {CreatedBy}, Page: {Page}, Size: {Size}",
                tableDescription ?? "null", createdBy ?? "null", page?.ToString() ?? "null", size?.ToString() ?? "null");
            
            try
            {
                var request = await CreateRequestWithCommonHeadersAsync(
                    ApiSettings.AuditHistoryApiUrl, 
                    Method.Get, 
                    useAuthentication, 
                    acceptHeader ?? "application/json");

                // Add query parameters
                if (!string.IsNullOrEmpty(tableDescription))
                    request.AddQueryParameter("tableDescription", tableDescription);
                
                if (!string.IsNullOrEmpty(createdBy))
                    request.AddQueryParameter("createdBy", createdBy);
                
                if (!string.IsNullOrEmpty(createdAtFrom))
                    request.AddQueryParameter("createdAtFrom", createdAtFrom);
                
                if (!string.IsNullOrEmpty(createdAtTo))
                    request.AddQueryParameter("createdAtTo", createdAtTo);
                
                if (page.HasValue)
                    request.AddQueryParameter("page", page.Value.ToString());
                
                if (size.HasValue)
                    request.AddQueryParameter("size", size.Value.ToString());
                
                if (!string.IsNullOrEmpty(sortBy))
                    request.AddQueryParameter("sortBy", sortBy);
                
                if (!string.IsNullOrEmpty(sortOrder))
                    request.AddQueryParameter("sortOrder", sortOrder);

                _lastResponse = await _client.ExecuteAsync(request);
                
                var duration = DateTime.UtcNow - startTime;
                var statusCode = (int)_lastResponse.StatusCode;
                
                _logger.TrackApiRequest("AuditHistoryList", "GET", statusCode, duration, _lastResponse.IsSuccessful);
                _logger.LogInformation("Audit history list request completed. Status: {StatusCode}, Duration: {Duration}ms",
                    statusCode, duration.TotalMilliseconds);
                
                return _lastResponse;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Error getting audit history list: {ErrorMessage}", ex.Message);
                _logger.TrackApiRequest("AuditHistoryList", "GET", 0, duration, false);
                throw;
            }
        }

        public async Task<RestResponse> GetAuditHistoryByIdAsync(string auditHistoryId, bool useAuthentication = true)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Requesting audit history by ID: {AuditHistoryId}", auditHistoryId);
            
            try
            {
                // Build the URL by replacing the placeholder with the actual ID
                var url = ApiSettings.AuditHistoryIdApiUrl.Replace("{AuditHistoryId}", auditHistoryId);
                var request = await CreateRequestWithCommonHeadersAsync(url, Method.Get, useAuthentication);

                _lastResponse = await _client.ExecuteAsync(request);
                
                var duration = DateTime.UtcNow - startTime;
                var statusCode = (int)_lastResponse.StatusCode;
                
                _logger.TrackApiRequest($"AuditHistoryById/{auditHistoryId}", "GET", statusCode, duration, _lastResponse.IsSuccessful);
                _logger.LogInformation("Audit history by ID request completed. Status: {StatusCode}, Duration: {Duration}ms",
                    statusCode, duration.TotalMilliseconds);
                
                return _lastResponse;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Error getting audit history by ID {AuditHistoryId}: {ErrorMessage}", auditHistoryId, ex.Message);
                _logger.TrackApiRequest($"AuditHistoryById/{auditHistoryId}", "GET", 0, duration, false);
                throw;
            }
        }

        public async Task<RestResponse> GetAuditHistoryWithInsufficientPermissionsAsync()
        {
            var startTime = DateTime.UtcNow;
            _logger.LogWarning("Requesting audit history with insufficient permissions token");
            
            try
            {
                // Simulate insufficient permissions by using a different/invalid token
                // Don't use the helper method here as we need to override with an invalid token
                var request = new RestRequest(ApiSettings.AuditHistoryApiUrl, Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("ObjectId", "5C8C2E10-FCB5-4C0C-8344-88F315E31206");
                request.AddHeader("Cored", "6C8C2E10-FCB5-4C0C-8344-88F315E31206");
                request.AddHeader("Type", "TeamTest");
                request.AddHeader("Authorization", "Bearer insufficient_permissions_token");

                _lastResponse = await _client.ExecuteAsync(request);
                
                var duration = DateTime.UtcNow - startTime;
                var statusCode = (int)_lastResponse.StatusCode;
                
                _logger.TrackApiRequest("AuditHistoryInsufficientPermissions", "GET", statusCode, duration, _lastResponse.IsSuccessful);
                _logger.LogInformation("Insufficient permissions request completed. Status: {StatusCode}", statusCode);
                
                return _lastResponse;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Error in insufficient permissions test: {ErrorMessage}", ex.Message);
                _logger.TrackApiRequest("AuditHistoryInsufficientPermissions", "GET", 0, duration, false);
                throw;
            }
        }
    }
}
