using AutomationTest.Tests.Support;
using Xunit;

// Configure test execution order for all tests in this assembly
[assembly: TestCaseOrderer("AutomationTest.Tests.Support.ScenarioOrderer", "AutomationTest.Tests")]
