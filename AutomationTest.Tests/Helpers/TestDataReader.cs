using System.Text.Json;

namespace AutomationTest.Tests.Helpers
{
    public static class TestDataReader
    {
        private static readonly string TestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");

        public static T LoadTestData<T>(string fileName, string dataKey = null)
        {
            var filePath = Path.Combine(TestDataPath, fileName);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Test data file not found: {fileName}");
            }

            var jsonContent = File.ReadAllText(filePath);

            if (string.IsNullOrEmpty(dataKey))
            {
                return JsonSerializer.Deserialize<T>(jsonContent)!;
            }

            var jsonDocument = JsonSerializer.Deserialize<JsonDocument>(jsonContent)!;
            var dataElement = jsonDocument.RootElement.GetProperty(dataKey);
            return JsonSerializer.Deserialize<T>(dataElement.GetRawText())!;
        }
    }
}
