using System.Text.RegularExpressions;
using Reqnroll;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AutomationTest.Tests.Support
{
    /// <summary>
    /// Custom test orderer to execute scenarios based on @order:N tags
    /// </summary>
    public class ScenarioOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            var orderedTests = testCases.Select(tc => new
            {
                TestCase = tc,
                Order = GetOrderFromDisplayName(tc.DisplayName)
            })
            .OrderBy(x => x.Order)
            .ThenBy(x => x.TestCase.DisplayName)
            .Select(x => x.TestCase);

            return orderedTests;
        }

        private int GetOrderFromDisplayName(string displayName)
        {
            // Look for @order:N pattern in the display name
            var match = Regex.Match(displayName, @"@order:(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int order))
            {
                return order;
            }

            // Default to high number so unordered tests run last
            return int.MaxValue;
        }
    }
}
