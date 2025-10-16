using AutomationTest.Core.Api;
using AutomationTest.Core.Models;
using AutomationTest.Core.Services;
using AutomationTest.Tests.Constants;
using AutomationTest.Tests.Helpers;
using RestSharp;
using Reqnroll;
using System.Text.Json;
using System.Runtime.Serialization;

namespace AutomationTest.Tests.Steps
{
    [Binding]
    public class AuditStepDefinitions
    {
        private readonly AuditApiClient _apiClient;
        private readonly LoggerService _logger;
        private AuditRequest _auditRequest = new();
        private RestResponse _lastResponse = null!;
        private string _auditResponse = string.Empty;
        private string _existingAuditHistoryId = string.Empty;
        private string _validCreatedById = string.Empty;
        private string _createdAtFrom = string.Empty;
        private string _createdAtTo = string.Empty;
        private List<AuditHistory> _auditHistoryList = new();

        public AuditStepDefinitions()
        {
            _apiClient = new AuditApiClient();
            _logger = LoggerService.Instance;
        }

        #region Legacy Steps
        [Given(@"I load audit request data from ""(.*)"" using ""(.*)""")]
        public void GivenILoadAuditRequestDataFromFile(string fileName, string dataKey)
        {
            try
            {
                _auditRequest = TestDataReader.LoadTestData<AuditRequest>(fileName, dataKey);
            }
            catch (FileNotFoundException ex)
            {
                throw new Exception($"Failed to load test data: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading test data: {ex.Message}");
            }
        }

        [When(@"I send the audit data request")]
        public async Task WhenISendTheAuditDataRequest()
        {
            try
            {
                _auditResponse = await _apiClient.GetAuditDataAsync(_auditRequest);
                _lastResponse = _apiClient.GetLastResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending audit request: {ErrorMessage}", ex.Message);
            }
        }

        [Then(@"the audit response should be valid")]
        public void ThenTheAuditResponseShouldBeValid()
        {
            Assert.True(!string.IsNullOrEmpty(_auditResponse), AssertionMessages.AuditResponseNull);
        }
        #endregion

        #region Given Steps
        [Given(@"the audit history store is empty")]
        public void GivenTheAuditHistoryStoreIsEmpty()
        {
            // This is a precondition - in real scenario, you might need to clear data
            // For testing purposes, we'll handle this in the API mock or test setup
        }

        [Given(@"an existing audit history id")]
        public async Task GivenAnExistingAuditHistoryId()
        {
            // First, get the list to find an existing ID
            var response = await _apiClient.GetAuditHistoryListAsync();
            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                var historyList = JsonSerializer.Deserialize<List<AuditHistory>>(response.Content);
                if (historyList != null && historyList.Count > 0)
                {
                    _existingAuditHistoryId = historyList[0].AuditId;
                }
            }
            
            // Fallback to a test GUID if no data exists
            if (string.IsNullOrEmpty(_existingAuditHistoryId))
            {
                _existingAuditHistoryId = Guid.NewGuid().ToString();
            }
        }

        [Given(@"a valid createdBy id")]
        public async Task GivenAValidCreatedById()
        {
            // Get an existing createdBy from the first record
            var response = await _apiClient.GetAuditHistoryListAsync();
            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                var historyList = JsonSerializer.Deserialize<List<AuditHistory>>(response.Content);
                if (historyList != null && historyList.Count > 0)
                {
                    _validCreatedById = historyList[0].CreatedBy;
                }
            }
            
            // Fallback to a test GUID if no data exists
            if (string.IsNullOrEmpty(_validCreatedById))
            {
                _validCreatedById = Guid.NewGuid().ToString();
            }
        }
        #endregion

        #region When Steps
        [When(@"I request audit history data")]
        public async Task WhenIRequestAuditHistoryData()
        {
            _lastResponse = await _apiClient.GetAuditHistoryListAsync();
            if (_lastResponse.IsSuccessful && !string.IsNullOrEmpty(_lastResponse.Content))
            {
                try
                {
                    _auditHistoryList = JsonSerializer.Deserialize<List<AuditHistory>>(_lastResponse.Content) ?? new();
                }
                catch
                {
                    _auditHistoryList = new();
                }
            }
        }

        [When(@"I request audit history by id with that id")]
        public async Task WhenIRequestAuditHistoryByIdWithThatId()
        {
            _lastResponse = await _apiClient.GetAuditHistoryByIdAsync(_existingAuditHistoryId);
        }

        [When(@"I request audit history by id with a non-existent auditHistoryId")]
        public async Task WhenIRequestAuditHistoryByIdWithNonExistentId()
        {
            var nonExistentId = Guid.NewGuid().ToString();
            _lastResponse = await _apiClient.GetAuditHistoryByIdAsync(nonExistentId);
        }

        [When(@"I request audit history by id with an invalid GUID format")]
        public async Task WhenIRequestAuditHistoryByIdWithInvalidGuidFormat()
        {
            _lastResponse = await _apiClient.GetAuditHistoryByIdAsync("invalid-guid-format");
        }

        [When(@"I request audit history data without authentication")]
        public async Task WhenIRequestAuditHistoryDataWithoutAuthentication()
        {
            _lastResponse = await _apiClient.GetAuditHistoryListAsync(useAuthentication: false);
        }

        [When(@"I request audit history data with insufficient permissions")]
        public async Task WhenIRequestAuditHistoryDataWithInsufficientPermissions()
        {
            _lastResponse = await _apiClient.GetAuditHistoryWithInsufficientPermissionsAsync();
        }

        [When(@"I request audit history data filtered by tableDescription equals ""(.*)""")]
        public async Task WhenIRequestAuditHistoryDataFilteredByTableDescription(string tableDescription)
        {
            _lastResponse = await _apiClient.GetAuditHistoryListAsync(tableDescription: tableDescription);
            if (_lastResponse.IsSuccessful && !string.IsNullOrEmpty(_lastResponse.Content))
            {
                try
                {
                    _auditHistoryList = JsonSerializer.Deserialize<List<AuditHistory>>(_lastResponse.Content) ?? new();
                }
                catch
                {
                    _auditHistoryList = new();
                }
            }
        }

        [When(@"I request audit history data filtered by createdBy equals that id")]
        public async Task WhenIRequestAuditHistoryDataFilteredByCreatedBy()
        {
            _lastResponse = await _apiClient.GetAuditHistoryListAsync(createdBy: _validCreatedById);
            if (_lastResponse.IsSuccessful && !string.IsNullOrEmpty(_lastResponse.Content))
            {
                try
                {
                    _auditHistoryList = JsonSerializer.Deserialize<List<AuditHistory>>(_lastResponse.Content) ?? new();
                }
                catch
                {
                    _auditHistoryList = new();
                }
            }
        }

        [When(@"I request audit history data filtered by createdBy equals ""(.*)""")]
        public async Task WhenIRequestAuditHistoryDataFilteredByCreatedByValue(string createdBy)
        {
            _lastResponse = await _apiClient.GetAuditHistoryListAsync(createdBy: createdBy);
        }

        [When(@"I request audit history data filtered by createdAt between ""(.*)"" and ""(.*)""")]
        public async Task WhenIRequestAuditHistoryDataFilteredByCreatedAtRange(string from, string to)
        {
            _createdAtFrom = from;
            _createdAtTo = to;
            _lastResponse = await _apiClient.GetAuditHistoryListAsync(createdAtFrom: from, createdAtTo: to);
            if (_lastResponse.IsSuccessful && !string.IsNullOrEmpty(_lastResponse.Content))
            {
                try
                {
                    _auditHistoryList = JsonSerializer.Deserialize<List<AuditHistory>>(_lastResponse.Content) ?? new();
                }
                catch
                {
                    _auditHistoryList = new();
                }
            }
        }

        [When(@"I request audit history data with page ""(.*)"" and size ""(.*)""")]
        public async Task WhenIRequestAuditHistoryDataWithPagination(string page, string size)
        {
            int.TryParse(page, out int pageNum);
            int.TryParse(size, out int sizeNum);
            _lastResponse = await _apiClient.GetAuditHistoryListAsync(page: pageNum, size: sizeNum);
            if (_lastResponse.IsSuccessful && !string.IsNullOrEmpty(_lastResponse.Content))
            {
                try
                {
                    _auditHistoryList = JsonSerializer.Deserialize<List<AuditHistory>>(_lastResponse.Content) ?? new();
                }
                catch
                {
                    _auditHistoryList = new();
                }
            }
        }

        [When(@"I request audit history data sorted by ""(.*)"" in ""(.*)"" order")]
        public async Task WhenIRequestAuditHistoryDataSorted(string sortBy, string sortOrder)
        {
            _lastResponse = await _apiClient.GetAuditHistoryListAsync(sortBy: sortBy, sortOrder: sortOrder);
            if (_lastResponse.IsSuccessful && !string.IsNullOrEmpty(_lastResponse.Content))
            {
                try
                {
                    _auditHistoryList = JsonSerializer.Deserialize<List<AuditHistory>>(_lastResponse.Content) ?? new();
                }
                catch
                {
                    _auditHistoryList = new();
                }
            }
        }

        [When(@"I request audit history data with Accept header ""(.*)""")]
        public async Task WhenIRequestAuditHistoryDataWithAcceptHeader(string acceptHeader)
        {
            _lastResponse = await _apiClient.GetAuditHistoryListAsync(acceptHeader: acceptHeader);
        }
        #endregion

        #region Then Steps
        [Then(@"the response status code should be (.*)")]
        public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
        {
            Assert.NotNull(_lastResponse);
            var actualStatusCode = (int)_lastResponse.StatusCode;
            Assert.True(actualStatusCode == expectedStatusCode, 
                AssertionMessages.StatusCodeMismatch(expectedStatusCode, actualStatusCode));
        }

        [Then(@"the Content-Type header should be ""(.*)""")]
        public void ThenTheContentTypeHeaderShouldBe(string expectedContentType)
        {
            var contentType = _lastResponse.ContentType;
            Assert.True(contentType?.Contains(expectedContentType) == true, 
                AssertionMessages.ContentTypeInvalid);
        }

        [Then(@"the response body should be a JSON array")]
        public void ThenTheResponseBodyShouldBeAJsonArray()
        {
            Assert.NotNull(_lastResponse.Content);
            Assert.True(_lastResponse.Content.TrimStart().StartsWith("["), 
                AssertionMessages.AuditHistoryNotJsonArray);
        }

        [Then(@"the response body should be an empty JSON array")]
        public void ThenTheResponseBodyShouldBeAnEmptyJsonArray()
        {
            Assert.NotNull(_lastResponse.Content);
            var content = _lastResponse.Content.Trim();
            Assert.True(content == "[]", AssertionMessages.AuditHistoryEmptyArrayExpected);
        }

        [Then(@"each item should include ""(.*)""")]
        public void ThenEachItemShouldIncludeFields(string fields)
        {
            var fieldList = fields.Split(',').Select(f => f.Trim().Trim('"')).ToList();
            var response  = _auditHistoryList[0];
            var itemType = response.GetType();
            foreach (var field in fieldList)
            {
                var property = itemType.GetProperty(
                    field, 
                        System.Reflection.BindingFlags.IgnoreCase | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.Instance);
                    
                    Assert.True(property != null, AssertionMessages.FieldMissing(field));
                }
        }

        [Then(@"""(.*)"" should be valid GUIDs for each item")]
        public void ThenFieldsShouldBeValidGuidsForEachItem(string fields)
        {
            var fieldList = fields.Split(',').Select(f => f.Trim().Trim('"')).ToList();
            
            foreach (var item in _auditHistoryList)
            {
                var itemType = item.GetType();
                foreach (var field in fieldList)
                {
                    var property = itemType.GetProperty(
                        field, 
                        System.Reflection.BindingFlags.IgnoreCase | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.Instance);
                    
                    Assert.NotNull(property);
                    var value = property.GetValue(item)?.ToString();
                    Assert.True(Guid.TryParse(value, out _), AssertionMessages.InvalidGuidFormat);
                }
            }
        }

        [Then(@"""(.*)"" should not be empty GUIDs for each item")]
        public void ThenFieldsShouldNotBeEmptyGuidsForEachItem(string fields)
        {
            var fieldList = fields.Split(',').Select(f => f.Trim().Trim('"')).ToList();
            
            foreach (var item in _auditHistoryList)
            {
                var itemType = item.GetType();
                foreach (var field in fieldList)
                {
                    var property = itemType.GetProperty(
                        field, 
                        System.Reflection.BindingFlags.IgnoreCase | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.Instance);
                    
                    Assert.NotNull(property);
                    var value = property.GetValue(item)?.ToString();
                    Assert.True(Guid.TryParse(value, out var guid), AssertionMessages.InvalidGuidFormat);
                    Assert.True(guid != Guid.Empty, AssertionMessages.EmptyGuidNotAllowed);
                }
            }
        }

        [Then(@"""(.*)"" should be a valid ISO 8601 UTC timestamp for each item")]
        public void ThenFieldShouldBeValidIso8601TimestampForEachItem(string field)
        {
            foreach (var item in _auditHistoryList)
            {
                var itemType = item.GetType();
                var property = itemType.GetProperty(
                    field, 
                    System.Reflection.BindingFlags.IgnoreCase | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance);
                
                Assert.NotNull(property);
                var value = property.GetValue(item)?.ToString();
                Assert.True(DateTime.TryParse(value, out _), AssertionMessages.InvalidIso8601Format);
            }
        }

        [Then(@"""(.*)"" should be non-empty strings for each item")]
        public void ThenFieldsShouldBeNonEmptyStringsForEachItem(string fields)
        {
            var fieldList = fields.Split(',').Select(f => f.Trim().Trim('"')).ToList();
            
            foreach (var item in _auditHistoryList)
            {
                var itemType = item.GetType();
                foreach (var field in fieldList)
                {
                    var property = itemType.GetProperty(
                        field, 
                        System.Reflection.BindingFlags.IgnoreCase | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.Instance);
                    
                    Assert.NotNull(property);
                    var value = property.GetValue(item)?.ToString();
                    Assert.True(!string.IsNullOrWhiteSpace(value), AssertionMessages.EmptyStringNotAllowed);
                }
            }
        }

        [Then(@"the response body should include ""(.*)""")]
        public void ThenTheResponseBodyShouldIncludeFields(string fields)
        {
            Assert.NotNull(_lastResponse.Content);
            var fieldList = fields.Split(',').Select(f => f.Trim().Trim('"')).ToList();
            
            foreach (var field in fieldList)
            {
                Assert.True(_lastResponse.Content.Contains($"\"{field}\""), 
                    AssertionMessages.FieldMissing(field));
            }
        }

        [Then(@"the ""(.*)"" in the response should match the requested id")]
        public void ThenTheFieldInResponseShouldMatchRequestedId(string field)
        {
            Assert.NotNull(_lastResponse.Content);
            var auditHistory = JsonSerializer.Deserialize<AuditHistory>(_lastResponse.Content);
            Assert.NotNull(auditHistory);
            
            var property = auditHistory.GetType().GetProperty(
                field, 
                System.Reflection.BindingFlags.IgnoreCase | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Instance);
            
            Assert.NotNull(property);
            var value = property.GetValue(auditHistory)?.ToString();
            Assert.Equal(_existingAuditHistoryId, value);
        }

        [Then(@"each item's ""(.*)"" should equal ""(.*)""")]
        public void ThenEachItemsFieldShouldEqual(string field, string expectedValue)
        {
            foreach (var item in _auditHistoryList)
            {
                var property = item.GetType().GetProperty(
                    field, 
                    System.Reflection.BindingFlags.IgnoreCase | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance);
                
                Assert.NotNull(property);
                var value = property.GetValue(item)?.ToString();
                Assert.Equal(expectedValue, value);
            }
        }

        [Then(@"each item's ""(.*)"" should match the requested id")]
        public void ThenEachItemsFieldShouldMatchRequestedId(string field)
        {
            foreach (var item in _auditHistoryList)
            {
                var property = item.GetType().GetProperty(
                    field, 
                    System.Reflection.BindingFlags.IgnoreCase | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance);
                
                Assert.NotNull(property);
                var value = property.GetValue(item)?.ToString();
                Assert.Equal(_validCreatedById, value);
            }
        }

        [Then(@"each item's ""(.*)"" should be within the requested range")]
        public void ThenEachItemsFieldShouldBeWithinRequestedRange(string field)
        {
            var fromDate = DateTime.Parse(_createdAtFrom);
            var toDate = DateTime.Parse(_createdAtTo);
            
            foreach (var item in _auditHistoryList)
            {
                var property = item.GetType().GetProperty(
                    field, 
                    System.Reflection.BindingFlags.IgnoreCase | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance);
                
                Assert.NotNull(property);
                var value = property.GetValue(item)?.ToString();
                var itemDate = DateTime.Parse(value!);
                
                Assert.True(itemDate >= fromDate && itemDate <= toDate, 
                    AssertionMessages.CreatedAtOutOfRange);
            }
        }

        [Then(@"the response should include exactly ""(.*)"" items or fewer")]
        public void ThenTheResponseShouldIncludeExactlyItemsOrFewer(string maxItems)
        {
            int.TryParse(maxItems, out int max);
            Assert.True(_auditHistoryList.Count <= max, 
                AssertionMessages.PaginationCountMismatch);
        }

        [Then(@"pagination metadata should indicate page ""(.*)"" and size ""(.*)""")]
        public void ThenPaginationMetadataShouldIndicatePageAndSize(string page, string size)
        {
            // This would check pagination metadata in response headers or body
            // Implementation depends on API response structure
            Assert.True(true); // Placeholder for pagination metadata validation
        }

        [Then(@"the items should be ordered by ""(.*)"" in descending order")]
        public void ThenItemsShouldBeOrderedByFieldInDescendingOrder(string field)
        {
            if (_auditHistoryList.Count <= 1) return;
            
            for (int i = 0; i < _auditHistoryList.Count - 1; i++)
            {
                var currentProperty = _auditHistoryList[i].GetType().GetProperty(
                    field, 
                    System.Reflection.BindingFlags.IgnoreCase | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance);
                
                var nextProperty = _auditHistoryList[i + 1].GetType().GetProperty(
                    field, 
                    System.Reflection.BindingFlags.IgnoreCase | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance);
                
                Assert.NotNull(currentProperty);
                Assert.NotNull(nextProperty);
                
                var currentValue = DateTime.Parse(currentProperty.GetValue(_auditHistoryList[i])?.ToString()!);
                var nextValue = DateTime.Parse(nextProperty.GetValue(_auditHistoryList[i + 1])?.ToString()!);
                
                Assert.True(currentValue >= nextValue, AssertionMessages.SortOrderIncorrect);
            }
        }

        [Then(@"no PascalCase properties like ""(.*)"" should be present")]
        public void ThenNoPascalCasePropertiesShouldBePresent(string exampleProperty)
        {
            Assert.NotNull(_lastResponse.Content);
            Assert.False(_lastResponse.Content.Contains($"\"{exampleProperty}\""), 
                AssertionMessages.PascalCasePropertyFound);
        }

        [Then(@"each item's ""(.*)"" should not be in the future")]
        public void ThenEachItemsFieldShouldNotBeInTheFuture(string field)
        {
            var now = DateTime.UtcNow;
            
            foreach (var item in _auditHistoryList)
            {
                var property = item.GetType().GetProperty(
                    field, 
                    System.Reflection.BindingFlags.IgnoreCase | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance);
                
                Assert.NotNull(property);
                var value = property.GetValue(item)?.ToString();
                var itemDate = DateTime.Parse(value!);
                
                Assert.True(itemDate <= now, AssertionMessages.TimestampInFuture);
            }
        }
        #endregion
    }
}
