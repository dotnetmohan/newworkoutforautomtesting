using AutomationTest.Core.Api;
using AutomationTest.Core.Models;
using AutomationTest.Core.Services;
using AutomationTest.Tests.Constants;
using Reqnroll;
using RestSharp;

namespace AutomationTest.Tests.Steps
{
    [Binding]
    public class UserStepDefinitions
    {
        private readonly UserApiClient _client;
        private readonly LoggerService _logger;
        private UserResponse _userResponse = null!;
        private RestResponse _lastResponse = null!;

        public UserStepDefinitions()
        {
            _client = new UserApiClient();
            _logger = LoggerService.Instance;
        }

        [When(@"I request user data")]
        public async Task WhenIRequestUserData()
        {
            try
            {
                _userResponse = await _client.GetUserDataAsync();
                _lastResponse = _client.GetLastResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user data: {ErrorMessage}", ex.Message);
            }
        }

        [Then(@"the user response should be valid")]
        public void ThenTheUserResponseShouldBeValid()
        {
            Assert.True(_userResponse != null, AssertionMessages.UserResponseNull);
            Assert.True(_userResponse.User != null, AssertionMessages.UserDataMissing);
            Assert.False(string.IsNullOrEmpty(_userResponse.User.Id), "User Id should not be empty");
        }
    }
}
