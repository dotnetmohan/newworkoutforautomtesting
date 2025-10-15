namespace AutomationTest.Tests.Constants
{
    public static class AssertionMessages
    {
        #region Audit API Assertions
        public const string AuditResponseNull = "The audit response should not be null";
        public const string TokenNotGenerated = "Failed to generate access token";
        public const string InvalidAuditResponse = "Invalid audit response format";
        public const string MissingRequiredFields = "Required fields are missing in the response";
        
        // Audit History Assertions
        public const string AuditHistoryResponseNull = "Audit history response should not be null";
        public const string AuditHistoryNotJsonArray = "Audit history response should be a JSON array";
        public const string AuditHistoryEmptyArrayExpected = "Expected an empty JSON array but got data";
        public const string AuditHistoryDataExpected = "Expected audit history data but response is empty";
        public const string ContentTypeInvalid = "Content-Type header is not application/json";
        public const string InvalidGuidFormat = "GUID format is invalid";
        public const string EmptyGuidNotAllowed = "GUID should not be empty";
        public const string InvalidIso8601Format = "Timestamp is not in valid ISO 8601 UTC format";
        public const string TimestampInFuture = "Timestamp should not be in the future";
        public const string EmptyStringNotAllowed = "String field should not be empty";
        public const string PascalCasePropertyFound = "Response should use camelCase, but PascalCase property found";
        public const string AuditHistoryIdMismatch = "Audit history ID in response does not match requested ID";
        public const string TDescriptionMismatch = "Table description does not match filter criteria";
        public const string CreatedByMismatch = "CreatedBy value does not match filter criteria";
        public const string CreatedAtOutOfRange = "CreatedAt timestamp is outside the requested range";
        public const string PaginationCountMismatch = "Response contains more items than requested page size";
        public const string PaginationMetadataMissing = "Pagination metadata is missing or incorrect";
        public const string SortOrderIncorrect = "Items are not sorted in the expected order";
        
        public static string FieldMissing(string fieldName) =>
            $"Required field '{fieldName}' is missing from the response";
        
        public static string InvalidDateRange(string from, string to) =>
            $"Invalid date range: 'from' ({from}) is after 'to' ({to})";
        #endregion
        
        #region Status Code Assertions
        public static string StatusCodeMismatch(int expected, int actual) => 
            $"Expected status code {expected} but received {actual}";
        #endregion

        #region Products List Assertions
        public const string ProductsResponseNull = "The products response object should not be null";
        public const string ProductsListNull = "The products list in the response should not be null";
        public const string ProductsListEmpty = "The products list should not be empty";
        #endregion

        #region Product Details Assertions
        public const string ProductResponseNull = "The product response object should not be null";
        public static string ProductIdInvalid(int id) => 
            $"Product ID should be greater than 0, but got {id}";
        public const string ProductTitleEmpty = "Product title should not be null or empty";
        public const string ProductDescriptionEmpty = "Product description should not be null or empty";
        #endregion

        #region Search Results Assertions
        public const string SearchResponseNull = "The search response object should not be null";
        public const string SearchResultsNull = "The search results list should not be null";
        public const string SearchResultsEmpty = "The search results list should not be empty";
        public const string NoMatchingProducts = "Search results should contain at least one product with 'phone' in title or description";
        #endregion

        #region User API Assertions
        public const string UserResponseNull = "The user response should not be null";
        public const string UserDataMissing = "User data is missing in the response";
        #endregion
    }
}
